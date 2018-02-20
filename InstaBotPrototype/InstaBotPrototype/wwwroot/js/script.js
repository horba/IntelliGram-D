let tags = [];
let topics = [];
const max = 12;

function itemClickHandler(event) {
    var array = event.data.itemArray;
    var $target = $(event.target).parent();
    var text = $target.text();
    text = text.substring(0, text.length - 3);
    $.ajax(event.data.deleteUrl, {
        method: 'DELETE',
        data: {item: text},
    });
    var index = array.indexOf(text);
    array.splice(index, 1);
    $target.remove();
    $(event.data.itemDiv).show();
}
function addItem(itemDiv, val, itemArray, deleteUrl) {
    $(itemDiv).val('');
    if (val.length > 0 && itemArray.length <= max) {
        var $newItem = $('<div class = "tag-topic">');
        var $closingSpan = $('<span class = "close"> x </span>');
        $newItem.append(val);
        $newItem.append($closingSpan);
        $newItem.insertBefore($(itemDiv));
        $closingSpan.click({ itemArray: itemArray, itemDiv: itemDiv,deleteUrl: deleteUrl }, itemClickHandler);
    }
    if (itemArray.length === max) {
        $(itemDiv).hide();
    }
}
function addTagFromInput(input, itemArray, val, url, deleteUrl) {
    if (val.length > 0 && !itemArray.includes(val) && itemArray.length < max) {
        itemArray.push(val);
        addItem(input, val, itemArray, deleteUrl);
        $.ajax(url, {
            method: 'POST',
            data: { item: val },
            dataType: 'json'
        });
    }
}
function setInputClickHandler(input, itemArray, url, deleteUrl) {
    itemArray.forEach(tag => addItem(input, tag, itemArray, deleteUrl));
    $(input).click(function (e) {
        var val = $(input).val();
        addTagFromInput(input, itemArray, val, url, deleteUrl);
    }
  );
}
function fillAutoComplete(completeElement, itemsList) {
    let elems = $(completeElement).toArray().reverse();
    for (var i = 0; i < elems.length; i++) {
        if (itemsList[i]) {
            $(elems[i]).show();
            $(elems[i]).text(itemsList[i]);
        }
        else {
            $(elems[i]).hide();
        }
    }
}
function inputChangeHandler(val,url,completeElem) {
    if (val.length > 0) {
        $.ajax(url, {
            method: 'GET',
            data: { item: val },
            dataType: 'json',
            success: function (response) {
                fillAutoComplete(completeElem, response)
            }
        })
    }
    else {
        fillAutoComplete(completeElem, []);
    }
}
function initItems() {
        $.ajax('/api/configuration/GetTags', {
            method: 'GET',
            dataType: 'json',
            success: function (response) {
               tags = response;
            }
        }).then(function () {
            setInputClickHandler("#tagInput", tags, 'api/Configuration/AddTag', 'api/Configuration/DeleteTag');
    });

    $.ajax('/api/configuration/GetTopics', {
            method: 'GET',
            dataType: 'json',
            success: function (response) {
                topics = response;
            }
        }).then(function () {
            setInputClickHandler("#topicInput", topics, 'api/Configuration/AddTopic', 'api/Configuration/DeleteTopic');
    });
    
    $(".tagComplete").click(function (e) {
        addTagFromInput('#tagInput', tags, $(e.target).text(), 'api/Configuration/AddTag', 'api/Configuration/DeleteTag');
        fillAutoComplete(".tagComplete", []);
    })
    $(".topicComplete").click(function (e) {
        addTagFromInput('#topicInput', topics, $(e.target).text(), 'api/Configuration/AddTopic', 'api/Configuration/DeleteTopic');
        fillAutoComplete(".topicComplete", []);
    })
    $("#tagInput").bind("change paste keyup", function (e) {
        let val = $(e.target).val();
        inputChangeHandler(val, '/api/Autocomplete/GetTags', ".tagComplete");
    })
    $("#topicInput").bind("change paste keyup", function (e) {
        let val = $(e.target).val();
        inputChangeHandler(val, '/api/Autocomplete/GetTopics',".topicComplete");
    })
}