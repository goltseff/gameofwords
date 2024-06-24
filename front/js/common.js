let apiPref = "/api/v1/";
let userName = "";
let isAdmin=false;
let isAuth=false;

$(document).ready(function () {
	$('div.shadow').on('click', function () {
		$('div.shadow').hide();
		$('div.modal_form').hide();
	});
});


async function checkAuth(){
	let ajaxPromise = $.get({ url: apiPref+'auth/check' });

	await ajaxPromise.then(
	  data => {
			isAuth = true;
			fillUserProfileData();
	  },
	  error => {
			isAuth = false;
	  }
	);

	return isAuth;
}



function doLogout() {
	$.ajax({'url': apiPref+'auth/logout', data: {}, type : 'get', success: function() {
				window.location.reload();
		}
	});
}

async function getUserProfile(){
	await $.ajax({'url': apiPref+'users/profile', data: {}, type : 'get', success: function(data) {
		userName = data.nickName;
		isAdmin=data.isAdmin;
		}
	});
}

async function fillUserProfileData()
{
	await getUserProfile()
	$('#user_nickname').text(userName);
	if (isAdmin){$('.only_admin').show();}
}

function closeModal(){
	$('div.shadow').trigger('click');
}

function showWait(){
	$('#wait').show();
	$('#shadow').show();
}

function hideWait(){
	$('#wait').hide();
	$('#shadow').hide();
}

$(document).on('keyup', function(e) {
  if (e.key == "Escape") closeModal();
});

