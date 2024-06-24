using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using gameofwords.service;
using gameofwords.common.Services;
using Grpc.Net.Client;
using System.Xml.Linq;
using gameofwords.common.config;
using Gameofwords.Common;
using gameofwords.api.Attributes;

namespace gameofwords.api.controllers
{
    [ApiController]
    [Produces( MediaTypeNames.Application.Json )]
    [Consumes( MediaTypeNames.Application.Json )]
    [Route( "api/v1/auth" )]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _auth;
        private string SessionId => Request.Cookies["gameofwords.session"] ?? string.Empty;
        private DataRequest RequestContext => new( ) { SessionId = SessionId };

        public AuthController( ILogger<AuthController> logger, IAuthService auth )
        {
            _logger = logger;
            _auth = auth;
        }

        [HttpPost]
        [Route( "login" )]
        public async Task<IActionResult> Login( [FromBody] LoginRequest request )
        {
            var result = await _auth.CallServiceAsync( async client =>
                    await client.LoginAsync( request ) );

            if (result.Error!= null)
            {
                Response.Cookies.Delete( "gameofwords.session" );
                return StatusCode( result.Error.Code, result.Error.Message );
            }
            Response.Cookies.Append( "gameofwords.session", result.SessionId, new CookieOptions { HttpOnly = true } );
            return Ok( );
        }

        [HttpGet]
        [Route( "sessions-list" )]
        [RequireAuth]
        public async Task<IActionResult> SessionsList( )
        {
            var result = await _auth.CallServiceAsync( async client =>
                    await client.SessionsListAsync( RequestContext ) );

            var resultData = result.Data.Select( x => new { x.UserId, x.SessionId, date = x.DateTime.ToDateTimeOffset( ) } );

            return Ok( resultData );
        }

        [HttpGet]
        [Route( "logout" )]
        public async Task<IActionResult> Logout( )
        {
            var result = await _auth.CallServiceAsync( async client =>
                    await client.LogoutAsync( RequestContext ) );

            return Ok( );
        }

        [HttpGet]
        [Route( "check" )]
        public async Task<IActionResult> Check( )
        {
            var result = await _auth.CallServiceAsync( async client =>
                    await client.CheckAsync( RequestContext ) );

            if(result.Error!= null)
            {
                return StatusCode( result.Error.Code, result.Error.Message );
            }
            return Ok( );
        }

        [HttpGet]
        [Route( "check-vk/{code}" )]
        public async Task<IActionResult> CheckVK( string code )
        {
            var result = await _auth.CallServiceAsync( async client =>
                    await client.CheckVKAsync( new StringRequest { Data = code } ) );

            if(result.Error!= null)
            {
                Response.Cookies.Delete( "gameofwords.session" );
                return StatusCode( result.Error.Code, result.Error.Message );
            }
            Response.Cookies.Append( "gameofwords.session", result.SessionId, new CookieOptions { HttpOnly = true } );
            return Ok( );
        }

        [HttpPost]
        [Route( "check-google" )]
        public async Task<IActionResult> CheckGoogle([FromBody] CheckGoogleRequest request )
        {
            var result = await _auth.CallServiceAsync( async client =>
                    await client.CheckGoogleAsync( new StringRequest { Data = request.code } ) );

            if(result.Error!= null)
            {
                Response.Cookies.Delete( "gameofwords.session" );
                return StatusCode( result.Error.Code, result.Error.Message );
            }
            Response.Cookies.Append( "gameofwords.session", result.SessionId, new CookieOptions { HttpOnly = true } );
            return Ok( );
        }
        public class CheckGoogleRequest
        {
            public string code { get; set; }
        }
    }
}
