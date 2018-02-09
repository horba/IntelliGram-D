$(function () {
    $("#saveConfig").click(function (e) {
        e.preventDefault();
        let data = {};

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

