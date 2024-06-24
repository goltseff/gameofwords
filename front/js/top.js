function fillTopWins(){
	$('#top_wins').append('<tr class=tr_wins><td colspan=2 align=center>загрузка...</td></tr>');
	$.get({
		'url': apiPref + 'game/top-wins',
		success: function (data) {
			$('.tr_wins').remove();
            $.each(data.data, function (index, item) {
				let htmlStr='<tr class="tr_wins '+(index%2==1?'tr2':'')+'">';
				htmlStr+='<td>'+item.name+'</td>';
				htmlStr+='<td>'+item.value+'</td>';
				$('#top_wins').append(htmlStr);
			});	
		}
	});
}

function fillTopGames(){
	$('#top_games').append('<tr class=tr_games><td colspan=2 align=center>загрузка...</td></tr>');
	$.get({
		'url': apiPref + 'game/top-games',
		success: function (data) {
			$('.tr_games').remove();
            $.each(data.data, function (index, item) {
				let htmlStr='<tr class="tr_games '+(index%2==1?'tr2':'')+'">';
				htmlStr+='<td>'+item.name+'</td>';
				htmlStr+='<td>'+item.value+'</td>';
				$('#top_games').append(htmlStr);
			});	
		}
	});
}

function fillTopPercent(){
	$('#top_percent').append('<tr class=tr_percent><td colspan=2 align=center>загрузка...</td></tr>');
	$.get({
		'url': apiPref + 'game/top-percent',
		success: function (data) {
			$('.tr_percent').remove();
            $.each(data.data, function (index, item) {
				let htmlStr='<tr class="tr_percent '+(index%2==1?'tr2':'')+'">';
				htmlStr+='<td>'+item.name+'</td>';
				htmlStr+='<td>'+item.value+'</td>';
				$('#top_percent').append(htmlStr);
			});	
		}
	});
}

function fillTopWords(){
	$('#top_words').append('<tr class=tr_words><td colspan=2 align=center>загрузка...</td></tr>');
	$.get({
		'url': apiPref + 'game/top-words',
		success: function (data) {
			$('.tr_words').remove();
            $.each(data.data, function (index, item) {
				let htmlStr='<tr class="tr_words '+(index%2==1?'tr2':'')+'">';
				htmlStr+='<td title="'+item.description+'">'+item.name+'</td>';
				htmlStr+='<td>'+item.value+'</td>';
				$('#top_words').append(htmlStr);
			});	
		}
	});
}

function fillTopContainsWords(){
	$('#top_contains_words').append('<tr class=tr_contains_words><td colspan=2 align=center>загрузка...</td></tr>');
	$.get({
		'url': apiPref + 'game/top-contains-words',
		success: function (data) {
			$('.tr_contains_words').remove();
            $.each(data.data, function (index, item) {
				let htmlStr='<tr class="tr_contains_words '+(index%2==1?'tr2':'')+'">';
				htmlStr+='<td title="'+item.description+'">'+item.name+'</td>';
				htmlStr+='<td>'+item.value+'</td>';
				$('#top_contains_words').append(htmlStr);
			});	
		}
	});
}

