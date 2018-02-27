function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length === 2)
        return parts.pop().split(";").shift();
}
function initConfigPage() {
    $.ajax('/api/configuration/VerifyKey', { method: 'GET' })
        .then(function (response) {
            $("#verifyKeyLabel").text(response.verifyKey);
            $("#configPage").show();
            $("#authPage").hide();
            initItems();
        });
}
if (getCookie("sessionID")) {
    initConfigPage();
}
else {
    $("#configPage").hide();
    $("#authPage").show();
}