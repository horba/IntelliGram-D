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
$("#loginSubmit").click(function (e) {
    e.preventDefault();
    $.ajax({
        type: $loginForm.attr('method'),
        url: $loginForm.attr('action'),
        data: $loginForm.serialize(),
        dataType: "json",
        success: function (response) {
            document.cookie = "sessionID=" + response.sessionID;
            $("#configPage").css("display", "block");
            $("#authPage").css("display", "none");
        },
        error: function (response) {
            $("#loginError").text(response.responseJSON.errorMessage);
        }
    });
});