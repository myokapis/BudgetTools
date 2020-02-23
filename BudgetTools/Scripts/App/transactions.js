(function transactionsPageScript() {

    $(document).ready(function () {
        setHeaders();
        bindEvents();
        selectFirstRow();
    });

    function bindEvents() {
        $("#bankAccountId").on("change", changeBankAccount);
        $("#periodId").on("change", changePeriod);
        $("#transactions").on("click", "tbody tr", selectRow);
        $("#prev").on("click", prevRow);
        $("#next").on("click", nextRow);
        $("#save").on("click", saveRow);
        $(".editor-table tbody input").on("change", amountChanged);
        $(".editor-table tbody input").on("keyup", seekEnterKey);
        $(".editor-table select").on("change", lineChanged);
    }

    function amountChanged(event) {
        var field = $(event.currentTarget);
        setCurrency(field);
        calculateTotals();
    }

    function calculateTotals() {

        var total = 0.0;

        // get the transaction amount
        var amount = getNumber($('#amount').text());

        // get the editor table rows
        var rows = $('.editor-table tbody').find('tr');

        // sum the row amounts
        for (var i = 0; i < rows.length; i++) {
            input = $(rows[i]).find('input');
            value = input.val();
            total += getNumber(input.val());
        }

        setCurrency("#remainingAmount", amount - total);
    }

    function changeBankAccount() {
        var data = {
            bankAccountId: $("#bankAccountId").val()
        };

        // save the values
        $.ajax({
            url: 'Transactions/ChangeBankAccount',
            type: "POST",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (data, textStatus, jqXHR) {
                $("#transactions > tbody").html(data.transactions);
                setHeaders();
                selectFirstRow();
            },
            error: function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            }
        });
    }

    function changePeriod() {

        var data = {
            periodId: $("#periodId").val()
        };

        // save the values
        $.ajax({
            url: 'Transactions/ChangePeriod',
            type: "POST",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $("#transactions > tbody").html(data.transactions);
                //$(".editor-section-div").html(data.editor);
                setHeaders();
                selectFirstRow();
            },
            error: function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            }
        });
    }

    function fillEditor(data) {

        // fill the descriptive fields
        $('#transactionId').val(data.transactionId);
        setCurrency('#amount', Math.abs(data.amount));
        $('#amount').attr('data-sign', data.amount < 0.0 ? -1 : 1);
        $('#transactionType').val(data.transactionType);
        $('#recipient').val(data.recipient);
        $('#notes').val(data.notes);

        // get the editor table rows
        var rows = $('.editor-table tbody').find('tr');

        // get mapped transactions
        var mapped = data.mappedTransactions;
        var xactCount = mapped.length;

        for (var i = 0; i < 5; i++) {

            // get the row and its fields
            var row = $(rows[i]);
            var select = $(row).find('select');
            var input = $(row).find('input');

            // set the field values
            var values = i < xactCount ? mapped[i] : { budgetLineId: 0, amount: 0.0 };
            select.val(values.budgetLineId);
            setCurrency(input, Math.abs(values.amount));
        }
    }

    function lineChanged() {
        // check if the transaction is already balanced
        var remainingAmount = getNumber($("#remainingAmount").text());
        if (remainingAmount === 0.0) return;

        // get the input field and its value
        var inputField = $(this).parent().next().find("input")[0];
        var lineAmount = getNumber($(inputField).val());
        if (lineAmount !== 0.0) return;

        // set the amount for this line to the remaining balance
        setCurrency(inputField, remainingAmount);

        // trigger the change event on the input field
        $(inputField).change();
    }

    function nextRow() {

        // get the transaction id of the current row
        var transactionId = $('#transactionId').val();
        if (!transactionId) selectFirstRow();

        // get the current row and use it to find the next row
        var currentRow = $("#transactions [data-id=" + transactionId + "]");
        var nextRow = $(currentRow).next('tr');

        // scroll the next row into focus and click it
        $(nextRow)[0].scrollIntoView();
        nextRow.click();

    }

    function prevRow() {

        // get the transaction id of the current row
        var transactionId = $('#transactionId').val();
        if (!transactionId) selectFirstRow();

        // get the current row and use it to find the next row
        var currentRow = $("#transactions [data-id=" + transactionId + "]");
        var prevRow = $(currentRow).prev('tr');

        // scroll the next row into focus and click it
        $(prevRow)[0].scrollIntoView();
        prevRow.click();

    }

    function saveRow() {

        var budgetLineIds = [];
        var mapData = [];

        // get transaction id, remaining amount, and sign
        var transactionId = $('#transactionId').val();
        var remainingAmount = getNumber($('#remainingAmount').text(), null);
        var sign = Number($('#amount').attr('data-sign'));

        // exit if there is no transaction
        if (!transactionId || transactionId <= 0) {
            alert('No transaction has been selected.');
            return false;
        }

        // warn if the allocations don't balance
        if (remainingAmount === null || remainingAmount !== parseFloat(0)) {
            alert('Transactions must be fully allocated before saving');
            return false;
        }

        // gather mapping data
        var rows = $('.editor-table tbody').find('tr');

        for (var i = 0; i < rows.length; i++) {
            var row = $(rows[i]);
            var select = $(row.find('select'));
            var budgetLineId = parseInt(select.val());
            var input = $(row.find('input'));
            var amount = getNumber(input.val()) * sign;

            if (amount && (!budgetLineId || (budgetLineId <= 0))) {
                alert('Each amount must be associated with a budget line.');
                return false;
            }

            if (!budgetLineId || budgetLineId <= 0 || amount === 0.0) continue;

            if (budgetLineIds.indexOf(budgetLineId) >= 0) {
                alert('A budget line may not have a duplicate allocation.');
                return false;
            }

            budgetLineIds.push(budgetLineId);

            mapData.push({
                MappedTransactionId: 0,
                TransactionId: transactionId,
                BudgetLineId: budgetLineId,
                Amount: amount
            });

        }

        // set main data object
        var data = {
            transactionId: transactionId,
            transactionTypeCode: $('#transactionType option:selected').val(),
            recipient: $('#recipient').val(),
            notes: $('#notes').val(),
            mappedTransactions: mapData
        };

        // save data
        $.ajax({
            url: 'Transactions/UpdateTransaction',
            type: "POST",
            data: JSON.stringify(data),
            contentType: 'application/json; charset=utf-8',
            success: function (data, textStatus, jqXHR) {
                setRowAsMapped(transactionId);
                nextRow();
            },
            error: function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            }
        });

    }

    function seekEnterKey(event) {
        if (event.keyCode !== 13) return;

        // if remaining balance is zero then click save button
        if (getNumber($("#remainingBalance").text()) === 0.0) $("#save").click();
    }

    function selectFirstRow() {
        var row = $("#transactions tbody tr:first-child");

        if (row.length === 0) return;

        $(row)[0].scrollIntoView();
        row.click();
    }

    function selectRow(event) {
        // clear previously selected row
        $(".transaction-row-focus").removeClass("transaction-row-focus");

        // set currently selected row
        var row = $(event.currentTarget);
        row.find("td").addClass("transaction-row-focus");

        var data = {
            transactionId: row.attr("data-id")
        };

        // load the transaction for this row
        $.ajax({
            url: 'Transactions/GetTransaction',
            type: "POST",
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (data, textStatus, jqXHR) {
                fillEditor(data);
                calculateTotals();
            },
            error: function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            }
        });
    }

    function setHeaders() {
        var dataCells = $("#transactions tbody tr:first-child td");
        if (dataCells.length == 0) return;

        var hdrCells = $(".tableheader-div").find("div");

        dataCells.each(function (index, dataCell) {
            var width = $(dataCell).outerWidth();
            $(hdrCells[index]).width(width - 8);
        });
    }

    function setRowAsMapped(transactionId) {
        $("#transactions [data-id=" + transactionId + "]").addClass("transaction-row-mapped");
    }

})();
