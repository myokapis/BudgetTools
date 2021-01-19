<!-- @@SELECTOR@@ -->
<form id="page-scope" action="" method="post" class="page-scope">
    <div class="selector-div">
        <div>Budget Period</div>
        <select id="periodId" name="pageScope.PeriodId">
            <!-- @@BUDGET_PERIODS@@ --><option value="@@VALUE@@" @@SELECTED@@>@@TEXT@@</option><!-- @@BUDGET_PERIODS@@ -->
        </select>
        <div class="selector-spacer"></div>
        <div>Bank Account</div>
        <select id="bankAccountId" name="pageScope.BankAccountId">
            <!-- @@BANK_ACCOUNTS@@ --><option value="@@VALUE@@" @@SELECTED@@>@@TEXT@@</option><!-- @@BANK_ACCOUNTS@@ -->
        </select>
        <input type="hidden" id="currentPeriodId" name="pageScope.CurrentPeriodId" value="@@CurrentPeriodId@@" />
    </div>
</form>
<!-- @@SELECTOR@@ -->