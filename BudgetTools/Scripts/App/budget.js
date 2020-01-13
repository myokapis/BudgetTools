$(document).ready(function () {
    setHeaders();
    updateGrandTotals();
    bindEvents();
});

function bindEvents() {
    $("#budgetLines").on("change", "input", updateRow);
    $("#bankAccountId").change(changeBankAccount);
    $("#periodId").change(changePeriod);
}

function changeBankAccount() {
    var data = {
        bankAccountId: $("#bankAccountId").val()
    };

    // save the values
    $.ajax({
        url: 'Budget/ChangeBankAccount',
        type: "POST",
        data: JSON.stringify(data),
        dataType: 'html',
        contentType: 'application/json; charset=utf-8',
        success: function (data, textStatus, jqXHR) {
            $("tbody").html(data);
            setHeaders();
            updateGrandTotals();
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
        url: 'Budget/ChangePeriod',
        type: "POST",
        data: JSON.stringify(data),
        dataType: 'html',
        contentType: 'application/json; charset=utf-8',
        success: function (data, textStatus, jqXHR) {
            $("tbody").html(data);
            setHeaders();
            updateGrandTotals();
        },
        error: function (xhr, textStatus, errorThrown) {
            alert('Error: ' + xhr.statusText);
            alert(errorThrown);
            alert(xhr.responseText);
        }
    });
}

function updateRow(event) {

    // get changed field and use it to fetch the cells in the current row
    var field = $(this);
    var row = new Row(field, true);

    var data = {
        periodId: $("#periodId").val(),
        budgetLineId: row.budgetLineId,
        bankAccountId: $("#bankAccountId").val(),
        plannedAmount: row.cellValues[0],
        allocatedAmount: row.cellValues[1],
        accruedAmount: row.cellValues[2]
    };

    // save the values
    $.ajax({
        url: 'Budget/SaveBudgetLine',
        type: "POST",
        data: JSON.stringify(data),
        dataType: 'html',
        contentType: 'application/json; charset=utf-8',
        success: function() {
            updateCategoryTotals(field);
            updateGrandTotals();
        },
        error: function(xhr, textStatus, errorThrown) {
            alert('Error: ' + xhr.statusText);
            alert(errorThrown);
            alert(xhr.responseText);
        }
    });
}

function setHeaders() {
    var dataCells = $("#budgetLines tbody tr:first-child td");
    if (dataCells.length == 0) return;

    var hdrCells = $(".tableheader-div").find("div");
    var totalCells = $(".totals-div").find("div");

    dataCells.each(function (index, dataCell) {
        var width = $(dataCell).outerWidth();
        $(hdrCells[index]).width(width - 0);
        $(totalCells[index]).width(width - 0);
    });
}

function updateCategoryTotals(elem) {

    // get the parent tr tag's budget category
    var category = elem.parent().parent().attr("data-category");

    // find all of the tr for that budget category and break them into summary and detail
    var trs = $("[data-category='" + category + "']");
    var summary = $(trs.filter("[data-type='summary']")[0]);
    var details = trs.filter("[data-type!='summary']");
    
    // initialize totals array
    var totals = Array(7).fill(0.0); //[0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0];

    // loop through each detail record and update the totals array
    for (var i = 0; i < details.length; i++) {
        var row = new Row($(details[i]));

        for (var j = 0; j < 7; j++)
        {
            totals[j] = totals[j] + row.cellValues[j];
        }
    }
    
    // get all of the td tags in the summary tr tag
    var tds = summary.find("td");

    // loop the summary td tags and update their text
    for (i = 1; i < tds.length; i++) {
        var td = $(tds[i]);
        setCurrency(td, totals[i - 1]);
    }
}

function updateGrandTotals() {
    var totals = Array(7).fill(0.0); //[0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0];
    var summaryHeaders = $("[data-type='summary']");

    for (i = 0; i < summaryHeaders.length; i++) {
        var amounts = $(summaryHeaders[i]).find("td");

        for (j = 0; j < 7; j++) {
            var amount = getNumber($(amounts[j + 1]).text());
            totals[j] = totals[j] + amount;
        }
    }

    var totalCells = $(".totals-div").find("div");

    for (i = 0; i < 7; i++) {
        setCurrency($(totalCells[i + 1]), totals[i]);
    }
}

function Row(elem, adjustValues) {
    // make constructor scope safe
    if (!(this instanceof Row)) { return new Row(elem, adjustValues); }

    var td, tr;

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
    var tds = tr.find("td");

    // get the index of the changed element
    var index = td.index();

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

        // recalculate the remaining amount
        this.cellValues[4] = this.cellValues[0] - this.cellValues[3];

        // recalculate the total cash
        var cashAdj = this.cellValues[2] <= 0.0 ? 0.0 : this.cellValues[2];
        this.cellValues[6] = this.cellValues[1] + cashAdj;

        // update the cell text
        for (var i = 0; i < 7; i++) {
            if (i === 3 || i === 5) continue;
            setCurrency(this.cells[i], this.cellValues[i]);
        }

    };

    // get all of the cells and cell values for this row
    for (var i = 1; i < tds.length; i++) {
        
        var tdx = $(tds[i]);
        var inputs = tdx.find("input");
        var input = inputs.length > 0 ? $(inputs[0]) : null;

        if(input) {
            this.cells.push(input);
            this.cellValues.push(getNumber(input.val()));
        }
        else {
            this.cells.push(tdx);
            this.cellValues.push(getNumber(tdx.text()));
        }
    }
    
    if (adjustValues) this.adjustValues(index);

}
