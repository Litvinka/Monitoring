$("#get_sites").click(function() {
    event.preventDefault();
    $.ajax({
        type: 'POST',
        url: '/Users/GetJsonEdu',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json'
    });
    $("#get_sites.active").css("display", "none");
    $("#link_get_sites").fadeIn();
});