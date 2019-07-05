$(document).ready(function () {
    if ($("#enter-name-edu").length) {
        $.ajax({
            type: 'POST',
            url: '/Users/GetNameEdu',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (data) {
                var sub = data.substr(2, data.length - 4);
                sub = sub.replace(/\\r/g, "");
                sub = sub.replace(/\\n/g, "");
                sub = sub.replace(/\\"/g, "\"");
                var arr = sub.split('","');
                $("#enter-name-edu").autocomplete({
                    source: arr
                });
            }
        });
    }

    // Отправка отзыва о сайте. Получается список всех учреждений из базы данных
    if ($("#enter_type_edu").length) {
        $.ajax({
            type: 'POST',
            url: '/Users/GetNameEdu',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (data) {
                var sub = data.substr(2, data.length - 4);
                sub = sub.replace(/\\r/g, "");
                sub = sub.replace(/\\n/g, "");
                sub = sub.replace(/\\"/g, "\"");
                var arr = sub.split('","');
                $("#enter_type_edu").autocomplete({
                    source: arr
                });
            }
        });
    }


    if ($('#area').length && $("#district").length) {
        $('#district').attr("disabled", true);
    }


    // Отправка отзыва о сайте. После выбора области, в поле с районами получаются из базы данных все районы данной области
    $("#area").change(function () {
        if ($("#area").val()) {
            $.ajax({
                type: 'POST',
                url: '/Users/GetAllDistrict/',
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: JSON.stringify({ 'param': $('#area').val() }),
                success: function (data) {
                    data = data.replace(/\]/g, "");
                    data = data.replace(/\[/g, "");
                    data = data.replace(/\"/g, "");
                    var arr = data.toString().split(',');
                    $('#district').attr("disabled", false);
                    document.getElementById('district').innerHTML = '';
                    $('#district').append(new Option(arr[i + 1]));
                    for (var i = 0; i < arr.length; ++i) {
                        $('#district').append(new Option(arr[i + 1], arr[i]));
                        ++i;
                    }
                }
            });
            ChangeEdu();
        }
        else {
            document.getElementById('district').innerHTML = '';
            $('#district').attr("disabled", true);
            ChangeEdu();
        }
    });


    //Отправка отзыва о сайте. Если пользователь выбрал район, изменяется список учреждений
    $("#district").change(function () {
        ChangeEdu();
    });


    //Отправка отзыва о сайте. Если пользователь выбрал тип учреждения, изменяется список учреждений
    $("#type_edu").change(function () {
        ChangeEdu();
    });


    //Изменение списка учреждений в зависимости от параметров (область, район, тип учреждения)
    function ChangeEdu() {
        $.ajax({
            type: 'POST',
            url: '/Users/GetNameEdu',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify({ 'area': $('#area').val(), 'district': $("#district").val(), 'type': $("#type_edu").val() }),
            success: function (data) {
                var sub = data.substr(2, data.length - 4);
                sub = sub.replace(/\\r/g, "");
                sub = sub.replace(/\\n/g, "");
                sub = sub.replace(/\\"/g, "\"");
                var arr = sub.split('","');
                $("#enter_type_edu").autocomplete({
                    source: arr
                });
            }
        });
        $("#enter_type_edu").val("");
    }

});


/* Отправка формы отзыва о сайте. Проверяется, есть ли учреждение с таким названием в базе данных. 
   Если учреждения в базе данных нет, то учреждение, на которое хотят оставить отзыв, не выбрано, и данные формы не будут отправлены */
$("#leave-feedback").submit(function (e) {
    e.preventDefault();
    $.ajax({
        context: this,
        type: 'POST',
        url: '/Users/FindEdu',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify({ 'name': $("#enter_type_edu").val() }),
        success: function (data) {
            if (data != 0) {
                if ($(".row .form-group p.error").length>0){
                    $(".row .form-group p.error").remove();
                }
                this.submit();
            }
            else {
                $("#enter_type_edu").before("<p class='error'>Выберите учреждение</p>");
            }
        }
    });  
});