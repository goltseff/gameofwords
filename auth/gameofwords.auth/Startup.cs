using gameofwords.auth.DataLayer;
using gameofwords.auth.Services;
using gameofwords.common.config;
using gameofwords.common.EventContracts;
using gameofwords.common.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace gameofwords.auth
{
    public class Startup
    {
        public void ConfigureServices( IServiceCollection services )
        {
            AppContext.SetSwitch( "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true );
            AppContext.SetSwitch( "System.Net.Http.SocketsHttpHandler.Http2Support", true );

            services.AddGrpc( );
            services.AddSingleton<ILogger, Logger<SessionService>>( );
            services.AddSingleton<ISessionService, SessionService>( );
            services.AddHostedService<SessionKiller>( );
            services.AdServiceUsers( );
            services.AddDbContext<PgDbContext>( options =>
                    options.UseNpgsql( Config.DbConnection ) );

            services.AddMassTransit( x =>
            {
                x.UsingRabbitMq( ( context, cfg ) =>
                {
                    cfg.Host( Config.Rabbit.Host, "/", h => {
                        h.Username( Config.Rabbit.Username );
                        h.Password( Config.Rabbit.Password );
                    } );
                    cfg.ConfigureEndpoints( context );
                } );
            } );
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if(env.IsDevelopment( ))
            {
                app.UseDeveloperExceptionPage( );
            }


            app.UseRouting( );

            app.UseEndpoints( endpoints =>
            {
                endpoints.MapGrpcService<gameofwords.auth.Services.AuthService>( );
            } );
        }

    }
}
