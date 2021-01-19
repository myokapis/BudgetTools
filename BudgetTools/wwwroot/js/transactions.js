(function transactionsPageScript() {

    $(document).ready(function () {
        setHeaders();
        bindEvents();
        selectFirstRow();
    });

    function bindEvents() {
        $("#bankAccountId").on("change", changePageScope);
        $("#periodId").on("change", changePageScope);
        $("#transactions").on("click", "tbody tr", selectRow);
        $("#prev").on("click", prevRow);
        $("#next").on("click", nextRow);
        $("#save").on("click", saveRow);
        $(".editor-table tbody input").on("change", amountChanged);
        $(".editor-table tbody input").on("keyup", seekEnterKey);
        $(".editor-table select").on("change", lineChanged);
    }

    function amountChanged(event) {
        let field = $(event.currentTarget);
        master.setCurrency(field);
        calculateTotals();
    }

    function calculateTotals() {

        let total = 0.0;

        // get the transaction amount
        let amount = master.getNumber($('#amount').text());

        // get the editor table rows
        let rows = $('.editor-table tbody').find('tr');

        // sum the row amounts
        for (let i = 0; i < rows.length; i++) {
            input = $(rows[i]).find('input');
            value = input.val();
            total += master.getNumber(input.val());
        }

        master.setCurrency("#remainingAmount", amount - total);
    }

    function changePageScope() {
        master.postForm("Transactions/ChangePageScope",
            function (data, textStatus, jqXHR) {
                $("#transactions > tbody").html(data.transactions);
                setHeaders();
                selectFirstRow();
            },
            function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            });
    }

    function fillEditor(data) {

        // fill the descriptive fields
        $('#transactionId').val(data.transactionId);
        master.setCurrency('#amount', Math.abs(data.amount));
        $('#amount').attr('data-sign', data.amount < 0.0 ? -1 : 1);
        $('#transactionType').val(data.transactionType);
        $('#recipient').val(data.recipient);
        $('#notes').val(data.notes);

        // get the editor table rows
        let rows = $('.editor-table tbody').find('tr');

        // get mapped transactions
        let mapped = data.mappedTransactions;
        let xactCount = mapped.length;

        for (let i = 0; i < 5; i++) {

            // get the row and its fields
            let row = $(rows[i]);
            let select = $(row).find('select');
            let input = $(row).find('input');

            // set the field values
            let values = i < xactCount ? mapped[i] : { budgetLineId: 0, amount: 0.0 };
            select.val(values.budgetLineId);
            master.setCurrency(input, Math.abs(values.amount));
        }
    }

    function lineChanged() {
        // check if the transaction is already balanced
        let remainingAmount = master.getNumber($("#remainingAmount").text());
        if (remainingAmount === 0.0) return;

        // get the input field and its value
        let inputField = $(this).parent().next().find("input")[0];
        let lineAmount = master.getNumber($(inputField).val());
        if (lineAmount !== 0.0) return;

        // set the amount for this line to the remaining balance
        master.setCurrency(inputField, remainingAmount);

        // trigger the change event on the input field
        $(inputField).change();
    }

    function nextRow() {

        // get the transaction id of the current row
        let transactionId = $('#transactionId').val();
        if (!transactionId) selectFirstRow();

        // get the current row and use it to find the next row
        let currentRow = $("#transactions [data-id=" + transactionId + "]");
        let nextRow = $(currentRow).next('tr');

        // scroll the next row into focus and click it
        $(nextRow)[0].scrollIntoView();
        nextRow.click();

    }

    function prevRow() {

        // get the transaction id of the current row
        let transactionId = $('#transactionId').val();
        if (!transactionId) selectFirstRow();

        // get the current row and use it to find the next row
        let currentRow = $("#transactions [data-id=" + transactionId + "]");
        let prevRow = $(currentRow).prev('tr');

        // scroll the next row into focus and click it
        $(prevRow)[0].scrollIntoView();
        prevRow.click();

    }

    function saveRow() {

        let budgetLineIds = [];
        let mapData = [];

        // get transaction id, remaining amount, and sign
        let transactionId = $('#transactionId').val();
        let remainingAmount = master.getNumber($('#remainingAmount').text(), null);
        let sign = Number($('#amount').attr('data-sign'));

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
        let rows = $('.editor-table tbody').find('tr');

        for (let i = 0; i < rows.length; i++) {
            let row = $(rows[i]);
            let select = $(row.find('select'));
            let budgetLineId = parseInt(select.val());
            let input = $(row.find('input'));
            let amount = master.getNumber(input.val()) * sign;

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

        master.postForm("Transactions/UpdateTransaction",
            function (data, textStatus, jqXHR) {
                setRowAsMapped(transactionId);
                nextRow();
            },
            function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            },
            function (formData) {
                formData.append("transactionId", transactionId);
                formData.append("transactionTypeCode", $("#transactionType option:selected").val());
                formData.append("recipient", $("#recipient").val());
                formData.append("notes", $("#notes").val());
                //formData.append("mappedTransactions", mapData);
                //formData.append("mappedTransactions.MappedTransactionId", mapData[0].MappedTransactionId);
                formData.append("mappedTransactions[0].TransactionId", mapData[0].TransactionId);
                formData.append("mappedTransactions[0].BudgetLineId", mapData[0].BudgetLineId);
                formData.append("mappedTransactions[0].Amount", mapData[0].Amount);
            });

    }

    function seekEnterKey(event) {
        if (event.keyCode !== 13) return;

        // if remaining balance is zero then click save button
        if (master.getNumber($("#remainingBalance").text()) === 0.0) $("#save").click();
    }

    function selectFirstRow() {
        let row = $("#transactions tbody tr:first-child");

        if (row.length === 0) {
            // clear editor if there is no record to load
            $(".editor-table select").val("");
            $(".editor-table input[type='text']").val("$0.00");
            return;
        };

        $(row)[0].scrollIntoView();
        row.click();
    }

    function selectRow(event) {
        // clear previously selected row
        $(".transaction-row-focus").removeClass("transaction-row-focus");

        // set currently selected row
        let row = $(event.currentTarget);
        row.find("td").addClass("transaction-row-focus");

        // load the transaction for this row
        master.postForm("Transactions/GetTransaction",
            function (data, textStatus, jqXHR) {
                fillEditor(data);
                calculateTotals();
            },
            function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            },
            function(formData){
                formData.append("transactionId", row.attr("data-id"));
            });

    }

    function setHeaders() {
        let dataCells = $("#transactions tbody tr:first-child td");
        if (dataCells.length == 0) return;

        let hdrCells = $(".tableheader-div").find("div");

        dataCells.each(function (index, dataCell) {
            var width = $(dataCell).outerWidth();
            $(hdrCells[index]).width(width - 8);
        });
    }

    function setRowAsMapped(transactionId) {
        $("#transactions [data-id=" + transactionId + "]").addClass("transaction-row-mapped");
    }

})();
