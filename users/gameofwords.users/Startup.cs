using gameofwords.common.config;
using gameofwords.common.EventContracts;
using gameofwords.common.Services;
using gameofwords.users.Consumers;
using gameofwords.users.DataLayer;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace gameofwords.users
{
    public class Startup
    {
        public void ConfigureServices( IServiceCollection services )
        {
            AppContext.SetSwitch( "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true );
            AppContext.SetSwitch( "System.Net.Http.SocketsHttpHandler.Http2Support", true );

            services.AddGrpc( );
            services.AddSingleton<ILogger, Logger<UsersService>>( );
            services.AdServiceAuth( );
            services.AddDbContext<PgDbContext>( options =>
                    options.UseNpgsql( Config.DbConnection ) );
            services.AddMassTransit( x =>
            {
                x.AddConsumer<AuthAttemptConsumer>( );
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

        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if(env.IsDevelopment( ))
            {
                app.UseDeveloperExceptionPage( );
            }


            app.UseRouting( );

            app.UseEndpoints( endpoints =>
            {
                endpoints.MapGrpcService<gameofwords.users.Services.UsersService>( );
            } );
        }

    }
}
