using gameofwords.auth.DataLayer;
using gameofwords.service;
using Gameofwords.Common;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using gameofwords.common.tools;
using Newtonsoft.Json;
using gameofwords.common.Services;
using System.Web;
using gameofwords.common.config;
using MassTransit;
using gameofwords.auth.AuthContracts;
using gameofwords.common.EventContracts;

namespace gameofwords.auth.Services
{
    public class AuthService : gameofwords.service.AuthService.AuthServiceBase
    {
        private readonly ILogger _logger;
        private readonly ISessionService _sessionService;
        private readonly PgDbContext _dbContext;
        private readonly IUsersService _users;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuthService( ILogger logger, ISessionService sessionService, PgDbContext dbContext, IUsersService users, IPublishEndpoint publishEndpoint )
        {
            _logger = logger;
            _sessionService = sessionService;
            _dbContext = dbContext;
            _users = users;
            _publishEndpoint=publishEndpoint;
        }


        public override async Task<LoginResponse> Login( service.LoginRequest request, ServerCallContext context )
        {
            var userData = await (from u in _dbContext.Users
                                  where u.Login == request.Login
                              select new 
                              {
                                  u.Id,
                                  u.Password,
                                  u.IsAdmin
                              }
            ).FirstOrDefaultAsync( );
            if(userData!=null)
            {
                var pwdHash = PasswordHash.GetPasswordHash( request.Password, userData.Id.ToString( ) );

                if(userData.Password == pwdHash)
                {
                    var sessionId = _sessionService.CreateSession( userData.Id, userData.IsAdmin );
                    await NotifyAuthAttemptAsync( userData.Id, true );
                    return new LoginResponse( ) { Error=null, SessionId=sessionId.ToString( ) };
                }
                else
                {
                    await NotifyAuthAttemptAsync( userData.Id, false );
                }
            }
            return new LoginResponse( ) { Error=new Error { Code=500, Message="wrong login or password" } };
        }

        public override async Task<SessionsListResponse> SessionsList( DataRequest request, ServerCallContext context )
        {
            var result = new SessionsListResponse( );
            var data = _sessionService.GetSessionList( ).Select(
                x => new SessionsListItem
                {
                    UserId = x.Item2,
                    SessionId = x.Item1.ToString( ),
                    DateTime = Timestamp.FromDateTime( DateTime.SpecifyKind( x.Item3, DateTimeKind.Utc ) )
                } );

            result.Data.AddRange( data );
            return await Task.FromResult(result);

        }

        public override async Task<BoolResponse> Logout( DataRequest request, ServerCallContext context )
        {
            if(Guid.TryParse( request.SessionId, out var guid ))
            {
                _sessionService.CloseSession( guid );
            }
            return await Task.FromResult( new BoolResponse( ) );
        }

        public override async Task<BoolResponse> Check( DataRequest request, ServerCallContext context )
        {
            if(!string.IsNullOrEmpty( request.SessionId ) && Guid.TryParse( request.SessionId, out var guid ))
            {
                if(_sessionService.CheckSession( guid ))
                {
                    _sessionService.RefreshSession( guid );
                    return await Task.FromResult( new BoolResponse( ) );
                }
            }
            return await Task.FromResult( new BoolResponse( ) { Error = new Error { Code=401, Message="unauthorized" } } );
        }

        public override async Task<BoolResponse> CheckAdmin( DataRequest request, ServerCallContext context )
        {
            if(!string.IsNullOrEmpty( request.SessionId ) && Guid.TryParse( request.SessionId, out var guid ))
            {
                if(_sessionService.CheckAdmin( guid ))
                {
                    _sessionService.RefreshSession( guid );
                    return await Task.FromResult( new BoolResponse( ) );
                }
            }
            return await Task.FromResult( new BoolResponse( ) { Error = new Error { Code=403, Message="forbidden" } } );
        }

        public override async Task<StringResponse> GetCurrentUserLogin( DataRequest request, ServerCallContext context )
        {
            if(!string.IsNullOrEmpty( request.SessionId ) && Guid.TryParse( request.SessionId, out var guid ))
            {
                var userId = _sessionService.GetUserId( guid );
                var user = await _dbContext.Users.Where( x => x.Id == userId ).FirstOrDefaultAsync( );
                return new StringResponse { Data=user.Login };
            }
            return new StringResponse( );
        }

        public override async Task<IdResponse> GetCurrentUserId( DataRequest request, ServerCallContext context )
        {
            if(!string.IsNullOrEmpty( request.SessionId ) && Guid.TryParse( request.SessionId, out var guid ))
            {
                var userId = _sessionService.GetUserId( guid );
                return await Task.FromResult( new IdResponse { Data=userId.Value } );
            }
            return await Task.FromResult( new IdResponse( ) );
        }

