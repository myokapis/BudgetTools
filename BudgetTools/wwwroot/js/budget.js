(function budgetPageScript() {

    $(document).ready(function () {
        setHeaders();
        updateGrandTotals();
        bindEvents();
    });

    function bindEvents() {
        $("#budgetLines").on("change", "input", updateRow);
        $("#bankAccountId").change(changePageScope);
        $("#periodId").change(changePageScope);
    }

    function changePageScope() {
        master.postForm("Budget/ChangePageScope",
            function (data, textStatus, jqXHR) {
                $("tbody").html(data.html);
                setHeaders();
                updateGrandTotals();
            },
            function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            })
    }

    function updateRow(event) {

        // get changed field and use it to fetch the cells in the current row
        let field = $(this);
        let row = new Row(field, true);

        master.postForm("Budget/SaveBudgetLine",
            function () {
                updateCategoryTotals(field);
                updateGrandTotals();
            },
            function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            },
            function (formData) {
                formData.append("budgetLineId", row.budgetLineId);
                formData.append("plannedAmount", row.cellValues[0]);
                formData.append("allocatedAmount", row.cellValues[1]);
                formData.append("accruedAmount", row.cellValues[2]);
            });
    }

    function setHeaders() {
        let dataCells = $("#budgetLines tbody tr:first-child td");
        if (dataCells.length === 0) return;

        let hdrCells = $(".tableheader-div").find("div");
        let totalCells = $(".totals-div").find("div");

        dataCells.each(function (index, dataCell) {
            let width = $(dataCell).outerWidth();
            $(hdrCells[index]).width(width - 0);
            $(totalCells[index]).width(width - 0);
        });
    }

    function updateCategoryTotals(elem) {

        // get the parent tr tag's budget category
        let category = elem.parent().parent().attr("data-category");

        // find all of the tr for that budget category and break them into summary and detail
        let trs = $("[data-category='" + category + "']");
        let summary = $(trs.filter("[data-type='summary']")[0]);
        let details = trs.filter("[data-type!='summary']");

        // initialize totals array
        let totals = Array(7).fill(0.0); //[0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0];

        // loop through each detail record and update the totals array
        for (let i = 0; i < details.length; i++) {
            let row = new Row($(details[i]));

            for (var j = 0; j < 7; j++) {
                totals[j] = totals[j] + row.cellValues[j];
            }
        }

        // get all of the td tags in the summary tr tag
        let tds = summary.find("td");

        // loop the summary td tags and update their text
        for (let i = 1; i < tds.length; i++) {
            let td = $(tds[i]);
            master.setCurrency(td, totals[i - 1]);
        }
    }

    function updateGrandTotals() {
        let totals = Array(7).fill(0.0); //[0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0];
        let summaryHeaders = $("[data-type='summary']");

        for (let i = 0; i < summaryHeaders.length; i++) {
            let amounts = $(summaryHeaders[i]).find("td");

            for (let j = 0; j < 7; j++) {
                var amount = master.getNumber($(amounts[j + 1]).text());
                totals[j] = totals[j] + amount;
            }
        }

        let totalCells = $(".totals-div").find("div");

        for (let i = 0; i < 7; i++) {
            master.setCurrency($(totalCells[i + 1]), totals[i]);
        }
    }

    function Row(elem, adjustValues) {
        // make constructor scope safe
        if (!(this instanceof Row)) { return new Row(elem, adjustValues); }

        let td, tr;

        // find the row and cell for this element
        if (elem.prop('nodeName') === "TR") {
            tr = elem;
            td = $(elem.find("td")[0]);
        }
        else {
            td = elem.prop('nodeName') === "INPUT" ? elem.parent() : elem;
            tr = td.parent();
        }

        // get the td elements for this row
        let tds = tr.find("td");

        // get the index of the changed element
        let index = td.index();

        this.budgetLineId = tr.attr("data-id");
        this.cells = [];
        this.cellValues = [];

        this.adjustValues = function (index) {

            // recalculate the amounts so that they balance
            if (index === 1) {
                // allocated = planned - accrued;
                this.cellValues[1] = this.cellValues[0] - this.cellValues[2];
            }
            else {
                // planned = allocated + accrued;
                this.cellValues[0] = this.cellValues[1] + this.cellValues[2];
            }

            // recalculate the total cash
            let cashAdj = this.cellValues[2] > 0.0 ? 0.0 : -this.cellValues[2];
            this.cellValues[6] = this.cellValues[1] + cashAdj;

            // recalculate the remaining amount
            let remAdj = this.cellValues[2] <= 0.0 ? 0.0 : this.cellValues[2];
            this.cellValues[4] = this.cellValues[1] - this.cellValues[3] + remAdj;

            // update the cell text
            for (let i = 0; i < 7; i++) {
                if (i === 3 || i === 5) continue;
                master.setCurrency(this.cells[i], this.cellValues[i]);
            }

        };

        // get all of the cells and cell values for this row
        for (let i = 1; i < tds.length; i++) {

            let tdx = $(tds[i]);
            let inputs = tdx.find("input");
            let input = inputs.length > 0 ? $(inputs[0]) : null;

            if (input) {
                this.cells.push(input);
                this.cellValues.push(master.getNumber(input.val()));
            }
            else {
                this.cells.push(tdx);
                this.cellValues.push(master.getNumber(tdx.text()));
            }
        }

        if (adjustValues) this.adjustValues(index);

    }

})();
