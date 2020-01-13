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
        $("#closeCurrentPeriod").on("click", getCloseCurrentPeriod);
        $("#transferBalance").on("click", getBalanceTransfer);
    }

    function bindTransferEvents() {
        $("#transfer").on("click", saveTransfer);
        $("#amount").on("blur", formatTransferAmount);
        $("#amount").on("blur", validateTransfer);
        $("#budgetLinesFrom").on("click", "tr", selectTransferLine);
        $("#budgetLinesTo").on("click", "tr", selectTransferLine);
    }

    function bindClosePeriodEvents() {
        $("#closePeriod").on("click", closeCurrentPeriod);
    }

    function changeBankAccount() {
        switch (adminPageScript.menuSelection) {
            case MenuSelection.Allocations:

                break;
            case MenuSelection.TransferBalance:
                changeBankAccountTransfer();
                break;
            default:
                changeBankAccountNone();
                break;
        }
    }

    function changeBankAccountTransfer() {
        var input = {
            "bankAccountId": parseInt($("#bankAccountId").val())
        };

        $.ajax({
            url: 'Admin/GetTransferBudgetLines',
            type: "POST",
            data: JSON.stringify(input),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $("#budgetLinesFrom tbody").html(data.budgetLinesFrom);
                $("#budgetLinesTo tbody").html(data.budgetLinesTo);
            },
            error: function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            }
        });
    }

    function changeBankAccountNone() {
        var input = {
            "bankAccountId": parseInt($("#bankAccountId").val())
        };

        $.ajax({
            url: 'Admin/ChangeBankAccount',
            type: "POST",
            data: JSON.stringify(input),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            error: function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            }
        });
    }

    function closeCurrentPeriod() {
        // hide the messages
        $("#closePeriodMessages").hide();

        $.ajax({
            url: 'Admin/CloseCurrentPeriod',
            type: "POST",
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            success: function (data, textStatus, jqXHR) {
                $("#closePeriodMessages tbody").html(data);
                $("#closePeriodMessages").show();
            },
            error: function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            }
        });
    }

    function formatTransferAmount() {
        setCurrency($("#amount"));
    }

    function getBalanceTransfer() {
        adminPageScript.menuSelection = MenuSelection.TransferBalance;

        $.ajax({
            url: 'Admin/GetBalanceTransfer',
            type: "GET",
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            success: function (data, textStatus, jqXHR) {
                $(".editor-section-div").html(data);
                bindTransferEvents();
            },
            error: function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            }
        });
    }

    function getCloseCurrentPeriod() {
        adminPageScript.menuSelection = MenuSelection.ClosePeriod;

        $.ajax({
            url: 'Admin/GetCloseCurrentPeriod',
            type: "GET",
            //data: JSON.stringify(data),
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            success: function (data, textStatus, jqXHR) {
                $(".editor-section-div").html(data);
                bindClosePeriodEvents();
            },
            error: function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            }
        });
    }

    function saveTransfer() {
        var inputData = {
            "bankAccountId": parseInt($("#bankAccountId option:selected").val()),
            "budgetLineFromId": parseInt($("#budgetLinesFrom .tr-selected").attr("data-id")),
            "budgetLineToId": parseInt($("#budgetLinesTo .tr-selected").attr("data-id")),
            "amount": getFixedNumeric($("#amount").val()),
            "note": $("#transferNotes").val()
        };

        $.ajax({
            url: 'Admin/SaveTransfer',
            type: "POST",
            data: JSON.stringify(inputData),
            dataType: 'html',
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $("#transferMessages tbody").html(data.messages);
                $("#transferMessages").show();

                if (data.isSuccess) {
                    $("#budgetLinesFrom tbody").html(data.budgetLinesFrom);
                    $("#budgetLinesTo tbody").html(data.budgetLinesTo);
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            }
        });
    }

    function selectTransferLine() {
        // get element references
        var clickedRow = $(this);
        var table = $(this).parent().parent()[0];

        // find selected cells
        var selectedRow = $(table).find(".tr-selected");

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

    function validateTransfer() {
        $("#transferMessages").hide();

        // check if both grids have a selected row and an amount exists
        var hasData = $("#budgetLinesFrom .tr-selected").length > 0
            && $("#budgetLinesTo .tr-selected").length > 0
            && $("#amount").val().trim() !== "";

        // hide messages and bail out if data is incomplete
        if (!hasData) {
            $("#actionButtons").hide();
            $("#transferMessage").text("");
            $("#transferMessage").hide();
            return;
        }

        // check if the amount is valid
        var isValidAmount = getFixedNumeric($("#amount").val()) > 0.0;

        // show/hide message and button(s)
        var msg = isValidAmount ? "Enter a valid amount." : "";
        $("#transferMessage").text(msg);
        if (isValidAmount) $("#actionButtons").show(); else $("#actionButtons").hide();
    }

})();
