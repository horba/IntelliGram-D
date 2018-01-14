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
    $("#saveConfig").click(function (e) {
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