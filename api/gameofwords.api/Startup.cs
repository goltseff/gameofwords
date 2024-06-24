using gameofwords.common.Services;

namespace gameofwords.api
{
    public class Startup
    {
        public void ConfigureServices( IServiceCollection services )
        {
            // Enable support for unencrypted HTTP2  
            AppContext.SetSwitch( "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true );
            AppContext.SetSwitch( "System.Net.Http.SocketsHttpHandler.Http2Support", true );

            services.AddGrpc( );
            services.AddControllers( );
            services.AddLogging( );
            services.AdServiceAuth( );
            services.AdServiceUsers( );
            services.AdServiceGame( );
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if(env.IsDevelopment( ))
            {
                app.UseDeveloperExceptionPage( );
            }
            app.UseRouting( );
            app.UseAuthentication( );
            app.UseAuthorization( );

            app.UseEndpoints( endpoints =>
            {
                endpoints.MapControllers( );
            } );


        }
    }
}
