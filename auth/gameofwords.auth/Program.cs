using System.Net;
using gameofwords.auth;
using gameofwords.common.config;
using Microsoft.AspNetCore.Server.Kestrel.Core;

internal class Program
{
    public static void Main( string[] args )
    {
        CreateHostBuilder( args ).Build( ).Run( );
    }

    public static IHostBuilder CreateHostBuilder( string[] args )
    {
        Config.GetConfig( args );
        var serviceUrl = Config.GetServiceUrl( "auth" );

        return Host.CreateDefaultBuilder( args )
            .ConfigureWebHostDefaults( webBuilder =>
            {
                webBuilder.UseStartup<Startup>( );
                webBuilder.UseUrls( urls: serviceUrl );
            } );
    }

}