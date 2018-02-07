function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length == 2)
        return parts.pop().split(";").shift();
}
function initCongigPage() {
    $.ajax('/api/configuration/VerifyKey', { method: 'GET' })
        .then(function (response) {
            $("#verifyKeyLabel").text(response.verifyKey);
            $("#configPage").css("display", "block");
            $("#authPage").css("display", "none");
        });
}
if (getCookie("sessionID")) {
    initCongigPage();
}
else {
    $("#configPage").css("display", "none");
    $("#authPage").css("display", "block");
}