using System;
using System.Threading.Tasks;

namespace gameofwords.common.Grpc.GrpcClientWrappers
{
    public interface IGrpcClientWrapper<out T>
    {
        Task<TResult> CallServiceAsync<TResult>( Func<T, Task<TResult>> callFunction );
    }
}