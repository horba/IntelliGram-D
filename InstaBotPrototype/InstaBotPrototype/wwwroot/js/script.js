$(function () {
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
    $("#save_config").click(function (e) {
        e.preventDefault();
        let data = {
            instaUsername: $("input[name='instaUsername']").val(),
            instaPassword: $("input[name='instaPassword']").val(),
            telegramUsername: $("input[name='telegramUsername']").val(),
        };

        let tags = [];
        let splitTags = $("input[name='tags']").val().split(',');
        splitTags.forEach(tag => tags.push({ tag: tag }));

        let topics = [];
        let splitTopics = $("input[name='topics']").val().split(',');
        splitTopics.forEach(topic => topics.push({ topic: topic }));

        data['tags'] = tags;
        data['topics'] = topics;

        $.ajax('/api/configuration', { method: 'POST', data: $.param(data) })
            .then(function (response) {
                console.log('success', response);
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