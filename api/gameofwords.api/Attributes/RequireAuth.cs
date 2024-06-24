using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using gameofwords.common.Services;
using Gameofwords.Common;

namespace gameofwords.api.Attributes
{
    public class RequireAuth : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context, ActionExecutionDelegate next )
        {
            var auth = context.HttpContext.RequestServices.GetService<IAuthService>( );
            string SessionId = context.HttpContext.Request.Cookies["gameofwords.session"] ?? string.Empty;

            var result = await auth.CallServiceAsync( async client =>
                        await client.CheckAsync( new Gameofwords.Common.DataRequest( ) { SessionId = SessionId } ) );

            if(result.Error!=null)
            {
                context.Result = new ObjectResult( result.Error.Message );
                context.HttpContext.Response.StatusCode = result.Error.Code;
                return;
            }

            await base.OnActionExecutionAsync( context, next );
        }
    }

    public class RequireAdmin : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context, ActionExecutionDelegate next )
        {
            var auth = context.HttpContext.RequestServices.GetService<IAuthService>( );
            string SessionId = context.HttpContext.Request.Cookies["gameofwords.session"] ?? string.Empty;

            var result = await auth.CallServiceAsync( async client =>
                        await client.CheckAdminAsync( new Gameofwords.Common.DataRequest( ) { SessionId = SessionId } ) );

            if(result.Error!=null)
            {
                context.Result = new ObjectResult( result.Error.Message );
                context.HttpContext.Response.StatusCode = result.Error.Code;
                return;
            }

            await base.OnActionExecutionAsync( context, next );
        }
    }
}