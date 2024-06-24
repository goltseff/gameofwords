using System.Net;
using gameofwords.common.config;
using gameofwords.common.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace gameofwords.api
{

    internal class Program
    {
        /*
        private static void Main( string[] args )
        {
            Config.GetConfig( args );

            var builder = WebApplication.CreateBuilder( args );

            // Add services to the container.
            AppContext.SetSwitch( "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true );
            builder.Services.AddGrpc( );
            builder.Services.AddControllers( );
            builder.Services.AddLogging( );
            builder.Services.AdServiceAuth( );
            var app = builder.Build( );

            app.MapControllers( );

            app.Run( Config.GetServiceUrl( "api" ) );
        }*/
        public static void Main( string[] args )
        {
            CreateHostBuilder( args ).Build( ).Run( );
        }

        public static IHostBuilder CreateHostBuilder( string[] args )
        {
            Config.GetConfig( args );
            var serviceUrl = Config.GetServiceUrl( "api" );

            return Host.CreateDefaultBuilder( args )
                .ConfigureWebHostDefaults( webBuilder =>
                {
                    webBuilder.UseStartup<Startup>( );
                    webBuilder.UseUrls( urls: serviceUrl );
                } );
        }
    }

}