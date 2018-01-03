$("#buttonLogin").click(function () {
    $("#loginModal").css("display", "block");
});
$("#closeLogin").click(function () {
    $("#loginModal").css("display", "none");
    $("#loginError").text("");
});
$("#buttonSignUp").click(function () {
    $("#signUpModal").css("display", "block");
});
$("#closeSignUp").click(function () {
    $("#signUpModal").css("display", "none");
});
var $loginForm = $("#loginForm");
$("#loginSubmit").click(function (event) {
    clickHandler(event, "#loginError", $loginForm);
});
var $signUpForm = $("#signUpForm");
$("#signUpSubmit").click(function (event) {
    clickHandler(event, "#signUpError", $signUpForm );
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
        },
        error: function (response) {
            $(errorDiv).text(response.responseJSON.errorMessage);
        }
    });
}