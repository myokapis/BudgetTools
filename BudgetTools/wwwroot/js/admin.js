(function adminPageScript() {

    const MenuSelection = Object.freeze({
        None: "None",
        Allocations: "Allocations",
        ClosePeriod: "ClosePeriod:",
        TransferBalance: "TransferBalance"
    });

    adminPageScript.menuSelection = MenuSelection.None;

    $(document).ready(function () {
        setHeaders();
        bindEvents();
    });

    function bindEvents() {
        $("#bankAccountId").on("change", changeBankAccount);
        $("#closeCurrentPeriod").on("click", showCloseCurrentPeriod);
        $("#transferBalance").on("click", showBalanceTransfer);
        $(".editor-section-div").on("click", "#closePeriod", closeCurrentPeriod);
        $(".editor-section-div").on("click", "#transfer", saveTransfer);
        $(".editor-section-div").on("blur", "#amount", formatTransferAmount);
        $(".editor-section-div").on("blur", "#amount", validateTransfer);
        $(".editor-section-div").on("click", "#budgetLinesFrom tr", selectTransferLine);
        $(".editor-section-div").on("click", "#budgetLinesTo tr", selectTransferLine);
    }

    function changeBankAccount() {
        switch (adminPageScript.menuSelection) {
            case MenuSelection.Allocations:

                break;
            case MenuSelection.TransferBalance:
                changeBankAccountTransfer();
                break;
            default:
                break;
        }
    }

    function changeBankAccountTransfer() {
        master.postForm("Admin/ShowTransferBudgetLines",
            function (data) {
                $("#budgetLinesFrom tbody").html(data.budgetLinesFrom);
                $("#budgetLinesTo tbody").html(data.budgetLinesTo);
            },
            function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            });
    }

    function closeCurrentPeriod() {
        $("#closePeriodMessages").hide();

        master.postForm("Admin/CloseCurrentPeriod",
            function(data) {
                $("#closePeriodMessages tbody").html(data.html);
                $("#closePeriodMessages").show();
                $("#currentPeriodId").val(data.pageScope.CurrentPeriodId);
                $("#periodId").val(data.pageScope.PeriodId);
            },
            function(xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            });
    }

    function formatTransferAmount() {
        master.setCurrency($("#amount"));
    }

    function showCloseCurrentPeriod() {
        adminPageScript.menuSelection = MenuSelection.ClosePeriod;

        master.postForm("Admin/ShowCloseCurrentPeriod",
            function (data, textStatus, jqXHR) {
                $(".editor-section-div").html(data.html);
            },
            function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            });

    }

    function saveTransfer() {

        master.postForm("Admin/SaveTransfer",
            function(data) {
                $("#transferMessages tbody").html(data.messages);
                $("#transferMessages").show();
                console.debug(data);
                if (data.isSuccess) {
                    $("#budgetLinesFrom tbody").html(data.budgetLinesFrom);
                    $("#budgetLinesTo tbody").html(data.budgetLinesTo);
                }
            },
            function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            },
            function(formData) {
                formData.append("budgetLineFromId", parseInt($("#budgetLinesFrom .tr-selected").attr("data-id")));
                formData.append("budgetLineToId", parseInt($("#budgetLinesTo .tr-selected").attr("data-id")));
                formData.append("amount", master.getFixedNumeric($("#amount").val()));
                formData.append("note", $("#transferNotes").val());
            });

    }

    function selectTransferLine() {
        // get element references
        let clickedRow = $(this);
        let table = $(this).parent().parent()[0];

        // find selected cells
        let selectedRow = $(table).find(".tr-selected");

        // remove selection
        $(selectedRow).removeClass("tr-selected");

        // bail out if the clicked row was the selected row
        if (selectedRow.length > 0 && $(selectedRow[0]).is(clickedRow)) return;

        // select the clicked row
        $(clickedRow).addClass("tr-selected");

        validateTransfer();
    }

    function setHeaders() {

    }

    function showBalanceTransfer() {
        adminPageScript.menuSelection = MenuSelection.TransferBalance;

        master.postForm("Admin/ShowBalanceTransfer",
            function(data) {
                $(".editor-section-div").html(data.html);
            },
            function(xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            });
    }

    function validateTransfer() {
        $("#transferMessages").hide();

        // check if both grids have a selected row and an amount exists
        let hasData = $("#budgetLinesFrom .tr-selected").length > 0
            && $("#budgetLinesTo .tr-selected").length > 0
            && $("#amount").val().trim() !== "";
        console.debug(hasData);
        // hide messages and bail out if data is incomplete
        if (!hasData) {
            $(".actionButtons").prop("disabled", true);
            $("#transferMessage").text("");
            $("#transferMessage").hide();
            return;
        }

        // check if the amount is valid
        let isValidAmount = master.getFixedNumeric($("#amount").val()) > 0.0;
        console.debug(isValidAmount);
        // show/hide message and button(s)
        let msg = isValidAmount ? "Enter a valid amount." : "";
        $("#transferMessage").text(msg);
        $(".actionButtons").prop("disabled", !isValidAmount);
        console.debug($($(".actionButtons")[0]).prop("disabled"));
    }

})();
