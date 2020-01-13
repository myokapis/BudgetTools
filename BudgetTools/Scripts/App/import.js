$(document).ready(function () {
    bindEvents();
});

function bindEvents() {
    $(".file-button").click(fileButtonClick)
    $("input[type='file']").change(fileChange);
}

function fileButtonClick() {
    var elem = $(this);
    var td = elem.parent();
    $(td.find("input")[0]).click();
}

function fileChange() {

    var elem = $(this);
    var td = elem.parent();
    var tr = td.parent();
    var file = td.find("input[type='file']")[0].files[0];
    var fileName = file.name;
    var div = $(td.find("div")[0]);
    div.text("Uploading " + fileName);

    var formData = new FormData();
    formData.append("bankAccountId", tr.attr("data-id"));
    formData.append("file", file, fileName);

    $.ajax({
        url: 'Import/ImportFile',
        type: "POST",
        cache: false,
        data: formData,
        dataType: 'html',
        contentType: false,
        processData: false,
        success: function (data, textStatus, jqXHR) {
            div.text("Loaded " + fileName);
            file.val("");
        },
        error: function (xhr, textStatus, errorThrown) {
            div.text("Failed! " + fileName);
        }
    });

}