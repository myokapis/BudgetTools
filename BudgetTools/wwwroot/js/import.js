(function importPageScript() {

    $(document).ready(function () {
        bindEvents();
    });

    function bindEvents() {
        $(".file-button").click(fileButtonClick)
        $("input[type='file']").change(fileChange);
    }

    function fileButtonClick() {
        let elem = $(this);
        let td = elem.parent();
        $(td.find("input")[0]).click();
    }

    function fileChange() {

        let elem = $(this);
        let td = elem.parent();
        let tr = td.parent();
        let file = td.find("input[type='file']")[0].files[0];
        let fileName = file.name;
        let div = $(td.find("div")[0]);
        div.text("Uploading " + fileName);

        master.postForm("Import/ImportFile",
            function (data, textStatus, jqXHR) {
                div.text("Loaded " + fileName);
                $(file).val("");
            },
            function (xhr, textStatus, errorThrown) {
                div.text("Failed! " + fileName);
            },
            function (formData) {
                formData.append("file", file, fileName);
                formData.append("bankAccountId", $(tr).data("id"));
                //master.openErrorDialog({ title: "File Upload" }, "Success!");
            }
        );

    }

})();
