using gameofwords.common.config;
using gameofwords.common.Services;
using gameofwords.game.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace gameofwords.game
{
    public class Startup
    {
        public void ConfigureServices( IServiceCollection services )
        {
            AppContext.SetSwitch( "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true );
            AppContext.SetSwitch( "System.Net.Http.SocketsHttpHandler.Http2Support", true );

            services.AddGrpc( );
            services.AddSingleton<ILogger, Logger<GameService>>( );
            services.AdServiceAuth( );
            services.AddDbContext<PgDbContext>( options =>
                    options.UseNpgsql( Config.DbConnection ) );
        }

        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if(env.IsDevelopment( ))
            {
                app.UseDeveloperExceptionPage( );
            }


            app.UseRouting( );

            app.UseEndpoints( endpoints =>
            {
                endpoints.MapGrpcService<gameofwords.game.Services.GameService>( );
            } );
        }

    }
}