        public override async Task<LoginResponse> CheckVK( StringRequest request, ServerCallContext context )
        {
            try
            {
                var clientId = Config.GetServiceParam( IAuthService.ServiceName, "vk-client-id" );
                var clientSecret = Config.GetServiceParam( IAuthService.ServiceName, "vk-client-secret" ); ;
                var redirectUrl = Config.GetServiceParam( IAuthService.ServiceName, "vk-redirect-url" );

                string url = $"https://oauth.vk.com/access_token?client_id={clientId}&client_secret={clientSecret}&redirect_uri={redirectUrl}&code={request.Data}&v=5.199";
                var client = new HttpClient( );
                var content = await client.GetStringAsync( url );
                var vkAuth = JsonConvert.DeserializeObject<VkAuth>( content );

                var vkUser = await _dbContext.Users.Where( x => x.Login==$"{vkAuth.user_id}@VK" ).FirstOrDefaultAsync( );
                var vkUserId = vkUser!=null ? vkUser.Id : 0;
                if(vkUser == null)
                {
                    url = $"https://api.vk.com/method/users.get?uids={vkAuth.user_id}&fields=first_name,last_name&access_token={vkAuth.access_token}&v=5.199";
                    content = await client.GetStringAsync( url );
                    var vkData = JsonConvert.DeserializeObject<VkData>( content );

                    var createRequest = new UserRequest( );
                    createRequest.Login = $"{vkAuth.user_id}@VK";
                    createRequest.IsBot = false;
                    createRequest.IsAdmin = false;
                    createRequest.NickName = $"{vkData.Response[0].first_name} {vkData.Response[0].last_name} (VK)";
                    var result = await _users.CallServiceAsync( async client =>
                        await client.CreateUserAsync( createRequest ) );
                    vkUserId = result.Data;
                }
                var sessionId = _sessionService.CreateSession( vkUserId, false );
                return new LoginResponse( ) { Error=null, SessionId=sessionId.ToString( ) };
            }
            catch(Exception e)
            {
                return new LoginResponse( ) { Error=new Error { Code=500, Message=e.Message } };
            }
        }

        public override async Task<LoginResponse> CheckGoogle( StringRequest request, ServerCallContext context )
        {
            try
            {
                var clientId = Config.GetServiceParam( IAuthService.ServiceName, "google-client-id" );
                var clientSecret = Config.GetServiceParam( IAuthService.ServiceName, "google-client-secret" ); ;
                var redirectUrl = Config.GetServiceParam( IAuthService.ServiceName, "google-redirect-url" );

                var postData = new FormUrlEncodedContent( [
                    new KeyValuePair<string, string>("code", HttpUtility.UrlDecode( request.Data)),
                new KeyValuePair<string, string>("client_id",clientId),
                new KeyValuePair<string, string>("client_secret",clientSecret),
                new KeyValuePair<string, string>("redirect_uri",redirectUrl),
                new KeyValuePair<string, string>("grant_type","authorization_code")
                ] );


                string url = $"https://accounts.google.com/o/oauth2/token";
                var client = new HttpClient( );
                var postResult = await client.PostAsync( url, postData );
                string content = await postResult.Content.ReadAsStringAsync( );
                var googleAuth = JsonConvert.DeserializeObject<GoogleAuth>( content );

                url = $"https://www.googleapis.com/oauth2/v1/userinfo?access_token={googleAuth.access_token}&id_token={googleAuth.id_token}&token_type=Bearer&expires_in=3599";
                content = await client.GetStringAsync( url );
                var googleData = JsonConvert.DeserializeObject<GoogleData>( content );

                var googleUser = await _dbContext.Users.Where( x => x.Login==$"{googleData.id}@Google" ).FirstOrDefaultAsync( );
                var googleUserId = googleUser!=null ? googleUser.Id : 0;
                if(googleUser==null)
                {
                    var createRequest = new UserRequest( );
                    createRequest.Login = $"{googleData.id}@Google";
                    createRequest.IsBot = false;
                    createRequest.IsAdmin = false;
                    createRequest.NickName = $"{googleData.name} (Google)";
                    var result = await _users.CallServiceAsync( async client =>
                        await client.CreateUserAsync( createRequest ) );
                    googleUserId = result.Data;
                }
                var sessionId = _sessionService.CreateSession( googleUserId, false );
                return new LoginResponse( ) { Error=null, SessionId=sessionId.ToString( ) };
            }catch(Exception e)
            {
                return new LoginResponse( ) { Error=new Error { Code=500, Message=e.Message} };
            }
        }

        public override async Task<LoginResponse> BotLogin( IdRequest request, ServerCallContext context )
        {
            var sessionId = _sessionService.CreateSession( request.Id, false );
            await NotifyAuthAttemptAsync( request.Id, true );
            return await Task.FromResult(new LoginResponse( ) { Error=null, SessionId=sessionId.ToString( ) });
        }

        private async Task NotifyAuthAttemptAsync (int userId, bool isSuccess )
        {
            await _publishEndpoint.Publish( new AuthAttemptEvent
            {
                IsSuccess = isSuccess,
                UserId = userId
            } );
        }

    }
}
