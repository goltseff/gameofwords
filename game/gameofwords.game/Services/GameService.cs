using gameofwords.common.Services;
using gameofwords.common.tools;
using gameofwords.game.DataLayer;
using gameofwords.game.Models;
using gameofwords.service;
using Gameofwords.Common;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace gameofwords.game.Services
{
    public class GameService : gameofwords.service.GameService.GameServiceBase
    {
        private readonly ILogger _logger;
        private readonly PgDbContext _dbContext;
        private readonly IAuthService _auth;


        public GameService( ILogger logger, PgDbContext dbContext, IAuthService auth )
        {
            _logger = logger;
            _dbContext = dbContext;
            _auth = auth;
        }

        public override async Task<WordResponse> WordInfo( IdRequest request, ServerCallContext context )
        {
            var reply = new WordResponse( );
            var data = await _dbContext.Words
            .Where( x => x.Id == request.Id )
            .Select( x => new WordResponse { Description = x.Description, Name=x.Name } )
            .FirstOrDefaultAsync( );
            if(data != null)
            {
                reply.Description = data.Description;
                reply.Name = data.Name;
            }
            return reply;
        }

        public override async Task<GameResponse> CreateGame( CreateGameRequest request, ServerCallContext context )
        {
            var reply = new GameResponse( );
            int gameId = 0;

            var currentUserId = await _auth.CallServiceAsync( async client =>
                await client.GetCurrentUserIdAsync( request.Context ) );


            var wordData = await (from w in _dbContext.Words
                                  let already = (from g in _dbContext.Games where g.UserId == currentUserId.Data && g.WordId == w.Id select 1).Any( )
                                  orderby Guid.NewGuid( )
                                  where w.Name.Length == request.WordLength
                                  && !already
                                  select new
                                  {
                                      w.Id,
                                      w.Name,
                                  }
                                  ).FirstOrDefaultAsync( );

            if(wordData == null) {
                return new GameResponse { Error = new Error { Code=500, Message = "не могу подобрать слово заданной длины, выберите другую" } };
            }

            var difficultyPercent = await _dbContext.GamesDifficulty.Where( x => x.Id == request.Difficulty ).Select( x => x.Percent ).FirstOrDefaultAsync( );
            var countWords = await _dbContext.WordsLinks.Where( x => x.WordId == wordData.Id ).CountAsync( );
            countWords = (int)Math.Ceiling( (decimal)countWords*difficultyPercent/100 );
            var wordsTmpIds = await _dbContext.WordsLinks
                .Where( x => x.WordId == wordData.Id )
                .Select(x => x.ContainsWordId)
                .OrderBy( x => Guid.NewGuid( ) )
                .Take( countWords )
                .ToListAsync();

            using var transaction = _dbContext.Database.BeginTransaction( );
            try
            {
                var game = new Games { UserId = currentUserId.Data, DifficultyId = request.Difficulty, WordId =wordData.Id, CreateDate=DateTime.UtcNow };
                await _dbContext.Games.AddAsync( game );
                await _dbContext.SaveChangesAsync( );
                gameId = game.Id;

                foreach(var word in wordsTmpIds)
                {
                    var item = new GamesWordsTmp { GameId = gameId, WordId = word };
                    await _dbContext.GamesWordsTmp.AddAsync( item );
                    await _dbContext.SaveChangesAsync( );
                }
                await transaction.CommitAsync();
            }
            catch(Exception e)
            {
                transaction.Rollback();
                return new GameResponse { Error = new Error { Code=500, Message = e.Message } };
            }

            reply.Word = wordData.Name;
            reply.WordId = wordData.Id;
            reply.GameId = gameId;

            return reply;
        }

        public override async Task<GameMoveResponse> GameMove( GameMoveRequest request, ServerCallContext context )
        {
            var reply = new GameMoveResponse();
            var word = request.Word.Trim().ToLower();

            var currentUserId = await _auth.CallServiceAsync( async client =>
                await client.GetCurrentUserIdAsync( request.Context ) );

            var gameData = await _dbContext.Games.Where( x => x.Id == request.GameId ).FirstOrDefaultAsync( );
            if(gameData == null)
            {
                return new GameMoveResponse { Error = new Error { Code=404, Message = "игра не найдена" } };
            }
            if(gameData.IsFinished)
            {
                return new GameMoveResponse { Error = new Error { Code=500, Message = "игра закончена" } };
            }
            if(gameData.UserId!=currentUserId.Data)
            {
                return new GameMoveResponse { Error = new Error { Code=500, Message = "нельзя играть в чужую игру" } };
            }


            var wordData = await _dbContext.Words.Where( x => x.Name == request.Word ).FirstOrDefaultAsync();
            if(wordData == null) {
                return new GameMoveResponse { Error = new Error { Code=500, Message = "я не знаю такое слово :(" } };
            }


            var usedWord = await (from w in _dbContext.Words
                                  join gw in _dbContext.GamesWords
                                  on w.Id equals gw.WordId
                                  where w.Name == word
                                  && gw.GameId == request.GameId
                                  select 1
                                  ).AnyAsync( );
            if(usedWord)
            {
                return new GameMoveResponse { Error = new Error { Code=500, Message = "такое слово уже было использовано в этой игре" } };
            }


            var isSutable = await (from w in _dbContext.Words
                                   join wl in _dbContext.WordsLinks
                                   on w.Id equals wl.ContainsWordId
                                   where wl.WordId == gameData.WordId
                                   && wl.ContainsWordId == wordData.Id
                                   select 1
                                   ).AnyAsync( );
            if(!isSutable)
            {
                return new GameMoveResponse { Error = new Error { Code=500, Message = "это слово не подходит" } };
            }

            var replyWord = await (from gwt in _dbContext.GamesWordsTmp
                                   join w in _dbContext.Words
                                   on gwt.WordId equals w.Id
                                   let isUsed = (from gw in _dbContext.GamesWords where gw.GameId == request.GameId && gw.WordId == gwt.WordId select 1).Any( )
                                   orderby Guid.NewGuid( )
                                   where gwt.GameId == request.GameId
                                   && w.Name!=word
                                   && !isUsed
                                   select new
                                   {
                                       w.Id,
                                       w.Name,
                                       w.Description
                                   }
                                   ).FirstOrDefaultAsync( );

            using var transaction = _dbContext.Database.BeginTransaction( );
            try
            {
                var item = new GamesWords { GameId = request.GameId, IsFromUser=true, WordId=wordData.Id, CreateDate=DateTime.UtcNow };
                await _dbContext.GamesWords.AddAsync( item );
                await _dbContext.SaveChangesAsync( );
                if(replyWord != null)
                {
                    var replyItem = new GamesWords { GameId = request.GameId, IsFromUser=false, WordId=replyWord.Id, CreateDate=DateTime.UtcNow };
                    await _dbContext.GamesWords.AddAsync( replyItem );
                    await _dbContext.SaveChangesAsync( );

                    reply.Word = replyWord.Name;
                    reply.WordId = replyWord.Id;
                    reply.WordDescription = replyWord.Description;
                }
                else
                {
                    var game = await _dbContext.Games.Where( x => x.Id == request.GameId ).FirstOrDefaultAsync( );
                    game.IsFinished = true;
                    game.IsUserWin = true;
                    await _dbContext.SaveChangesAsync( );
                    reply.GameEnd = true;
                }

                await transaction.CommitAsync( );
            }
            catch(Exception e)
            {
                transaction.Rollback( );
                return new GameMoveResponse { Error = new Error { Code=500, Message = e.Message } };
            }





            return reply;
        }

        public override async Task<GameInfoResponse> GameInfo( IdRequest request, ServerCallContext context )
        {
            var currentUserId = await _auth.CallServiceAsync( async client =>
                await client.GetCurrentUserIdAsync( request.Context ) );

            var data = await (from g in _dbContext.Games
                              join w in _dbContext.Words
                              on g.WordId equals w.Id
                              join u in _dbContext.Users
                              on g.UserId equals u.Id
                              join d in _dbContext.GamesDifficulty
                              on g.DifficultyId equals d.Id
                              where g.Id == request.Id
                              select new GameInfoResponse
                              {
                                  Difficulty = d.Name,
                                  IsFinished = g.IsFinished,
                                  UserId = g.UserId,
                                  UserNickName = u.Nickname,
                                  IsUserWin = g.IsUserWin,
                                  Word = w.Name,
                                  WordId = w.Id,
                                  CanConinue = !g.IsFinished && g.UserId == currentUserId.Data,
                                  CreateDate = Timestamp.FromDateTime( DateTime.SpecifyKind( g.CreateDate, DateTimeKind.Unspecified ) ),
                                  WordDescription = w.Description,
                              }
                              ).FirstOrDefaultAsync( );

            if (data == null )
                return new GameInfoResponse { Error = new Error { Code=404, Message = "игра не найдена" } };

            var movesData = await (from gw in _dbContext.GamesWords
                                   join w in _dbContext.Words
                                   on gw.WordId equals w.Id
                                   orderby gw.CreateDate
                                   where gw.GameId == request.Id
                                   select new GameMoveItem
                                   {
                                       CreateDate = Timestamp.FromDateTime( DateTime.SpecifyKind( gw.CreateDate, DateTimeKind.Unspecified ) ),
                                       IsFromUser = gw.IsFromUser,
                                       Word = w.Name,
                                       WordId = w.Id,
                                       WordDescription = w.Description
                                   }
                                   ).ToListAsync( );
            data.Moves.AddRange( movesData );
            return data;
        }

        public override async Task<BoolResponse> GiveUp( IdRequest request, ServerCallContext context )
        {

            var currentUserId = await _auth.CallServiceAsync( async client =>
                await client.GetCurrentUserIdAsync( request.Context ) );

            var gameData = await _dbContext.Games.Where( x => x.Id == request.Id ).FirstOrDefaultAsync( );
            if(gameData == null)
            {
                return new BoolResponse { Error = new Error { Code=404, Message = "игра не найдена" } };
            }
            if(gameData.IsFinished)
            {
                return new BoolResponse { Error = new Error { Code=500, Message = "игра закончена" } };
            }
            if(gameData.UserId!=currentUserId.Data)
            {
                return new BoolResponse { Error = new Error { Code=500, Message = "нельзя играть в чужую игру" } };
            }

            var game = await _dbContext.Games.Where( x => x.Id == request.Id ).FirstOrDefaultAsync( );
            game.IsFinished = true;
            game.IsUserWin = false;
            await _dbContext.SaveChangesAsync( );

            return new BoolResponse( );
        }

        public override async Task<GamesListResponse> GetCurrentUserGamesList( GamesListRequest request, ServerCallContext context )
        {
            var currentUserId = await _auth.CallServiceAsync( async client =>
                await client.GetCurrentUserIdAsync( request.Context ) );

            var data = await (from g in _dbContext.Games
                              join w in _dbContext.Words
                              on g.WordId equals w.Id
                              join d in _dbContext.GamesDifficulty
                              on g.DifficultyId equals d.Id
                              let countWords = (from gw in _dbContext.GamesWords where gw.GameId == g.Id select 1).Count()
                              orderby g.CreateDate descending
                              where g.UserId == currentUserId.Data
                              select new GamesListItem
                              {
                                  Id = g.Id,
                                  CreateDate = Timestamp.FromDateTime( DateTime.SpecifyKind( g.CreateDate, DateTimeKind.Unspecified ) ),
                                  Difficulty = d.Name,
                                  IsFinished = g.IsFinished,
                                  IsUserWin = g.IsUserWin,
                                  WordId = w.Id,
                                  Word=w.Name,
                                  CountWords=countWords,
                                  CanConinue = !g.IsFinished && g.UserId == currentUserId.Data,
                                  WordDescription = w.Description,
                              }
            )
            .Skip( request.Skip )
            .Take( request.Take )
            .ToListAsync( );

            var reply = new GamesListResponse( );
            if(data != null) reply.Data.AddRange( data );

            return reply;

        }
        
        public override async Task<IdResponse> GetCurrentUserGamesListCount( GamesListRequest request, ServerCallContext context )
        {
            var currentUserId = await _auth.CallServiceAsync( async client =>
                await client.GetCurrentUserIdAsync( request.Context ) );

            var data = await (from g in _dbContext.Games
                              where g.UserId == currentUserId.Data
                              select 1
            ).CountAsync( );

            return new IdResponse( ) { Data = data };


        }

        public override async Task<DifficultyListResponse> DifficultyList( DataRequest request, ServerCallContext context )
        {
            var data = await _dbContext.GamesDifficulty.OrderBy( x => x.Id ).Select( x => new DifficultyListItem { Id = x.Id, Name=x.Name } ).ToListAsync( );
            var reply = new DifficultyListResponse( );
            reply.Data.AddRange( data );
            return reply;


        }

        public override async Task<TopResponse> TopWords( DataRequest request, ServerCallContext context )
        {
            var data = await _dbContext.TopWords.Select( x => new TopResponseItem { Value = x.Count, Name=x.Name, Description=x.Description  } ).ToListAsync( );
            var reply = new TopResponse( );
            reply.Data.AddRange( data );
            return reply;
        }

        public override async Task<TopResponse> TopContainsWords( DataRequest request, ServerCallContext context )
        {
            var data = await _dbContext.TopContainsWords.Select( x => new TopResponseItem { Value = x.Count, Name=x.Name, Description=x.Description } ).ToListAsync( );
            var reply = new TopResponse( );
            reply.Data.AddRange( data );
            return reply;
        }

        public override async Task<TopResponse> TopGames( DataRequest request, ServerCallContext context )
        {
            var data = await _dbContext.TopGames.Select( x => new TopResponseItem { Value = x.Count, Name=x.Name } ).ToListAsync( );
            var reply = new TopResponse( );
            reply.Data.AddRange( data );
            return reply;
        }

        public override async Task<TopResponse> TopWins( DataRequest request, ServerCallContext context )
        {
            var data = await _dbContext.TopWins.Select( x => new TopResponseItem { Value = x.Count, Name=x.Name } ).ToListAsync( );
            var reply = new TopResponse( );
            reply.Data.AddRange( data );
            return reply;
        }

        public override async Task<TopResponse> TopPercent( DataRequest request, ServerCallContext context )
        {
            var data = await _dbContext.TopPercent.Select( x => new TopResponseItem { Value = x.Count, Name=x.Name } ).ToListAsync( );
            var reply = new TopResponse( );
            reply.Data.AddRange( data );
            return reply;
        }

        public override async Task<GamesListResponse> GetLastGamesList( DataRequest request, ServerCallContext context )
        {
            var currentUserId = await _auth.CallServiceAsync( async client =>
                await client.GetCurrentUserIdAsync( request ) );

            var data = await (from g in _dbContext.Games
                              join w in _dbContext.Words
                              on g.WordId equals w.Id
                              join d in _dbContext.GamesDifficulty
                              on g.DifficultyId equals d.Id
                              join u in _dbContext.Users
                              on g.UserId equals u.Id
                              let countWords = (from gw in _dbContext.GamesWords where gw.GameId == g.Id select 1).Count( )
                              orderby g.Id descending
                              select new GamesListItem
                              {
                                  Id = g.Id,
                                  CreateDate = Timestamp.FromDateTime( DateTime.SpecifyKind( g.CreateDate, DateTimeKind.Unspecified ) ),
                                  Difficulty = d.Name,
                                  IsFinished = g.IsFinished,
                                  IsUserWin = g.IsUserWin,
                                  WordId = w.Id,
                                  Word=w.Name,
                                  CountWords=countWords,
                                  CanConinue = !g.IsFinished && g.UserId == currentUserId.Data,
                                  WordDescription = w.Description,
                                  UserNickName = u.Nickname
                              }
            )
            .Take( 10 )
            .ToListAsync( );

            var reply = new GamesListResponse( );
            if(data != null) reply.Data.AddRange( data );

            return reply;

        }

    }
}




