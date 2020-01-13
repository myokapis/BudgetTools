<!-- @@HEAD@@ -->
<link rel="stylesheet" href="Content/Budget.css" />
<script type="text/javascript" src="Scripts/App/budget.js"></script>
<!-- @@HEAD@@ -->

<!-- @@BODY@@ -->
@@SELECTOR@@
<div class="content-section-div">
    <div class="tableheader-div">
        <div style="text-align: left;">Budget Line</div>
        <div style="display: none;">Planned</div>
        <div>From Cash</div>
        <div>From Acct</div>
        <div>Spent</div>
        <div>Remaining</div>
        <div>Balance</div>
        <div>Total Cash</div>
        <br style="clear: left;" />
    </div>
    <div class="totals-div">
        <div style="text-align: left;">Totals</div>
        <div style="display: none;">0.00</div>
        <div>0.00</div>
        <div>0.00</div>
        <div>0.00</div>
        <div>0.00</div>
        <div>0.00</div>
        <div>0.00</div>
        <br style="clear: left;" />
    </div>
    <div class="table-div">
        <table id="budgetLines">
            <tbody>
            <!-- @@TBODY@@ -->
            <!-- @@ROWS@@ -->
            <!-- @@ROW_A@@ -->
            <tr class="rowAccrued" data-category="@@BudgetCategoryName@@" data-type="accrued" data-id="@@BudgetLineId@@">
                <td style="text-align: left;">@@BudgetLineName@@</td>
                <td data-type="planned" style="display: none;"><input type="text" value="@@PlannedAmount@@" /></td>
                <td data-type="allocated"><input type="text" value="@@AllocatedAmount@@" /></td>
                <td data-type="accrued"><input type="text" value="@@AccruedAmount@@" /></td>
                <td>@@ActualAmount@@</td>
                <td>@@RemainingAmount@@</td>
                <td>@@AccruedBalance@@</td>
                <td data-type="total-cash">@@TotalCashAmount@@</td>
            </tr>
            <!-- @@ROW_A@@ -->
            <!-- @@ROW_D@@ -->
            <tr class="rowData" data-category="@@BudgetCategoryName@@" data-type="detail" data-id="@@BudgetLineId@@">
                <td style="text-align: left;">@@BudgetLineName@@</td>
                <td data-type="planned" style="display: none;"><input type="text" value="@@PlannedAmount@@" /></td>
                <td data-type="allocated"><input type="text" value="@@AllocatedAmount@@" /></td>
                <td data-type="accrued">@@AccruedAmount@@</td>
                <td>@@ActualAmount@@</td>
                <td>@@RemainingAmount@@</td>
                <td>@@AccruedBalance@@</td>
                <td data-type="total-cash">@@TotalCashAmount@@</td>
            </tr>
            <!-- @@ROW_D@@ -->
            <!-- @@ROW_S@@ -->
            <tr class="rowSummary" data-category="@@BudgetCategoryName@@" data-type="summary" data-id="@@BudgetLineId@@">
                <td style="text-align: left; background: #AA0000;">@@BudgetLineName@@</td>
                <td data-type="planned" style="display: none;">@@PlannedAmount@@</td>
                <td data-type="allocated">@@AllocatedAmount@@</td>
                <td data-type="accrued">@@AccruedAmount@@</td>
                <td>@@ActualAmount@@</td>
                <td>@@RemainingAmount@@</td>
                <td>@@AccruedBalance@@</td>
                <td data-type="total-cash">@@TotalCashAmount@@</td>
            </tr>
            <!-- @@ROW_S@@ -->
            <!-- @@ROWS@@ -->
            <!-- @@TBODY@@ -->
            </tbody>
        </table>
    </div>
</div>
<!-- @@BODY@@ -->

