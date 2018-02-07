$("#btnLogin").click(function () {
    $("#loginPopup").css("display", "block");
});
$("#closeLogin").click(function () {
    $("#loginPopup").css("display", "none");
    $("#loginError").text("");
});
$("#btnSignup").click(function () {
    $("#signupPopup").css("display", "block");
});
$("#closeSignUp").click(function () {
    $("#signupPopup").css("display", "none");
    $("#signupError").text("");
});

var $loginForm = $("#modalLogin");
$loginForm.submit(function (event) {
    clickHandler(event, "#loginError", $loginForm);
});
var $signUpForm = $("#modalSignup");
$signUpForm.submit(function (event) {
    clickHandler(event, "#signUpError", $signUpForm);
});
$("#logOut").click(function (e) {
    e.preventDefault();
    document.cookie = "sessionID" + '=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    $("#loginPopup").css("display", "none");
    $("#signupPopup").css("display", "none");
    $("#configPage").css("display", "none");
    $("#authPage").css("display", "block");
});
function clickHandler(event, errorDiv, $form) {
    event.preventDefault();
    $.ajax({
        type: $form.attr('method'),
        url: $form.attr('action'),
        data: $form.serialize(),
        dataType: "json",
        success: function (response) {
            document.cookie = "sessionID=" + response.sessionID;
            initCongigPage();
        },
        error: function (response) {
            $(errorDiv).text(response.responseJSON.errorMessage);
        }
    });
}