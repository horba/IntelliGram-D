﻿$(function () {
    $("#getConfig").click(function (e) {
        e.preventDefault();
        $.get("/api/configuration").done(function (data) {
            let values = data;
            for (var fieldName in values) {
                if (values.hasOwnProperty(fieldName)) {
                    $('#configForm input[name=' + fieldName + ']').val(values[fieldName]);
                }
            };


        });
    });
    $("#saveConfig").click(function (e) {
        e.preventDefault();
        let data = $("#configForm").serialize();
        $.post("/api/configuration", data).done(function (response, status, xhm) {
            console.log(response);
            $("#ajaxResult").text("Save status: " + status);
        });

    });
});

document.getElementById('labelNumber').innerHTML = 100000000000000000 * Math.random();

var btnLogin = document.getElementById("btnLogin");
var btnSignUp = document.getElementById("btnSignup");
var modalLogin = document.getElementById("loginPopup");
var modalSignUp = document.getElementById("signupPopup");
var spanExitLogin = document.getElementById("closeLogin");
var spanExitSignUp = document.getElementById("closeSignup");
var loginForm = document.getElementById("modalLogin");
var signUpForm = document.getElementById("modalSignup");
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