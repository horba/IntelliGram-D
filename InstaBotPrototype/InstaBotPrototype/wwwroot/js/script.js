$(function () {//jquery magic to run script after page fully loaded
    $("#getConfig").click(function (e) {
        e.preventDefault();
        $.get("/api/configuration").done(function (data) {
            let values = data;
            for (var fieldName in values) {
                if (values.hasOwnProperty(fieldName)) {
                    $('#configForm input[name=' + fieldName + ']').val(values[fieldName]);//magic - putting values into corresponding fields
                }
            };
        });
    });
    $("#saveConfig").click(function (e) {
        e.preventDefault();
        let data = $("#configForm").serialize();//magic - gathering data from fields into js object, ready for sending via post
        $.post("/api/configuration", data).done(function (response, status, xhm) {
            console.log(response);
            $("#ajaxResult").text("Save status: " + status);
        });

    });
});