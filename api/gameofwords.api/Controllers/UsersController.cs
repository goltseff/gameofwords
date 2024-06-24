using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using gameofwords.service;
using gameofwords.common.Services;
using Grpc.Net.Client;
using System.Xml.Linq;
using gameofwords.common.config;
using Gameofwords.Common;
using gameofwords.api.Attributes;
using Newtonsoft.Json;

namespace gameofwords.api.controllers
{
    [ApiController]
    [Produces( MediaTypeNames.Application.Json )]
    [Consumes( MediaTypeNames.Application.Json )]
    [Route( "api/v1/users" )]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUsersService _users;
        private string SessionId => Request.Cookies["gameofwords.session"] ?? string.Empty;
        private DataRequest RequestContext => new( ) { SessionId = SessionId };



        public UsersController( ILogger<UsersController> logger, IUsersService users )
        {
            _logger = logger;
            _users = users;
        }

        [HttpPost]
        [Route( "list" )]
        [RequireAdmin]
        public async Task<IActionResult> List( [FromBody] UsersListRequest request )
        {
            var result = await _users.CallServiceAsync( async client =>
                    await client.GetUsersListAsync( request ) );

            return Ok( result.Data );
        }

        [HttpPost]
        [Route( "list/count" )]
        [RequireAdmin]
        public async Task<IActionResult> ListCount( [FromBody] UsersListRequest request )
        {
            var result = await _users.CallServiceAsync( async client =>
                    await client.GetUsersListCountAsync( request ) );

            return Ok( result.Data );
        }

        [HttpGet]
        [Route( "{id}" )]
        [RequireAdmin]
        public async Task<IActionResult> UserGet( int id )
        {
            var result = await _users.CallServiceAsync( async client =>
                    await client.GetUserAsync( new IdRequest { Id = id } ) );

            return Ok( result.Data );
        }

        [HttpPost]
        [Route( "create" )]
        [RequireAdmin]
        public async Task<IActionResult> Create( [FromBody] UserRequest request )
        {
            request.Context = RequestContext;
            var result = await _users.CallServiceAsync( async client =>
                    await client.CreateUserAsync( request ) );

            if(result.Error!= null)
            {
                return StatusCode( result.Error.Code, result.Error.Message );
            }
            return Ok( result.Data );
        }

        [HttpPut]
        [Route( "{id}/update" )]
        [RequireAdmin]
        public async Task<IActionResult> Update( int id, [FromBody] UserRequest request )
        {
            request.Id = id;
            request.Context = RequestContext;
            var result = await _users.CallServiceAsync( async client =>
                    await client.UpdateUserAsync( request ) );

            if(result.Error!= null)
            {
                return StatusCode( result.Error.Code, result.Error.Message );
            }
            return Ok( );
        }

        [HttpPut]
        [Route( "{id}/update-password" )]
        [RequireAdmin]
        public async Task<IActionResult> UpdatePassword( int id, [FromBody] UserPasswordRequest request )
        {
            request.Id = id;
            request.Context = RequestContext;
            var result = await _users.CallServiceAsync( async client =>
                    await client.UpdateUserPasswordAsync( request ) );

            if(result.Error!= null)
            {
                return StatusCode( result.Error.Code, result.Error.Message );
            }
            return Ok( );
        }

        [HttpDelete]
        [Route( "{id}/delete" )]
        [RequireAdmin]
        public async Task<IActionResult> Delete( int id )
        {
            IdRequest request = new IdRequest();
            request.Id = id;
            request.Context = RequestContext;
            var result = await _users.CallServiceAsync( async client =>
                    await client.DeleteUserAsync( request ) );

            if(result.Error!= null)
            {
                return StatusCode( result.Error.Code, result.Error.Message );
            }
            return Ok( );
        }

        [HttpGet]
        [Route( "{id}/history" )]
        [RequireAdmin]
        public async Task<IActionResult> History( int id )
        {
            IdRequest request = new IdRequest( );
            request.Id = id;
            request.Context = RequestContext;
            var result = await _users.CallServiceAsync( async client =>
                    await client.GetUserHistoryAsync( request ) );
            var resultData = result.Data.Select( x => new { x.Action, x.Message, date = x.Datetime.ToDateTimeOffset( ), x.Who } );

            return Ok( resultData );
        }

        [HttpGet]
        [Route( "profile" )]
        [RequireAuth]
        public async Task<IActionResult> Profile( )
        {
            IdRequest request = new IdRequest( );
            request.Context = RequestContext;
            var result = await _users.CallServiceAsync( async client =>
                    await client.GetUserProfileAsync( RequestContext ) );
            var resultData = new { result.Data.Id, result.Data.Login, result.Data.NickName, result.Data.IsAdmin };

            return Ok( resultData );
        }

        /*
        [HttpGet]
        [Route( "import" )]
        public async Task<IActionResult> Import( )
        {
            
            var client = new System.Net.Http.HttpClient( );
            var content = await client.GetStringAsync( @"https://randomuser.me/api/?results=100&nat=US" );
            var json = JsonConvert.DeserializeObject<Root>( content );
            foreach(var item in json.results)
            {
                var request = new UserRequest( );
                request.Context = RequestContext;
                request.Email = item.email;
                request.NickName = $"{item.name.title} {item.name.first} {item.name.last}";
                request.Login = item.login.username;
                request.IsBot = true;
                var result = await _users.CallServiceAsync( async client =>
                            await client.CreateUserAsync( request ) );

            }
            return Ok( );
        }
            */
    }
}
