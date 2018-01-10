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
function clickHandler(event,errorDiv,$form) {
    event.preventDefault();
    $.ajax({
        type: $form.attr('method'),
        url: $form.attr('action'),
        data: $form.serialize(),
        dataType: "json",
        success: function (response) {
            document.cookie = "sessionID=" + response.sessionID;
            $("#configPage").css("display", "block");
            $("#authPage").css("display", "none");
            $("#verifyKeyLabel").text(response.verifyKey);
        },
        error: function (response) {
            $(errorDiv).text(response.responseJSON.errorMessage);
        }
    });
}