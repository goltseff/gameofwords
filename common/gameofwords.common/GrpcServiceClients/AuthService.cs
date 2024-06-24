using gameofwords.common.config;
using gameofwords.common.Grpc.GrpcClientWrappers;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace gameofwords.common.Services
{
    public static class AuthServiceExt
    {
        public static IServiceCollection AdServiceAuth( this IServiceCollection services )
            => services
            .AddSingleton<ILogger, Logger<AuthService>>()
            .AddSingleton<IAuthService, AuthService>( );
    }

    public interface IAuthService : IGrpcClientWrapper<service.AuthService.AuthServiceClient>
    {
        public const string ServiceName = "auth";
    }

    public class AuthService : GrpcClientWrapperBase<service.AuthService.AuthServiceClient>, IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private static string ServiceName => IAuthService.ServiceName;

        public AuthService( ILogger<AuthService> logger ) : base( logger )
        {
            _logger = logger;
        }

        protected override async Task<Uri> GetClientUri( )
        {
            var serviceUrl = Config.GetServiceUrl( ServiceName );
            return await Task<Uri>.FromResult( new Uri( serviceUrl ) );
        }
            

        protected override service.AuthService.AuthServiceClient CreateClient( ChannelBase channel )
            => new( channel );

    }

}
