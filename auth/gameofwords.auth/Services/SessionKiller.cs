using gameofwords.common.config;
using gameofwords.common.Services;

namespace gameofwords.auth.Services
{
    public class SessionKiller : IHostedService
    {
        const int Delay = 30000;

        private readonly ISessionService _sessionService;
        private readonly ILogger _logger;
        private readonly TimeSpan _sessionTimeout;
        private CancellationTokenSource _cts;

        public SessionKiller( ISessionService sessionService,
            ILogger logger )
        {
            _logger = logger;
            _sessionService = sessionService;

            string sessionLifetimeMinutesString = Config.GetServiceParam( IAuthService.ServiceName, "session-lifetime-minutes" );

            if(int.TryParse( sessionLifetimeMinutesString, out int _sessionLifetimeMinutes ) == false)
            {
                _sessionLifetimeMinutes = 60;
            }

            _sessionTimeout = TimeSpan.FromMinutes( _sessionLifetimeMinutes );
        }

        private async Task CheckSessions( CancellationToken ct )
        {
            _logger.LogInformation( "Session Killer started" );

            while(ct.IsCancellationRequested == false)
            {
                var userSessions = _sessionService.GetSessionList( );

                foreach(var userSession in userSessions.Where(
                    x => x.sessionDateTime + _sessionTimeout < DateTimeOffset.Now ))
                {
                    if(ct.IsCancellationRequested) break;
                    _sessionService.CloseSession( userSession.sessionId );
                    _logger.LogInformation( $"Session {userSession.sessionId} closed" );
                }
                await Task.Delay( Delay, ct );
            }
        }
        public async Task StartAsync( CancellationToken cancellationToken )
        {
            _cts = new CancellationTokenSource( );
            var _ = CheckSessions( _cts.Token );

            await Task.CompletedTask;
        }
        public Task StopAsync( CancellationToken cancellationToken )
        {
            if(_cts != null) _cts.Cancel( );
            _cts = null;

            return Task.CompletedTask;
        }
    }
}
