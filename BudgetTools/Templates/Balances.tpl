<!-- @@HEAD@@ -->
<link rel="stylesheet" href="Content/Balances.css" />
<script type="text/javascript" src="Scripts/App/balances.js"></script>
<!-- @@HEAD@@ -->

<!-- @@BODY@@ -->
@@SELECTOR@@
<div class="content-section-div">
    <!-- @@CONTENT@@ -->
    <div class="tableheader-div">
        <div style="text-align: left;">Bank Account</div>
        <div>Previous</div>
        <div>Current</div>
        <div>Projected</div>
        <br style="clear: left;" />
    </div>
    <!-- @@ALL_ACCOUNTS@@ -->
    <div class="totals-div">
        <div style="text-align: left;">All Accounts</div>
        <div>@@PreviousBalance@@</div>
        <div>@@Balance@@</div>
        <div>@@ProjectedBalance@@</div>
        <br style="clear: left;" />
    </div>
    <!-- @@ALL_ACCOUNTS@@ -->
    <div class="table-div">
        <table id="balances">
            <tbody>
            <!-- @@ROWS@@ -->
            <!-- @@ROW_S@@ -->
            <tr class="rowSummary">
                <td style="text-align: left; background: #AA0000;">@@BankAccountName@@</td>
                <td>@@PreviousBalance@@</td>
                <td>@@Balance@@</td>
                <td>@@ProjectedBalance@@</td>
            </tr>
            <!-- @@ROW_S@@ -->
            <!-- @@ROW_D@@ -->
            <tr class="rowData">
                <td style="text-align: left;">@@BudgetLineName@@</td>
                <td>@@PreviousBalance@@</td>
                <td>@@Balance@@</td>
                <td>@@ProjectedBalance@@</td>
            </tr>
            <!-- @@ROW_D@@ -->
            <!-- @@ROWS@@ -->
            </tbody>
        </table>
    </div>
    <!-- @@CONTENT@@ -->
</div>
<!-- @@BODY@@ -->

