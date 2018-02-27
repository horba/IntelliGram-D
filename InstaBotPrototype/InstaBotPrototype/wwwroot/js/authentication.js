$("#btnLogin").click(function () {
    $("#authPage").show();
    $("#signIn").show();
});
$("#btnSignup").click(function () {
    $("#authPage").show();
    $("#signUp").show();
});
$("#signUpCancel").click(function (e) {
    e.preventDefault();
    $("#authPage").hide();
    $("#signUp").hide();
    $("#signUpError").hide();
    $("#modalSignup")[0].reset();
});
$("#signInCancel").click(function (e) {
    e.preventDefault();
    $("#authPage").hide();
    $("#signIn").hide();
    $("#loginError").hide();
    $("#modalLogin")[0].reset();
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
    $("#authPage").hide();
    $("#signUp").hide();
    $("#signIn").hide();
    $("#logOut").hide();
    $("#configPage").hide();
    $("#btnSignup").show();
    $("#btnLogin").show();
    $("#authPage").hide();
    $("#modalLogin")[0].reset();
    $("#modalSignup")[0].reset();
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
            $(errorDiv).hide();
            initConfigPage();
        },
        error: function (response) {
            $(errorDiv).show();
            $(errorDiv).text(response.responseJSON.errorMessage);
        }
    });
}