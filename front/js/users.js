let pageSize = 20;
let countItems = 0;


function fillUsers(page){
	let skip = (page-1)*pageSize;
	
	let requestData = {
		skip: skip,
		take: pageSize
	}
	$('.tr_users').remove();
	$('#users_table').append('<tr class=tr_users><td colspan=9 align=center>загрузка...</td></tr>');
	
	$.ajax({
		'url': apiPref + 'users/list',
		contentType: 'application/json',
		type: 'POST',
		data: JSON.stringify(requestData),
		success: function (data) {
			$('.tr_users').remove();
            $.each(data, function (index, item) {
				let htmlStr='<tr class="tr_users '+(index%2==1?'tr2':'')+'">';
				htmlStr+='<td>'+item.id+'</td>';
				htmlStr+='<td>'+item.login+'</td>';
				htmlStr+='<td>'+item.nickName+'</td>';
				htmlStr+='<td>'+item.email+'</td>';
				htmlStr+='<td>'+item.isAdmin+'</td>';
				htmlStr+='<td>'+item.isBot+'</td>';
				htmlStr+='<td><a href="javascript:showHistory('+item.id+')">history</a></td>';
				htmlStr+='<td><a href="javascript:showEdit('+item.id+')">edit</a></td>';
				htmlStr+='<td><a href="javascript:if (confirm(\'точно?\')){deleteUser('+item.id+');}">delete</a></td>';
				htmlStr+='</tr>';
				
				$('#users_table').append(htmlStr);
				
			});	
		}
	});
}

function fillUsersPages()
{
	let requestData = {
		skip: 0,
		take: pageSize
	}
	
	$('#cnt_pages').html('загрузка...');
	$.ajax({
		'url': apiPref + 'users/list/count',
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
		fillUsers($(this).text());
    });
}





function showHistory(userId)
{
	$('div.shadow').show();
	$('#user_history').show();
	
	$('#user_span_id').text(userId);
	$('.tr_history').remove();
	$('#history_table').append('<tr class=tr_history><td colspan=4 align=center>загрузка...</td></tr>');
	$.ajax({
		'url': apiPref + 'users/'+userId+'/history',
		contentType: 'application/json',
		type: 'GET',
		success: function (data) {
			$('.tr_history').remove();
            $.each(data, function (index, item) {
				let htmlStr='<tr class="tr_history '+(index%2==1?'tr2':'')+'">';
				htmlStr+='<td>'+item.action+'</td>';
				htmlStr+='<td>'+item.message+'</td>';
				htmlStr+='<td>'+item.date.substring(0, 19).replace("T","&nbsp;")+'</td>';
				htmlStr+='<td>'+item.who+'</td>';
				htmlStr+='</tr>';
				$('#history_table').append(htmlStr);
				
			});	
		}
	});
}


function showAddUserForm()
{
	switchFormToAdd();
	$('div.shadow').show();
	$('#user_add').show();
}

function addUser()
{
	let requestData = {
		login: $('#user_form_login').val(),
		nickName: $('#user_form_nickname').val(),
		email: $('#user_form_email').val(),
		password: $('#user_form_password').val(),
		isAdmin: $('#user_form_isadmin').is(":checked"),
		isBot: $('#user_form_isbot').is(":checked")
	}
	closeModal();
	showWait();

	$.ajax({
		'url': apiPref + 'users/create',
		contentType: 'application/json',
		type: 'POST',
		data: JSON.stringify(requestData),
		success: function (data) {
			closeModal();
			$('.user_add_input').val('');
			$('.user_add_input').prop('checked', false);
			fillUsers(1);
			alert('Успешно добавлено');
		},
		error: function (data) {
			alert(data.responseText);
			showAddUserForm();
		},
		always: function () {
			hideWait();
		}		
	});
}

function deleteUser(userId)
{
	showWait();
	$.ajax({
		'url': apiPref + 'users/'+userId+'/delete',
		contentType: 'application/json',
		type: 'DELETE',
		success: function (data) {
			$('#wait').show();
			closeModal();
			fillUsers(1);
			alert('Успешно удалено');
		},
		error: function (data) {
			alert(data.responseText);
		},
		always: function () {
			hideWait();
		}
	});
}
function switchFormToAdd()
{
	$('#user_add_table_header').text('Добавить нового пользователя');
	$('#user_form_login').prop( "disabled", false );
	$('#user_form_password').prop( "disabled", false );
	$('#user_form_edit_button').hide();
	$('#user_form_add_button').show();
	$('#password_form').hide();
}

function switchFormToEdit(userId)
{
	$('#user_add_table_header').text('Редактировать пользователя #'+userId);
	$('#user_form_login').prop( "disabled", true );
	$('#user_form_password').prop( "disabled", true );
	$('#user_form_edit_button').show();
	$('#user_form_add_button').hide();
	$('#password_form').show();
}


function showEdit(userId)
{
	showWait();

	$.ajax({
		'url': apiPref + 'users/'+userId,
		contentType: 'application/json',
		type: 'GET',
		success: function (data) {
			$('#user_form_login').val(data.login);
			$('#user_form_nickname').val(data.nickName);
			$('#user_form_email').val(data.email);
			$('#user_form_isadmin').prop( "checked", data.isAdmin );
			$('#user_form_isbot').prop( "checked", data.isBot );
			$('#user_form_id').val( userId );
			hideWait();
			switchFormToEdit(userId);
			$('div.shadow').show();
			$('#user_add').show();
		}
	});
}

function editUser()
{
	let requestData = {
		login: $('#user_form_login').val(),
		nickName: $('#user_form_nickname').val(),
		email: $('#user_form_email').val(),
		isAdmin: $('#user_form_isadmin').is(":checked"),
		isBot: $('#user_form_isbot').is(":checked")
	}
	closeModal();
	showWait();
	$.ajax({
		'url': apiPref + 'users/'+$('#user_form_id').val()+'/update',
		contentType: 'application/json',
		type: 'PUT',
		data: JSON.stringify(requestData),
		success: function (data) {
			closeModal();
			$('.user_add_input').val('');
			$('.user_add_input').prop('checked', false);
			fillUsers(1);
			alert('Успешно отредактировали');
		},
		error: function (data) {
			alert(data.responseText);
			showAddUserForm();
		},
		always: function () {
			hideWait();
		}		
	});
}

function editPassword()
{
	let requestData = {
		password: $('#user_form_newpassword').val(),
	}
	closeModal();
	showWait();
	$.ajax({
		'url': apiPref + 'users/'+$('#user_form_id').val()+'/update-password',
		contentType: 'application/json',
		type: 'PUT',
		data: JSON.stringify(requestData),
		success: function (data) {
			closeModal();
			fillUsers(1);
			alert('Успешно поменяли пароль');
		},
		error: function (data) {
			alert(data.responseText);
			showAddUserForm();
		},
		always: function () {
			hideWait();
		}		
	});
}
