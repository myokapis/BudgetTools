$(document).ready(function () {
    setHeaders();
    updateGrandTotals();
    bindEvents();
});

function bindEvents() {
    $("#budgetLines input").change(updateRow);
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

function getNumber(value) {
    if (!value) return 0.0;
    return Number(value.toString().replace(/[^-.0-9]/g, ""));
}

function setCurrency (elem, value) {
    var curValue = Number(value).toLocaleString(undefined, { style: "currency", currency: "USD" });

    if (elem.prop("nodeName") == "INPUT") {
        elem.val(curValue);
    }
    else {
        elem.text(curValue);
    }
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
        success: function (data, textStatus, jqXHR) {
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
    var dataCells = $("#budgetLines tbody tr:first-child").find("td");
    var hdrCells = $(".tableheader-div").find("div");
    var totalCells = $(".totals-div").find("div");

    for (i = 0; i < dataCells.length; i++) {
        var width = $(dataCells[i]).outerWidth();
        $(hdrCells[i]).width(width);
        $(totalCells[i]).width(width);
    }
}

function updateCategoryTotals(elem) {

    // get the parent tr tag's budget category
    var category = elem.parent().parent().attr("data-category");

    // find all of the tr for that budget category and break them into summary and detail
    var trs = $("[data-category='" + category + "']");
    var summary = $(trs.filter("[data-type='summary']")[0]);
    var details = trs.filter("[data-type!='summary']");
    
    // initialize totals array
    var totals = [0.0, 0.0, 0.0, 0.0, 0.0, 0.0];

    // loop through each detail record and update the totals array
    for (var i = 0; i < details.length; i++) {
        var row = new Row($(details[i]));

        for (var j = 0; j < 6; j++)
        {
            totals[j] = totals[j] + row.cellValues[j];
        }
    }
    
    // get all of the td tags in the summary tr tag
    var tds = summary.find("td");

    // loop the summary td tags and update their text
    for (var i = 1; i < tds.length; i++) {
        var td = $(tds[i]);
        setCurrency(td, totals[i - 1]);
    }
}

function updateGrandTotals() {
    var totals = [0.0, 0.0, 0.0, 0.0, 0.0, 0.0];
    var summaryHeaders = $("[data-type='summary']");

    for (i = 0; i < summaryHeaders.length; i++) {
        var amounts = $(summaryHeaders[i]).find("td");

        for (j = 0; j < 6; j++) {
            var amount = getNumber($(amounts[j + 1]).text());
            totals[j] = totals[j] + amount;
        }
    }

    var totalCells = $(".totals-div").find("div");

    for (i = 0; i < 6; i++) {
        $(totalCells[i + 1]).text(totals[i].toFixed(2));
    }
}

function Row(elem, adjustValues) {
    // make constructor scope safe
    if (!(this instanceof Row)) { return new Row(elem, adjustValues); }

    var td, tr;

    // find the row and cell for this element
    if (elem.prop('nodeName') == "TR") {
        tr = elem;
        td = $(elem.find("td")[0]);
    }
    else {
        td = (elem.prop('nodeName') == "INPUT") ? elem.parent() : elem;
        tr = td.parent();
    }

    // get the td elements for this row
    var tds = tr.find("td");

    // get the index of the changed element
    var index = td.index();

    this.budgetLineId = tr.attr("data-id");
    this.cells = [];
    this.cellValues = [];

    this.adjustValues = function(index) {

        // recalculate the amounts so that they balance
        if (index == 1) {
            // allocated = planned - accrued;
            this.cellValues[1] = this.cellValues[0] - this.cellValues[2];
        }
        else {
            // planned = allocated + accrued;
            this.cellValues[0] = this.cellValues[1] + this.cellValues[2];
        }

        // update the cell text
        for (var i = 0; i < 3; i++) {
            this.setCurrency(this.cells[i], this.cellValues[i]);
        }

    }

    this.getNumber = function(value) {
        if (!value) return 0.0;
        return Number(value.toString().replace(/[^-.0-9]/g, ""));
    }

    this.setCurrency = function (elem, value) {
        var curValue = Number(value).toLocaleString(undefined, { style: "currency", currency: "USD" });

        if (elem.prop("nodeName") == "INPUT") {
            elem.val(curValue);
        }
        else {
            elem.text(curValue);
        }
    }

    // get all of the cells and cell values for this row
    for (var i = 1; i < tds.length; i++) {
        
        var tdx = $(tds[i]);
        var input = $(tdx.find("input")[0]);

        if(input) {
            this.cells.push(input);
            this.cellValues.push(this.getNumber(input.val()));
        }
        else {
            this.cells.push(tdx);
            this.cellValues.push(this.getNumber(tdx.text()));
        }
    }
    
    if (adjustValues) this.adjustValues(index);

}

/*
function getValue(elemPrefix, budgetLineId) {
  var elemName = '#' + elemPrefix + '_' + budgetLineId.toString();
  var elem = $(elemName);
  var elemVal = (elem.val() || elem.html() || 0.00);
  var sign = (elemVal.toString().substr(0, 1) == '(') ? -1 : 1;
  var exp = new RegExp("[($),]", "gi");
  var val = elemVal.toString().replace(exp, '');
  return parseFloat(val) * sign;
}

function getRowData(elem, typeName) {
  try{
    var cell = elem.parentNode;
    var row = cell.parentNode;
    var body = row.parentNode;
    var budgetLineId = row.id.toString().replace('tr_', '');
    var budgetLineString = budgetLineId.toString();
    var accruedAmount = getValue('accrued', budgetLineId);
    var allocatedAmount = getValue('allocated', budgetLineId);
    var plannedAmount = getValue('planned', budgetLineId);
    var accruedBalance = getValue('td_balance', budgetLineId);

    if (typeName == 'planned'){
      allocatedAmount = plannedAmount - accruedAmount;
    }
    else if (typeName == 'allocated'){
      plannedAmount = allocatedAmount + accruedAmount
    }
    else{
        plannedAmount = allocatedAmount + accruedAmount;
    }

    var selectedValue = $('#BankAccountId :selected').val();

    var data = {
      BudgetLineId: budgetLineId,
      BankAccountId: selectedValue,
      AccruedAmount: accruedAmount,
      AllocatedAmount: allocatedAmount,
      PlannedAmount: plannedAmount
    }

    $.ajax({
      url: buildUrl('Budget/SaveBudgetLine'),
      type: "POST",
      data: JSON.stringify(data),
      dataType: 'html',
      contentType: 'application/json; charset=utf-8',
      success: function (data2) {
        body.innerHTML = data2;
      },
      error: function (xhr, textStatus, errorThrown) {
        alert('Error: ' + xhr.statusText);
        alert(errorThrown);
        alert(xhr.responseText);
      },
      async: false
    });
  }
  catch (e) {
    alert(e.message);
  }
}

function handleAccruedClick(elem) {
  getRowData(elem, 'accrued');
}

function handleAllocatedClick(elem) {
  getRowData(elem, 'allocated');
}

function handlePlannedClick(elem) {
  getRowData(elem, 'planned');
}
*/