using gameofwords.common.config;
using gameofwords.common.Grpc.GrpcClientWrappers;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace gameofwords.common.Services
{
    public static class UsersServiceExt
    {
        public static IServiceCollection AdServiceUsers( this IServiceCollection services )
            => services
            .AddSingleton<ILogger, Logger<UsersService>>()
            .AddSingleton<IUsersService, UsersService>( );
    }

    public interface IUsersService : IGrpcClientWrapper<service.UsersService.UsersServiceClient>
    {
        public const string ServiceName = "users";
    }

    public class UsersService : GrpcClientWrapperBase<service.UsersService.UsersServiceClient>, IUsersService
    {
        private readonly ILogger<UsersService> _logger;
        private static string ServiceName => IUsersService.ServiceName;

        public UsersService( ILogger<UsersService> logger ) : base( logger )
        {
            _logger = logger;
        }

        protected override async Task<Uri> GetClientUri( )
        {
            var serviceUrl = Config.GetServiceUrl( ServiceName );
            return await Task<Uri>.FromResult( new Uri( serviceUrl ) );
        }
            

        protected override service.UsersService.UsersServiceClient CreateClient( ChannelBase channel )
            => new( channel );

    }

}
