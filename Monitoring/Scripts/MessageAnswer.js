$("document").ready(function () {

    //Сообщение админитратору
    $("#dialog_form_admin").dialog({
        autoOpen: false,
    });
    $("#messageAdmin").on("click", function (e) {
        e.preventDefault();
        $("#div_dialog_form").fadeIn();
        $("#dialog_form_admin").dialog("open");
    });
    $("#cancel").on("click", function (e) {
        e.preventDefault();
        $("#dialog_form_admin").dialog("close");
    });
    $('#dialog_form_admin').on('dialogclose', function (event) {
        $("#div_dialog_form").fadeOut();
    });
    $("#send").on("click", function (e) {
        //e.preventDefault();
        //
    });
    $("#dialog_form_admin").submit(function (e) {
        e.preventDefault();
        $.ajax({
            type: "POST",
            url: "/Users/SendMessage",
            data: $("#dialog_form_admin").serialize(), 
            success: function () {
                $("#dialog_form_admin").dialog("close");
                location.href = "/Messages/Index?type=2";
            }
        });
    });
    //Сообщение админитратору


    //send answer for message
    $('.one_event a.btn-success').click(function (e) {
        e.preventDefault();
        var opt = {
            autoOpen: false,
            modal: true,
            width: 550,
            title: 'Ответ на сообщение',
        }
        $("#dialog").dialog(opt).dialog("open");
    });
    $("#send_answer").submit(function (e) {
        e.preventDefault();
        $.ajax({
            type: "POST",
            url: "/Users/SendMessage",
            data: $("#send_answer").serialize(),
            success: function () {
                $("#dialog").dialog("close");
                location.href = "/Messages/Index?type=2";
            }
        });
    });
    //send answer for message (end)


});