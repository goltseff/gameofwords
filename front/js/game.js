let pageSize = 20;
let countItems = 0;
let gameId = 0;

function fillDifficulty()
{
	$.get({ url: apiPref+'game/difficulty/list', success: function(response) {
			let htmlStr='';
            $.each(response.data, function (index, item) {
				htmlStr+='<option value="'+item.id+'">'+item.name;
			});	
			$('#difficulty').html(htmlStr);
		}
	});	
}

function fillWordLength()
{
	let ii=5;
	let htmlStr='';
	while (ii<=20){
		htmlStr+='<option value="'+ii+'">'+ii;
		ii++;
	}
	$('#word_length').html(htmlStr);
}

function fillOldGames(page){
	let skip = (page-1)*pageSize;
	
	let requestData = {
		skip: skip,
		take: pageSize
	}
	$('.tr_games').remove();
	$('#games_table').append('<tr class=tr_games><td colspan=9 align=center>загрузка...</td></tr>');
	
	$.ajax({
		'url': apiPref + 'game/list',
		contentType: 'application/json',
		type: 'POST',
		data: JSON.stringify(requestData),
		success: function (data) {
			$('.tr_games').remove();
            $.each(data, function (index, item) {
				let htmlStr='<tr class="tr_games '+(index%2==1?'tr2':'')+'">';
				htmlStr+='<td class="item_with_tooltip" title="'+item.wordDescription+'">'+item.word+'</td>';
				htmlStr+='<td>'+item.difficulty+'</td>';
				htmlStr+='<td>'+item.createDate.substring(0, 19).replace("T","&nbsp;")+'</td>';
				htmlStr+='<td>'+item.countWords+'</td>';
				htmlStr+='<td>'+(item.isFinished?'<span style=color:green>да</span>':'<span style=color:red>нет</span>')+'</td>';
				htmlStr+='<td>'+(item.isUserWin?'<span style=color:green>да</span>':'<span style=color:red>нет</span>')+'</td>';
				htmlStr+='<td>'+(item.canConinue?'<a href="game.html?id='+item.id+'">продолжить</a>':'<a href="game.html?id='+item.id+'">посмотреть</a>')+'</td>';
				htmlStr+='</tr>';
				$('#games_table').append(htmlStr);
			});	
		}
	});
}

function fillGamesPages()
{
	let requestData = {
		skip: 0,
		take: pageSize
	}
	
	$('#cnt_pages').html('загрузка...');
	$.ajax({
		'url': apiPref + 'game/list/count',
		contentType: 'application/json',
		type: 'POST',
		data: JSON.stringify(requestData),
		success: function (data) {
			countItems = data;
			let htmlStr = '';
			let ii=0;
			while (ii<=countItems/pageSize){
				htmlStr += (htmlStr==''?'':' :: ')
				htmlStr +=' [<a href="javascript:void(0)" class="users_page">'+(ii+1)+'</a>] ';
				ii++;
			}
			$('#cnt_pages').html('перейти на страницу: '+htmlStr);
			addPageLinkHook();
		}
	});
}

function addPageLinkHook(){
    $('.users_page').click(function () {
		fillOldGames($(this).text());
    });
}

function fillGameInfo()
{
	const params = new Proxy(new URLSearchParams(window.location.search), {
	  get: (searchParams, prop) => searchParams.get(prop),
	});
	gameId = parseInt(params.id);
    if (isNaN(gameId)){window.location = 'index.html';}
	
	$.get({ url: apiPref+'game/'+gameId, 
			success: function(response) {
				$('#words_table').show();
				$('#word').text(response.word);
				$('#word').prop('title', response.wordDescription);
				$('#game_info').html('сложность: '+response.difficulty);
				$('#game_info').append('<br>создана: '+response.createDate.substring(0, 19).replace("T","&nbsp;"));
				$('#game_info').append('<br>автор: '+response.userNickName);
				$('#game_info').append('<br>закончено: '+(response.isFinished?'<span style=color:green>да</span>':'<span style=color:red>нет</span>'));
				$('#game_info').append('<br>победа: '+(response.isUserWin?'<span style=color:green>да</span>':'<span style=color:red>нет</span>'));
				if (response.canContinue){$('#move_table').show();}
				let htmlStr='';
				$.each(response.moves, function (index, item) {
					htmlStr+='<tr>';
					if (!item.isFromUser){htmlStr+='<td>&nbsp;</td>';}
					htmlStr+='<td><span style="font-size:60%">'+item.createDate.substring(0, 19).replace("T","&nbsp;")+'</span><br><span class="item_with_tooltip" title="'+item.wordDescription+'">'+item.word+'</span></td>';
					if (item.isFromUser){htmlStr+='<td>&nbsp;</td>';}
					htmlStr+='</tr>';
				});	
				$('#words_table').append(htmlStr);
		},
		error: function(xhr, status, error) {
			alert(xhr.responseText);
			window.location = 'index.html';
		}
	});	
}

