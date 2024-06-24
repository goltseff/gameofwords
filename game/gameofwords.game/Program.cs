using gameofwords.common.config;
using gameofwords.game;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

internal class Program
{
    public static void Main( string[] args )
    {
        CreateHostBuilder( args ).Build( ).Run( );
    }

    public static IHostBuilder CreateHostBuilder( string[] args )
    {
        Config.GetConfig( args );
        var serviceUrl = Config.GetServiceUrl( "game" );

        return Host.CreateDefaultBuilder( args )
            .ConfigureWebHostDefaults( webBuilder =>
            {
                webBuilder.UseStartup<Startup>( );
                webBuilder.UseUrls( urls: serviceUrl );
            } );
    }
}

