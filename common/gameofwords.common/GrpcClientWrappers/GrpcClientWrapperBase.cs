using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;


namespace gameofwords.common.Grpc.GrpcClientWrappers
{
    public abstract class GrpcClientWrapperBase<T> : IGrpcClientWrapper<T>
        where T: ClientBase<T>
    {
        private readonly ILogger _logger;


        protected GrpcClientWrapperBase( ILogger logger)
        {
            _logger = logger;
        }

        private readonly AsyncLock _grpcClientLock = new( );
        private T _grpcClient;
        public async Task<T> GetGrpcClient()
        {
            if( _grpcClient is null )
            {
                using( await _grpcClientLock.LockAsync() )
                {
                    if( _grpcClient is null )
                    {
                        _grpcClient = await CreateClient( );
                    }
                }
            }

            return _grpcClient;
        }

        private void ResetGrpcClient( ) { _grpcClient = null; }

        protected abstract Task<Uri> GetClientUri( );
        protected abstract T CreateClient( ChannelBase channel );

        protected virtual ChannelBase CreateGrpcChannel( Uri uri )
        {
            return GrpcChannel.ForAddress( uri,
                new GrpcChannelOptions
                {
                    HttpClient = new HttpClient(
                        new HttpClientHandler( )
                        {
                            ServerCertificateCustomValidationCallback =
                                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                            UseProxy = false

                        } ),
                    Credentials = ChannelCredentials.Insecure
                } );
        }

        private async Task<T> CreateClient()
        {
            try
            {
                var channel = CreateGrpcChannel( await GetClientUri() );
                return CreateClient( channel );
            }
            catch( Exception ex )
            {
                _logger.LogError( "Error CreateClient "+ex.Message );
                throw;
            }
        }

        public virtual async Task<TResult> CallServiceAsync<TResult>( 
            Func<T, Task<TResult>> callFunction)
        {
            while( true )
            {
                try
                {
                    var grpcClient = await GetGrpcClient( );
                    var resultTask = callFunction( grpcClient );
                    var result = await resultTask;
                    return result;
                }
                catch( RpcException )
                {
                    throw;
                }
            }
        }
    }
}
