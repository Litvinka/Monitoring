//delete user
$('.user_list button.btn-danger').click(function (e) {
    var opt = {
        autoOpen: false,
        modal: true,
        width: 550,
        title: 'Вы уверены?',
        buttons: {
            "Удалить": function () {
                if (e.target.tagName == "BUTTON") {
                    var id = $(e.target).siblings(".user_id").first().val(); //get id for delete user
                }
                else {
                    var id = $(e.target).parent().siblings(".user_id").first().val(); //get id for delete user
                }
                $.ajax({
                    type: 'POST',
                    url: '/Users/Delete/' + id,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    success: function () {
                        //$('#user_list').load('/Users/Index #user_list');
                        location.reload();
                    }
                });
                $(this).dialog("close");
            },
            "Отмена": function () {
                $(this).dialog("close");
            }
        }
    };
    $("#dialog").dialog(opt).dialog("open");
});  
//delete user (end)


//delete review
$('.review a.btn-danger').click(function (e) {
    e.preventDefault();
    var opt = {
        autoOpen: false,
        modal: true,
        width: 550,
        title: 'Вы уверены?',
        buttons: {
            "Удалить": function () {
                var id = $("#review_id").val();
                $.ajax({
                    type: 'POST',
                    url: '/Reviews/Delete/' + id,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    success: function () {
                        window.location.href = "/Reviews/Index";
                    }
                });
                $(this).dialog("close");
            },
            "Отмена": function () {
                $(this).dialog("close");
            }
        }
    };
    $("#dialog").dialog(opt).dialog("open");
});
//delete review (end)


//delete institution
$('#institutions_list button.btn-danger').click(function (e) {
    var opt = {
        autoOpen: false,
        modal: true,
        width: 550,
        title: 'Вы уверены?',
        buttons: {
            "Удалить": function () {
                if (e.target.tagName == "BUTTON") {
                    var id = $(e.target).siblings(".institution_id").first().val(); //get id for delete user
                }
                else {
                    var id = $(e.target).parent().siblings(".institution_id").first().val(); //get id for delete user
                }
                
                $.ajax({
                    type: 'POST',
                    url: '/Institution/Delete/' + id,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    success: function () {
                       window.location.href = "/Institution/Index";
                    }
                });
                $(this).dialog("close");
            },
            "Отмена": function () {
                $(this).dialog("close");
            }
        }
    };
    $("#dialog").dialog(opt).dialog("open");
});
//delete institution (end)




