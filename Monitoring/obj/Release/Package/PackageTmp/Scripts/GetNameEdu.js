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
                console.log($("#enter_type_edu").length);
                $("#enter_type_edu").autocomplete({
                    source: arr
                });
            }
        });
    }


    if ($('#area').length && $("#district").length) {
        $('#district').attr("disabled", true);
    }
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

    $("#district").change(function () {
            ChangeEdu();
    });

    $("#type_edu").change(function () {
        ChangeEdu();
    });


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
                console.log($("#enter_type_edu").length);
                $("#enter_type_edu").autocomplete({
                    source: arr
                });
            }
        });
        $("#enter_type_edu").val("");
    }


});