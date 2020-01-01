$(document).ready(function () {
    setHeaders();
    bindEvents();
});

function bindEvents() {
    $("#closeCurrentPeriod").on("click", getCloseCurrentPeriod);
    $("#transferBalance").on("click", getBalanceTransfer);
}

function bindTransferEvents() {
    $("#bankAccountFrom").on("change", changeBankAccount);
    $("#bankAccountTo").on("change", changeBankAccount);
    $("#transfer").on("click", saveTransfer);
    $("#amount").on("blur", validateTransfer);
    $("#budgetLinesFrom").on("click", "tr", "From", selectTransferLine);
    $("#budgetLinesTo").on("click", "tr", selectTransferLine);
}

function bindClosePeriodEvents() {
    $("#closePeriod").on("click", closeCurrentPeriod);
}

function changeBankAccount() {
    var direction = $(this).attr("id") == "bankAccountFrom" ? "From" : "To";

    var input = {
        "bankAccountId": Number($(this).children("option:selected").val()),
        "direction": direction
    };

    $.ajax({
        url: 'Admin/GetTransferBudgetLines',
        type: "POST",
        data: JSON.stringify(input),
        dataType: 'html',
        contentType: 'application/json; charset=utf-8',
        success: function (data, textStatus, jqXHR) {
            var selector = "#budgetLines" + direction + " tbody";
            $(selector).html(data);
        },
        error: function (xhr, textStatus, errorThrown) {
            alert('Error: ' + xhr.statusText);
            alert(errorThrown);
            alert(xhr.responseText);
        }
    });
}

function closeCurrentPeriod() {
    // hide the messages
    $("#messages").hide();

    $.ajax({
        url: 'Admin/CloseCurrentPeriod',
        type: "POST",
        //data: JSON.stringify(data),
        dataType: 'html',
        contentType: 'application/json; charset=utf-8',
        success: function (data, textStatus, jqXHR) {
            $("#messages tbody").html(data);
            $("#messages").show();
        },
        error: function (xhr, textStatus, errorThrown) {
            alert('Error: ' + xhr.statusText);
            alert(errorThrown);
            alert(xhr.responseText);
        }
    });
}

function getBalanceTransfer() {
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
        "bankAccountFromId": parseInt($("#bankAccountFrom option:selected").val()),
        "budgetLineFromId": parseInt($("#budgetLinesFrom .tr - selected").val()),
        "bankAccountToId": parseInt($("#bankAccountTo option:selected").val()),
        "budgetLineToId": parseInt($("#budgetLinesTo .tr-selected").val()),
        "amount": getNumber($("#amount")),
        "note": $("#transferNotes").val()
    };

    $.ajax({
        url: 'Admin/SaveTransfer',
        type: "POST",
        data: JSON.stringify(inputData),
        dataType: 'html',
        contentType: 'application/json; charset=utf-8',
        success: function (data, textStatus, jqXHR) {
            $("#transferMessages tbody").html(data);
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
}

function setHeaders() {

}

function validateTransfer() {
    // check if both grids have a selected row and an amount exists
    var hasData = $("#budgetLinesFrom .selectedRow") && $("#budgetLinesTo .selectedRow")
        && $("#amount").val().trim() !== "";

    // hide messages and bail out if data is incomplete
    if (!hasData) {
        $("#actionButtons").hide();
        $("#transferMessage").text("");
        $("#transferMessage").hide();
        return;
    }

    // check if the amount is valid
    var isValidAmount = Number($("#amount").val()) > 0.0;

    // show/hide message and button(s)
    var msg = isValidAmount ? "Enter a valid amount." : "";
    $("#transferMessage").text(msg);
    if (isValidAmount) $("#actionButtons").show(); else $("#actionButtons").hide();
}