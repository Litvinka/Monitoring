$(function () {

    //$("#tel").mask("(99) 999-9999");
    //$('#tel-edu').mask("(99) 999-9999");
    //var rightPass = 0;

    $.each($('#user-reg-form #user-registration .form-control:not(input[rel="gp"])'), function () {
        $(this).on('focusout', function () {
            if (($(this).val().length < 2) || ($(this).val() === '(__) ___-____')) {
                $(this).addClass('error');
                if (!($(this).next().hasClass('error-inf'))) {
                    $(this).after("<p class='error-inf'>Заполните поле</p>");
                }
            }
            else {
                $(this).removeClass('error');
                $(this).next().remove();
            }
            var regexp = new RegExp(/^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/);
            if ($('input[type="email"]').length && $('input[type="email"]').val().length > 3) {
                if (!(regexp.test($('input[type="email"]').val()))) {
                    $('input[type="email"]').removeClass('error');
                    $('input[type="email"]').next().remove();
                    $('input[type="email"]').addClass('error');
                    if (!($('input[type="email"]').next().length))
                        $('input[type="email"]').after("<p class='error-inf'>Неверный email</p>");
                }
            }
        });
    });
    $('input[name=user-role]').on('change', function () {
        var role = $('input[name=user-role]:checked').attr('id');
        if (role == "open-org-kur" || role == "open-org-kont") {
            $('#div-org').show();
            $('#div-org input').attr("disabled", false);
        }
        else {
            $('#div-org').hide();
            $('#div-org input').attr("disabled", true);
        }
    });

});

$("#user-reg-form.edit_user").submit(function (e) {
    if ($("#password_old").val() != "" || $("#password").val() != "" || $("#password_new").val() != "") {
        //if ($("#password_old").val() != $("#pass").val()) {
        //    if (!($('#password_old').next().length)) {
        //        $('#password_old').after("<p class='error-inf'>Неверный пароль</p>");
        //    }
        //    e.preventDefault();
        //}
        if ($("#password").val().trim() == "" || ($("#password").val() != $("#password_new").val())) {
            e.preventDefault();
            if (!($('#password_new').next().length)){
                $('#password_new').after("<p class='error-inf'>Новые пароли не совпадают или не заполнены</p>");
            }
         }
    }
});


$("#user-reg-form.registration_user_form").submit(function (e) { 
    e.preventDefault();
    email = $("#email1").val();
    var form = $(this);
    $.ajax({
        type: 'POST',
        url: '/Users/HaveUserByEmail?email=' +email,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {
            console.log(data);
            if (data == true) {
                if (!($('#email1').next().hasClass('error-inf'))) {
                    $('#email1').after("<p class='error-inf'>Такой email уже есть в базе данных</p>");
                }
            }
            else {
                $("#user-reg-form").removeClass("registration_user_form");
                form[0].submit();
            }
        }
    });

});