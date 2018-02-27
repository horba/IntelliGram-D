function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length === 2)
        return parts.pop().split(";").shift();
}
function initConfigPage() {

    $.ajax('api/Statistic/Photos', {
        method: 'GET',
        dataType: 'json',
        success: function (response) {
            $('#statPhotos').html(response);
        }
    });
    $.ajax('api/Statistic/Tags', {
        method: 'GET',
        dataType: 'text',
        success: function (response) {
            $('#statTags').html(response);
        }
    });
    $.ajax('api/Instagram/Nick', {
        method: 'GET',
        dataType: 'text',
        success: function (response) {
            if (response) {
                $("#nickInst").show();
                $("#verifyInst").hide();
                $("#nickLink").html(response);
                $("#nickLink").attr("href", "https://www.instagram.com/" + response + "/?hl=en");
            }
            else {
                $("#nickInst").hide();
                $("#verifyInst").show();
            }
        }
    });
    $.ajax('api/Statistic/Topics', {
        method: 'GET',
        dataType: 'json',
        success: function (response) {
            $('#statTopics').html(response);
        }
    });

    $.ajax('/api/configuration/VerifyKey', { method: 'GET' })
        .then(function (response) {
            $("#verifyKeyLabel").text(response.verifyKey);
            $("#authPage").hide();
            $("#btnSignup").hide();
            $("#btnLogin").hide();
            $("#logOut").show();
            $("#configPage").show();
            initItems();
        });
}
if (getCookie("sessionID")) {
    initConfigPage();
}