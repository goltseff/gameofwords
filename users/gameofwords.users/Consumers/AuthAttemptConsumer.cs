using gameofwords.common.EventContracts;
using gameofwords.users.DataLayer;
using gameofwords.users.Models;
using MassTransit;

namespace gameofwords.users.Consumers
{
    public class AuthAttemptConsumer : IConsumer<AuthAttemptEvent>
    {
        private readonly PgDbContext _dbContext;

        public AuthAttemptConsumer( PgDbContext dbContext )
        {
            _dbContext=dbContext;
        }

        public async Task Consume( ConsumeContext<AuthAttemptEvent> context )
        {
            var attempt = new UsersAuthAttempts
            {
                Datetime = DateTime.UtcNow,
                IsSuccess = context.Message.IsSuccess,
                UserId = context.Message.UserId
            };
            await _dbContext.UsersAuthAttempts.AddAsync( attempt );
            await _dbContext.SaveChangesAsync( );
        }
    }
}
