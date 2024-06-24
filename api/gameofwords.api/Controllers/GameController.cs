using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using gameofwords.service;
using gameofwords.common.Services;
using Grpc.Net.Client;
using System.Xml.Linq;
using gameofwords.common.config;
using Gameofwords.Common;
using gameofwords.api.Attributes;
using Newtonsoft.Json;
using System;

namespace gameofwords.api.controllers
{
    [ApiController]
    [Produces( MediaTypeNames.Application.Json )]
    [Consumes( MediaTypeNames.Application.Json )]
    [Route( "api/v1/game" )]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IGameService _game;
        private string SessionId => Request.Cookies["gameofwords.session"] ?? string.Empty;
        private DataRequest RequestContext => new( ) { SessionId = SessionId };

        public GameController( ILogger<GameController> logger, IGameService game )
        {
            _logger = logger;
            _game = game;
        }

        [HttpPost]
        [Route( "create" )]
        [RequireAuth]
        public async Task<IActionResult> Create( [FromBody] CreateGameRequest request )
        {
            request.Context = RequestContext;
            var result = await _game.CallServiceAsync( async client =>
                    await client.CreateGameAsync( request ) );

            if(result.Error!= null)
            {
                return StatusCode( result.Error.Code, result.Error.Message );
            }
            return Ok( result );
        }

        [HttpPost]
        [Route( "{id}/move" )]
        [RequireAuth]
        public async Task<IActionResult> GameMove( int id, [FromBody] GameMoveRequest request )
        {
            request.Context = RequestContext;
            request.GameId = id;
            var result = await _game.CallServiceAsync( async client =>
                    await client.GameMoveAsync( request ) );

            if(result.Error!= null)
            {
                return StatusCode( result.Error.Code, result.Error.Message );
            }
            return Ok( result );
        }

        [HttpGet]
        [Route( "word/{id}/info" )]
        [RequireAuth]
        public async Task<IActionResult> WordInfo( int id )
        {
            var result = await _game.CallServiceAsync( async client =>
                    await client.WordInfoAsync( new IdRequest { Id = id } ) );

            return Ok( result );
        }

        [HttpGet]
        [Route( "{id}" )]
        [RequireAuth]
        public async Task<IActionResult> GameInfo( int id )
        {
            var result = await _game.CallServiceAsync( async client =>
                    await client.GameInfoAsync( new IdRequest { Id = id , Context = RequestContext } ) );

            if(result.Error!= null)
            {
                return StatusCode( result.Error.Code, result.Error.Message );
            }

            var newResult = new ModelsDto.GameInfoResponse
            {
                CreateDate = result.CreateDate.ToDateTime( ),
                Difficulty = result.Difficulty,
                IsFinished = result.IsFinished,
                IsUserWin = result.IsUserWin,
                UserId = result.UserId,
                UserNickName = result.UserNickName,
                Word = result.Word,
                WordId = result.WordId,
                CanContinue = result.CanConinue,
                WordDescription = result.WordDescription,
                Moves = new List<ModelsDto.GameMoveItem>( )
            };
            if(result.Moves.Count!=0)
            {
                var newMoves = result.Moves.Select( x => new ModelsDto.GameMoveItem
                {
                    CreateDate = x.CreateDate.ToDateTime( ),
                    isFromUser = x.IsFromUser,
                    WordId= x.WordId,
                    Word = x.Word,
                    WordDescription = x.WordDescription
                } ).ToList( );

                newResult.Moves.AddRange( newMoves );
            }
            return Ok( newResult );
        }

        [HttpPost]
        [Route( "{id}/give-up" )]
        [RequireAuth]
        public async Task<IActionResult> GiveUp( int id)
        {
            var result = await _game.CallServiceAsync( async client =>
                    await client.GiveUpAsync( new IdRequest { Context = RequestContext, Id=id } ) );

            if(result.Error!= null)
            {
                return StatusCode( result.Error.Code, result.Error.Message );
            }
            return Ok( result );
        }


        [HttpPost]
        [Route( "list" )]
        [RequireAuth]
        public async Task<IActionResult> List( [FromBody] GamesListRequest request )
        {
            request.Context = RequestContext;
            var result = await _game.CallServiceAsync( async client =>
                    await client.GetCurrentUserGamesListAsync( request ) );

            var data = result.Data.Select(x => new
            {
                x.Id,
                x.Word,
                x.WordId,
                x.IsUserWin,
                x.IsFinished,
                x.CountWords,
                CreateDate = x.CreateDate.ToDateTime( ),
                x.Difficulty,
                x.CanConinue,
                x.WordDescription
            } );
            return Ok( data );
        }

        [HttpPost]
        [Route( "list/count" )]
        [RequireAuth]
        public async Task<IActionResult> ListCount( [FromBody] GamesListRequest request )
        {
            request.Context = RequestContext;
            var result = await _game.CallServiceAsync( async client =>
                    await client.GetCurrentUserGamesListCountAsync( request ) );

            return Ok( result.Data );
        }

        [HttpGet]
        [Route( "difficulty/list" )]
        [RequireAuth]
        public async Task<IActionResult> DifficultyList( int id )
        {
            var result = await _game.CallServiceAsync( async client =>
                    await client.DifficultyListAsync( new DataRequest( ) ) );

            return Ok( result );
        }

        [HttpGet]
        [Route( "top-games" )]
        [RequireAuth]
        public async Task<IActionResult> TopGames( int id )
        {
            var result = await _game.CallServiceAsync( async client =>
                    await client.TopGamesAsync( new DataRequest( ) ) );

            return Ok( result );
        }

        [HttpGet]
        [Route( "top-wins" )]
        [RequireAuth]
        public async Task<IActionResult> TopWins( int id )
        {
            var result = await _game.CallServiceAsync( async client =>
                    await client.TopWinsAsync( new DataRequest( ) ) );

            return Ok( result );
        }

        [HttpGet]
        [Route( "top-percent" )]
        [RequireAuth]
        public async Task<IActionResult> TopPercent( int id )
        {
            var result = await _game.CallServiceAsync( async client =>
                    await client.TopPercentAsync( new DataRequest( ) ) );

            return Ok( result );
        }

        [HttpGet]
        [Route( "top-words" )]
        [RequireAuth]
        public async Task<IActionResult> TopWords( int id )
        {
            var result = await _game.CallServiceAsync( async client =>
                    await client.TopWordsAsync( new DataRequest( ) ) );

            return Ok( result );
        }

        [HttpGet]
        [Route( "top-contains-words" )]
        [RequireAuth]
        public async Task<IActionResult> TopContainsWords( int id )
        {
            var result = await _game.CallServiceAsync( async client =>
                    await client.TopContainsWordsAsync( new DataRequest( ) ) );

            return Ok( result );
        }

        [HttpGet]
        [Route( "last-games" )]
        [RequireAuth]
        public async Task<IActionResult> LastGames( )
        {
            var result = await _game.CallServiceAsync( async client =>
                    await client.GetLastGamesListAsync( RequestContext ) );

            var data = result.Data.Select( x => new
            {
                x.Id,
                x.Word,
                x.WordId,
                x.IsUserWin,
                x.IsFinished,
                x.CountWords,
                CreateDate = x.CreateDate.ToDateTime( ),
                x.Difficulty,
                x.CanConinue,
                x.WordDescription,
                x.UserNickName
            } );
            return Ok( data );
        }


    }
}
