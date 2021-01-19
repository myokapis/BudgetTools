<!-- @@HEAD@@ -->
<link rel="stylesheet" href="css/Import.css" />
<script type="text/javascript" src="js/import.js"></script>
<!-- @@HEAD@@ -->

<!-- @@BODY@@ -->
<form id="page-scope" action="" method="post" class="page-scope">
    <div id="selector-div">
        <input type="hidden" id="periodId" name="pageScope.PeriodId" value="@@PeriodId@@" />
        <input type="hidden" id="bankAccountId" name="pageScope.BankAccountId" value="@@BankAccountId@@" />
        <input type="hidden" id="currentPeriodId" name="pageScope.CurrentPeriodId" value="@@CurrentPeriodId@@" />
    </div>
</form>
<div class="table-div">
    <table>
        <thead>
            <tr>
                <th>Bank Account</th>
                <th>Upload</th>
            </tr>
        </thead>
        <tbody>
            <!-- @@ROW@@ -->
            <tr data-id="@@BankAccountId@@">
                <td>@@BankAccountName@@</td>
                <td class="file-display">
                    <button type="button" class="file-button" >...</button>
                    <input type="file" />
                    <div>Select a File</div>
                </td>
            </tr>
            <!-- @@ROW@@ -->
        </tbody>
    </table>
</div>
<!-- @@BODY@@ -->
