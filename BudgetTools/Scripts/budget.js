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