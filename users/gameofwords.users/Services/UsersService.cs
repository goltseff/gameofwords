using gameofwords.common.Services;
using gameofwords.common.tools;
using gameofwords.service;
using gameofwords.users.DataLayer;
using gameofwords.users.Models;
using Gameofwords.Common;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace gameofwords.users.Services
{
    public class UsersService : gameofwords.service.UsersService.UsersServiceBase
    {
        private readonly ILogger _logger;
        private readonly PgDbContext _dbContext;
        private readonly IAuthService _auth;


        public UsersService( ILogger logger,  PgDbContext dbContext, IAuthService auth )
        {
            _logger = logger;
            _dbContext = dbContext;
            _auth = auth;
        }

        public override async Task<UserResponse> GetUser( IdRequest request, ServerCallContext context )
        {
            var data = await (from u in _dbContext.Users
                              where u.Id == request.Id
                              select new UsersListItem
                              {
                                  Id = u.Id,
                                  Email = u.Email,
                                  Login = u.Login,
                                  NickName = u.Nickname,
                                  IsAdmin = u.IsAdmin,
                                  IsBot = u.IsBot,
                                  UpdatedBy = u.UpdatedBy,
                              }
            ).FirstOrDefaultAsync( );

            var reply = new UserResponse( ) { Data = data };
            if(data is null)
            {
                reply.Error = new Error( ) { Code=404, Message="user not found" };
            }
            return reply;
        }

        public override async Task<IdResponse> GetUsersListCount( UsersListRequest request, ServerCallContext context )
        {
            var data = await (from u in _dbContext.Users
                              select 1
            ).CountAsync( );

            return new IdResponse( ) { Data = data };
        }


        public override async Task<UsersListResponse> GetUsersList( UsersListRequest request, ServerCallContext context )
        {
            var data = await (from u in _dbContext.Users
                              orderby u.Id descending
                              select new UsersListItem
                              {
                                  Id = u.Id,
                                  Email = u.Email,
                                  Login = u.Login,
                                  NickName = u.Nickname,
                                  IsAdmin = u.IsAdmin,
                                  IsBot = u.IsBot,
                                  UpdatedBy = u.UpdatedBy,
                              }
            )
            .Skip( request.Skip )
            .Take( request.Take )
            .ToListAsync( );

            var reply = new UsersListResponse( );
            if(data != null) reply.Data.AddRange( data );

            return reply;
        }

        public override async Task<IdResponse> CreateUser( UserRequest request, ServerCallContext context )
        {
            Error err = null;
            if(err==null)
            {
                var checkUnickNick = await _dbContext.Users.AnyAsync( x => x.Nickname == request.NickName );
                if(checkUnickNick)
                {
                    err = new Error { Code=500, Message="user nick name must be uniq" };
                }
            }
            if(err != null)
            {
                return new IdResponse { Error = err };
            }

            using var transaction = _dbContext.Database.BeginTransaction( );
            try
            {
                var currentLogin = "unknown";
                if(request.Context!= null && !string.IsNullOrEmpty( request.Context.SessionId ))
                {
                    var currentLoginData = await _auth.CallServiceAsync( async client =>
                        await client.GetCurrentUserLoginAsync( request.Context ) );
                    currentLogin = currentLoginData.Data;
                }

                var user = new Users( )
                {
                    Email = request.Email,
                    Login = request.Login,
                    Nickname = request.NickName,
                    IsAdmin = request.IsAdmin,
                    IsBot = request.IsBot,
                    UpdatedBy = currentLogin
                };
                await _dbContext.Users.AddAsync( user );
                await _dbContext.SaveChangesAsync( );

                var userId = user.Id;

                user.Password = PasswordHash.GetPasswordHash( request.Password, userId.ToString( ) );
                await _dbContext.SaveChangesAsync( );
                await transaction.CommitAsync( );
                return new IdResponse { Data = userId };
            }
            catch(Exception e)
            {
                await transaction.RollbackAsync( );
                return new IdResponse { Error = new Error { Code=500, Message = e.Message } };
            }
        }

        public override async Task<BoolResponse> UpdateUser( UserRequest request, ServerCallContext context )
        {
            try
            {
                Error err = null;
                var user = await _dbContext.Users.Where( x => x.Id == request.Id ).FirstOrDefaultAsync( );
                if(user==null) { 
                    err = new Error { Code=404, Message="user not found" }; 
                }
                if(err==null && request.Login!=user.Login)
                {
                    var checkUnickLogin = await _dbContext.Users.AnyAsync( x => x.Login == request.Login );
                    if(checkUnickLogin)
                    {
                        err = new Error { Code=500, Message="user login must be uniq" };
                    }
                }
                if(err==null && request.NickName!=user.Nickname)
                {
                    var checkUnickNick = await _dbContext.Users.AnyAsync( x => x.Nickname == request.NickName );
                    if(checkUnickNick)
                    {
                        err = new Error { Code=500, Message="user nick name must be uniq" };
                    }
                }
                if (err != null)
                {
                    return new BoolResponse { Error = err };
                }
                var currentLogin = await _auth.CallServiceAsync( async client =>
                    await client.GetCurrentUserLoginAsync( request.Context ) );


                user.IsBot = request.IsBot;
                user.IsAdmin = request.IsAdmin;
                user.Login = request.Login;
                user.Email = request.Email;
                user.Nickname = request.NickName;
                user.UpdatedBy = currentLogin.Data;

                await _dbContext.SaveChangesAsync( );

            }
            catch(Exception e)
            {
                return new BoolResponse { Error = new Error { Code=500, Message = e.Message } };
            }
            return new BoolResponse( );
        }

        public override async Task<BoolResponse> UpdateUserPassword( UserPasswordRequest request, ServerCallContext context )
        {
            Error err = null;
            var user = await _dbContext.Users.Where( x => x.Id == request.Id ).FirstOrDefaultAsync( );
            if(user==null)
            {
                err = new Error { Code=404, Message="user not found" };
            }

            var currentLogin = await _auth.CallServiceAsync( async client =>
                await client.GetCurrentUserLoginAsync( request.Context ) );

            user.Password = PasswordHash.GetPasswordHash( request.Password, request.Id.ToString( ) );
            user.UpdatedBy = currentLogin.Data;
            await _dbContext.SaveChangesAsync( );

            return new BoolResponse( );
        }


        public override async Task<BoolResponse> DeleteUser( IdRequest request, ServerCallContext context )
        {
            using var transaction = _dbContext.Database.BeginTransaction( );
            var currentLogin = await _auth.CallServiceAsync( async client =>
                await client.GetCurrentUserLoginAsync( request.Context ) );

            try
            {
                var user = new Users { Id = request.Id };
                user.UpdatedBy = currentLogin.Data;
                await _dbContext.SaveChangesAsync( );
                _dbContext.Entry( user ).State = EntityState.Deleted;
                await _dbContext.SaveChangesAsync( );
                await transaction.CommitAsync();
            }
            catch(Exception e)
            {
                transaction.Rollback();
                return new BoolResponse { Error = new Error { Code=500, Message = e.Message } };
            }
            return new BoolResponse( );
        }

        public override async Task<UserHistoryResponse> GetUserHistory( IdRequest request, ServerCallContext context )
        {
            var data = await (from u in _dbContext.UsersHistory
                              where u.UserId == request.Id
                              orderby u.Datetime
                              select new UserHistoryItem
                              {
                                  Action = u.Action,
                                  Message = u.Message,
                                  Datetime = Timestamp.FromDateTime( DateTime.SpecifyKind( u.Datetime, DateTimeKind.Unspecified ) ),
                                  Who = u.Who
                              }
            )
            .ToListAsync( );

            var reply = new UserHistoryResponse( );
            if(data != null) reply.Data.AddRange( data );

            return reply;
        }

        public override async Task<UserResponse> GetUserProfile( DataRequest request, ServerCallContext context )
        {
            var currentUserId = await _auth.CallServiceAsync( async client =>
                await client.GetCurrentUserIdAsync( request ) );

            return await GetUser( new IdRequest { Id = currentUserId.Data, Context = request }, context );
        }

    }
}




