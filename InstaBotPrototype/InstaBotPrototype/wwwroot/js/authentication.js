$("#btnLogin").click(function () {
    $("#signUp").css("display", "none");
    $("#btnLogin").css("background-color", "#1E90FF");
    $("#btnSignup").css("background-color", "#87CEFA");
    $("#signIn").css("display", "block");
});
$("#btnSignup").click(function () {
    $("#signIn").css("display", "none");
    $("#signUp").css("display", "block");
    $("#btnLogin").css("background-color", "#87CEFA");
    $("#btnSignup").css("background-color", "#1E90FF");
});
var $loginForm = $("#modalLogin");
$loginForm.submit(function (event) {
    event.preventDefault();
    clickHandler("#loginError", $loginForm);
});
var $signUpForm = $("#modalSignup");
$signUpForm.submit(function (event) {
    event.preventDefault();
    let errorDiv = "#signUpError";
    if ($("#password").val() === $("#confirm").val()) {
        clickHandler(errorDiv, $signUpForm);
    }
    else {
        $(errorDiv).text("Passwords are not matching");
    }

});
$("#logOut").click(function (e) {
    e.preventDefault();
    document.cookie = "sessionID" + '=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    $("#loginPopup").hide();
    $("#signupPopup").hide();
    $("#configPage").hide();
    $("#authPage").show();
    tags = [];
    topics = [];
    $(".tag-topic").remove();
});
function clickHandler(errorDiv, $form) {
    $.ajax({
        type: $form.attr('method'),
        url: $form.attr('action'),
        data: $form.serialize(),
        dataType: "json",
        success: function (response) {
            document.cookie = "sessionID=" + response.sessionID;
            initConfigPage();
        },
        error: function (response) {
            $(errorDiv).text(response.responseJSON.errorMessage);
        }
    });
}