function createGame(){
	let requestData = {
		wordLength: $('#word_length').val(),
		difficulty: $('#difficulty').val()
	}
	$.ajax({
		'url': apiPref + 'game/create',
		contentType: 'application/json',
		type: 'POST',
		data: JSON.stringify(requestData),
		success: function (response) {
			window.location = 'game.html?id='+response.gameId;
		},
		error: function(xhr, status, error) {
			alert(xhr.responseText);
		}
	});
}


function doMove()
{
	let currentDate = new Date().toISOString().substring(0,19).replace("T","&nbsp;");
	let requestData = {
		word: $('#move_word').val()
	}
	$.ajax({
		'url': apiPref + 'game/'+gameId+'/move',
		contentType: 'application/json',
		type: 'POST',
		data: JSON.stringify(requestData),
		success: function (response) {
			let htmlStr='';
			htmlStr+='<tr>';
			htmlStr+='<td><span style="font-size:60%">'+currentDate+'</span><br>'+$('#move_word').val()+'</td>';
			htmlStr+='<td>&nbsp;</td>';
			htmlStr+='</tr>';
			if (response.gameEnd)
			{
				alert('Вы победили');
				$('#move_table').hide();
			}else{
				htmlStr+='<tr>';
				htmlStr+='<td>&nbsp;</td>';
				htmlStr+='<td><span style="font-size:60%">'+currentDate+'</span><br><span class="item_with_tooltip" title="'+response.wordDescription+'">'+response.word+'</span></td>';
				htmlStr+='</tr>';
				$('#move_word').val('');
				$('#move_word').focus();
			}
			$('#words_table').append(htmlStr);
		},
		error: function(xhr, status, error) {
			alert(xhr.responseText);
			$('#move_word').focus();
		}
	});
	
	
	
}
function doGiveUp()
{
	let requestData = {
	}
	$.ajax({
		'url': apiPref + 'game/'+gameId+'/give-up',
		contentType: 'application/json',
		type: 'POST',
		data: JSON.stringify(requestData),
		success: function (response) {
			window.location.reload();			
		},
		error: function(xhr, status, error) {
			alert(xhr.responseText);
		}
	});
	
	
	
}

function fillLastGames(){
	$('.tr_games').remove();
	$('#games_table').append('<tr class=tr_games><td colspan=10 align=center>загрузка...</td></tr>');
	
	$.get({
		'url': apiPref + 'game/last-games',
		success: function (data) {
			$('.tr_games').remove();
            $.each(data, function (index, item) {
				let htmlStr='<tr class="tr_games '+(index%2==1?'tr2':'')+'">';
				htmlStr+='<td>'+item.userNickName+'</td>';
				htmlStr+='<td class="item_with_tooltip" title="'+item.wordDescription+'">'+item.word+'</td>';
				htmlStr+='<td>'+item.difficulty+'</td>';
				htmlStr+='<td>'+item.createDate.substring(0, 19).replace("T","&nbsp;")+'</td>';
				htmlStr+='<td>'+item.countWords+'</td>';
				htmlStr+='<td>'+(item.isFinished?'<span style=color:green>да</span>':'<span style=color:red>нет</span>')+'</td>';
				htmlStr+='<td>'+(item.isUserWin?'<span style=color:green>да</span>':'<span style=color:red>нет</span>')+'</td>';
				htmlStr+='<td>'+(item.canConinue?'<a href="game.html?id='+item.id+'">продолжить</a>':'<a href="game.html?id='+item.id+'">посмотреть</a>')+'</td>';
				htmlStr+='</tr>';
				$('#games_table').append(htmlStr);
			});	
		}
	});
}
