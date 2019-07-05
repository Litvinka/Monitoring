
$(function() {

	//$("#tel").mask("(99) 999-9999");
	//$('#tel-edu').mask("(99) 999-9999");
	var rightPass = 0;

	$.each($('#curator-reg-form #user-registration input:not(#repeat-password)'), function() {
		$(this).on('focusout', function() {
			//if( ($(this).val().length < 2) || ($(this).val() === '(__) ___-____') ) {
			//	$(this).addClass('error');
			//	if ( !($(this).next().hasClass('error-inf')) ){
			//		$(this).after("<p class='error-inf'>Заполните поле</p>");
			//	}				
			//}
			//else {
			//	$(this).removeClass('error');
			//	$(this).next().remove();
			//}
			var regexp = new RegExp(/^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/);
			if ( $('input[type="email"]').val().length > 3 ) {
				if ( !(regexp.test($('input[type="email"]').val())) ) {
					$('input[type="email"]').removeClass('error');
					$('input[type="email"]').next().remove();
					$('input[type="email"]').addClass('error');
					if ( !($('input[type="email"]').next().length) )
						$('input[type="email"]').after("<p class='error-inf'>Неверный email</p>");
				}
			}
		});
	});
	$('input#repeat-password').on('focusout', function() {
		if ( $('input#repeat-password').val() !== $('input#password').val() ) {
			$('input#repeat-password').addClass('error');
			rightPass = 0;
			if ( !($('input#repeat-password').next().hasClass('error-inf')) ){
				$('input#repeat-password').after("<p class='error-inf'>Неверный пароль</p>");
			}
		} else {
			$('input#repeat-password').removeClass('error');
			$('input#repeat-password').next().remove();
			rightPass = 1;
		}
	});
	var count = 0;
	$('#btn-next-step').click(function(){
		$.each($('#curator-reg-form #user-registration input'), function() {
			if( ! ($(this).val().length < 2) && (rightPass == 1) ) {
				$('#user-registration').hide();
				$('#curator-reg-form #org-registration').show();
				$('#curator-reg-form #first-step').removeClass('active');
				$('#curator-reg-form #second-step').addClass('active');
			}
			else {
			    var opt = {
			        autoOpen: false,
			        modal: true,
			        width: 550,
			        title: 'Ошибка!'
				};
				$("#dialog").dialog(opt).dialog("open");
			}
		});
	});


	$('#btn-prev-step').click(function(){
		$('#curator-reg-form #org-registration').hide();
		$('#curator-reg-form #second-step').removeClass('active');
		$('#curator-reg-form #first-step').addClass('active');		
		$('#curator-reg-form #user-registration').show();
	});


	$.each($('#curator-reg-form #org-registration .form-control'), function() {
		$(this).on('focusout', function() {
			if( ($(this).val().length < 2) || ($(this).val() === '(__) ___-____') ) {
				$(this).addClass('error');
				if ( !($(this).next().hasClass('error-inf')) ){
					$(this).after("<p class='error-inf'>Заполните поле</p>");
				}				
			}
			else {
				$(this).removeClass('error');
				$(this).next().remove();
			}
			var regexp = new RegExp(/^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/);
			if ( $('input[type="email"]').val().length > 3 ) {
				if ( !(regexp.test($('input[type="email"]').val())) ) {
					$('input[type="email"]').removeClass('error');
					$('input[type="email"]').next().remove();
					$('input[type="email"]').addClass('error');
					if ( !($('input[type="email"]').next().length) )
						$('input[type="email"]').after("<p class='error-inf'>Неверный email</p>");
				}
			}
		});
	});
});

