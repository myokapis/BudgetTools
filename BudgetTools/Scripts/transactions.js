// extensions
if (!String.prototype.trim) {
  String.prototype.trim = function () {
    return this.replace(/^\s+|\s+$/g, '');
  }
}

// page load code
$(document).ready(function () {
  currentSelectionSet(0);
  moveSelection(0);
});

function cleanCurrency(amount) {
  var parsedAmount = amount.toString().replace('$', '');
  return (isNaN(parsedAmount)) ? parseFloat(0.0) : parseFloat(parsedAmount);
}

// read the stored value of the current selection
// the value is the row index of the selected grid row
function currentSelectionGet() {
  try {
    return document.getElementById('current_record').value;
  }
  catch (e) {
  }
}

// saves a row index value to a hidden field
function currentSelectionSet(rowIndex) {
  try {
    document.getElementById('current_record').value = rowIndex;
  }
  catch (e) {
  }
}

// deselects a grid row by removing the background color class
function deselectGridRow(row) {
  try {
    row.classList.remove('transaction-row-selected');
  }
  catch (e) {

  }
}

// formats amounts as two decimal place numeric values
function formatAmount(value) {
  var numValue = (isNaN(value) | value === '' | value === null) ? 0.00 : value;
  return parseFloat(numValue).toFixed(2).toString();
}

// returns the grid rows as an array
function gridRowsGet() {
  try {
    var table = document.getElementById('transaction_table_inner');
    var body = table.getElementsByTagName('tbody')[0];
    return body.getElementsByTagName('tr');
  }
  catch (e) {
    alert(e.message);
  }
}

// event handler for when a grid row is clicked
// deselects the current row and selects the clicked row
// saves the rowIndex of the clicked row
function handleRowClick(row) {
  //try {
  // move selection to current row
  moveSelection(row.rowIndex);
  //}
  //catch (e) {
  //  alert('error');
  //}
}

// handles next button click
// loads the next record in the grid to the editor
function handleNextClick() {
  // get the row index of the next row in the grid and select it
  var row_index = parseInt(currentSelectionGet()) + 1;
  moveSelection(row_index);
}

// handles previous button click
// loads the previous record in the grid to the editor
function handlePrevClick() {
  // get the previous record in the grid and select it
  var row_index = parseInt(currentSelectionGet()) - 1;
  moveSelection(row_index);
}

// handles the save button click
// saves the currently selected transaction
function handleSaveClick() {
  if (validateMapping()) {
    alert('Invalid mapping. Transaction cannot be saved.');
    return false;
  }

  var row_index = currentSelectionGet();
  var row = gridRowsGet()[row_index];
  var transaction =
  {
    TransactionId: transactionIdGet(row),
    TransactionTypeCode: $("#TransactionType option:selected").val(),
    Recipient: $("#Recipient").val(),
    Notes: $("#Notes").val(),
    BudgetLine1Id: $("#BudgetLine1 option:selected").val(),
    Amount1: $("#Amount1").val(),
    BudgetLine2Id: $("#BudgetLine2 option:selected").val(),
    Amount2: $("#Amount2").val(),
    BudgetLine3Id: $("#BudgetLine3 option:selected").val(),
    Amount3: $("#Amount3").val()
  };
  
  $.ajax({
    url: buildUrl('/Transaction/SaveTransaction'),
    type: "POST",
    data: JSON.stringify(transaction),
    dataType: 'json',
    contentType: 'application/json; charset=utf-8',
    success: function (data2) {
      setMappedFeatures(row);

      if ($('#transaction_next').is(':disabled') == false) handleNextClick();
    },
    error: function (xhr, textStatus, errorThrown) {
      alert('Error: ' + xhr.statusText);
    },
    async: false
  });
}

// manages moving up and down the grid
function moveSelection(row_index) {
  //try {
  // get the current row index and the grid rows then save the current selection
  var current_row_index = currentSelectionGet();
  var grid_rows = gridRowsGet();
  currentSelectionSet(row_index);

  // enable/disable the prev and next buttons
  setButtonProperties(row_index, grid_rows.length);

  // bail out if the grid is empty
  if (grid_rows.length == 0) return;

  // deselect the current row
  deselectGridRow(grid_rows[current_row_index]);

  // select the current row
  var row = grid_rows[row_index];
  selectGridRow(row);

  // make a JSON call to load editor
  var jqxhr = $.getJSON(buildUrl("Transaction/LoadEditor"), { TransactionId: transactionIdGet(row) })
    .done(function (json) {
      $("#TransactionType").val(json['TransactionTypeCode']);
      $("#Recipient").val(json['Recipient']);
      $("#Notes").val(json['Notes']);
      $("#BudgetLine1").val(json['BudgetLine1Id']);
      $("#Amount1").val(formatAmount(json['Amount1']));
      $("#BudgetLine2").val(json['BudgetLine2Id']);
      $("#Amount2").val(formatAmount(json['Amount2']));
      $("#BudgetLine3").val(json['BudgetLine3Id']);
      $("#Amount3").val(formatAmount(json['Amount3']));
      $("#TotalAmount").val(json['TotalAmount']);
    })
    .fail(function (jqxhr, textStatus, error) {
      alert('grid load error');
      alert(error);
    });

  //}
  //catch (e) {
  //  alert('error');
  //}
}

// selects a grid row by adding a class to change the background color
function selectGridRow(row) {
  try {
    row.classList.add('transaction-row-selected');
  }
  catch (e) {

  }
}

// enables or disables the prev and next buttons based on the currently
// selected row and the number of grid records
function setButtonProperties(row_index, row_count) {
  // determine if prev and next buttons should be disabled
  var is_prev_disabled = (row_index == 0 || row_count == 0);
  var is_next_disabled = (row_index == row_count - 1 || row_count == 0);
  var is_save_disabled = (row_count == 0);

  // set the button disabled property
  document.getElementById('transaction_prev').disabled = is_prev_disabled;
  document.getElementById('transaction_next').disabled = is_next_disabled;
  document.getElementById('transaction_save').disabled = is_save_disabled;
}

// sets the display properties for a mapped row
// if no row is provided, the current row is used
function setMappedFeatures(row) {
  if (row == null) {
    row_index = currentSelectionGet();
    row = gridRowsGet()[row_index];
  }
  row.classList.add('transaction-row-mapped');
}

// gets the transaction id that is stored as part of the row element id
function transactionIdGet(row) {
  try {
    var id = row.id;
    return id.replace('tr_', '');
  }
  catch (e) {

  }
}

function validateMapping() {
  // get the amounts and budget lines
  var amounts = [cleanCurrency($("#Amount1").val()), cleanCurrency($("#Amount2").val()), cleanCurrency($("#Amount3").val())];
  var budgetLines = [parseInt($("#BudgetLine1 option:selected").val()), parseInt($("#BudgetLine2 option:selected").val()), parseInt($("#BudgetLine3 option:selected").val())];
  var totalAmount = parseFloat($("#TotalAmount").val());
  var sumAmounts = parseFloat(0.0);
  var hasError = false;

  for (var i = 0; i < 3; i++) {
    amount = parseFloat(amounts[i]);
    sumAmounts = sumAmounts + parseFloat(amount);
    if ((budgetLines[i] > 0) && (amount == 0.0)) hasError = true;
  }

  hasError = hasError | (totalAmount != sumAmounts);
  return hasError;
}