
using System.Collections.Concurrent;

namespace gameofwords.auth.Services
{
    public interface ISessionService
    {
        Guid CreateSession( int userId, bool isAdmin );
        bool CloseSession( Guid sessionId );
        void RefreshSession( Guid sessionId );
        List<(Guid sessionId,int userId,DateTime sessionDateTime)> GetSessionList( );
        bool CheckSession( Guid sessionId );
        bool CheckAdmin( Guid sessionId );
        int? GetUserId( Guid sessionId );
    }


    public class SessionService : ISessionService
    {
        private class Session
        {
            public int userId { get; set; }
            public DateTime sessionDateTime { get; set; }
            public bool isAdmin { get; set; }
        }

        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<Guid, Session> _userSessions
            = new( );

        public SessionService( ILogger logger ) {  _logger = logger; }

        public bool CheckAdmin( Guid sessionId )
        {
            if (_userSessions.ContainsKey( sessionId ))
            {
                return _userSessions[sessionId].isAdmin;
            }
            return false;
        }

        public bool CheckSession( Guid sessionId )
            =>_userSessions.ContainsKey( sessionId );

        public bool CloseSession( Guid sessionId )
        {
            _userSessions.TryRemove( sessionId, out var session );
            return true;
        }

        public Guid CreateSession( int userId, bool isAdmin )
        {
            var id = Guid.NewGuid();
            _userSessions.TryAdd( id, new Session { userId = userId, sessionDateTime= DateTime.Now , isAdmin = isAdmin} );
            return id;
        }

        public List<(Guid, int, DateTime)> GetSessionList( )
        {
            return _userSessions.Select( x => (x.Key, x.Value.userId, x.Value.sessionDateTime) ).ToList( );
        }

        public void RefreshSession( Guid sessionId )
        {
            _userSessions[sessionId].sessionDateTime = DateTime.Now;
        }

        public int? GetUserId( Guid sessionId )
        {
            if(_userSessions.TryGetValue( sessionId, out Session value ))
                return value.userId;
            return null;
        }
    }
}
