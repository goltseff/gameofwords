using gameofwords.common.config;
using gameofwords.common.Grpc.GrpcClientWrappers;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace gameofwords.common.Services
{
    public static class GameServiceExt
    {
        public static IServiceCollection AdServiceGame( this IServiceCollection services )
            => services
            .AddSingleton<ILogger, Logger<GameService>>()
            .AddSingleton<IGameService, GameService>( );
    }

    public interface IGameService : IGrpcClientWrapper<service.GameService.GameServiceClient>
    {
        public const string ServiceName = "game";
    }

    public class GameService : GrpcClientWrapperBase<service.GameService.GameServiceClient>, IGameService
    {
        private readonly ILogger<GameService> _logger;
        private static string ServiceName => IGameService.ServiceName;

        public GameService( ILogger<GameService> logger ) : base( logger )
        {
            _logger = logger;
        }

        protected override async Task<Uri> GetClientUri( )
        {
            var serviceUrl = Config.GetServiceUrl( ServiceName );
            return await Task<Uri>.FromResult( new Uri( serviceUrl ) );
        }
            

        protected override service.GameService.GameServiceClient CreateClient( ChannelBase channel )
            => new( channel );

    }

}
