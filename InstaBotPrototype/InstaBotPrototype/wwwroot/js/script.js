$(function () {//jquery magic to run script after page fully loaded
    $("#get_config").click(function (e) {
        e.preventDefault();
        $.get("/api/configuration").done(function (data) {
            let values = data;
            for (var fieldName in values) {
                if (values.hasOwnProperty(fieldName)) {
                    $('#config_form input[name=' + fieldName + ']').val(values[fieldName]);//magic - putting values into corresponding fields
                }
            };


        });
    });
    $("#save_config").click(function (e) {
        e.preventDefault();
        let data = $("#config_form").serialize();//magic - gathering data from fields into js object, ready for sending via post
        $.post("/api/configuration", data).done(function (response, status, xhm) {
            console.log(response);
            $("#ajax_result").text("Save status: " + status);
        });

    });
});
var btnLogin = document.getElementById("btnLogin");
var btnSignUp = document.getElementById("btnSignUp");
var modalLogin = document.getElementById("modalLoginWindow");
var modalSignUp = document.getElementById("modalSignUpWindow");
var spanExitLogin = document.getElementsByClassName("close")[0];
var spanExitSignUp = document.getElementsByClassName("close")[1];
var loginForm = document.getElementById("modalLogin");
var signUpForm = document.getElementById("modalSignUp");
var authPage = document.getElementById("authPage");
var configPage = document.getElementById("configPage");
btnLogin.onclick = function () {
    modalLogin.style.display = "block";
}
spanExitLogin.onclick = function () {
    modalLogin.style.display = "none";
}
btnSignUp.onclick = function () {
    modalSignUp.style.display = "block";
}
spanExitSignUp.onclick = function () {
    modalSignUp.style.display = "none";
}
loginForm.onsubmit = function (event) {
    event.preventDefault();
    configPage.style.display = "block";
    authPage.style.display = "none";
}
signUpForm.onsubmit = function (event) {
    event.preventDefault();
    configPage.style.display = "block";
    authPage.style.display = "none";
}