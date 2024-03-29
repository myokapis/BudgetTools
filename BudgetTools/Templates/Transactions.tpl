﻿<!-- @@HEAD@@ -->
<link rel="stylesheet" href="css/Transactions.css" />
<script type="text/javascript" src="js/transactions.js"></script>
<!-- @@HEAD@@ -->

<!-- @@BODY@@ -->
@@SELECTOR@@
<div class="content-section-div">
    <div class="table-section-div">
        <div class="tableheader-div">
            <div>Date</div>
            <div>Check No</div>
            <div style="text-align: right;">Amount</div>
            <div>Description</div>
        </div>
        <div class="table-div">
            <table id="transactions">
                <tbody>
                    <!-- @@ROW@@ -->
                    <tr data-id="@@TransactionId@@" class="@@Class@@">
                        <td>@@TransactionDate@@</td>
                        <td>@@CheckNo@@</td>
                        <td style="text-align: right;">@@Amount@@</td>
                        <td>@@TransactionDesc@@</td>
                    </tr>
                    <!-- @@ROW@@ -->
                </tbody>
            </table>
        </div>
    </div>
    <div class="editor-section-div">
        <!-- @@EDITOR@@ -->
        <input type="hidden" id="transactionId" />
        <div class="label">Transaction Type</div>
        <select id="transactionType">
            <!-- @@TRANSACTION_TYPE@@ --><option value="@@VALUE@@" @@SELECTED@@>@@TEXT@@</option><!-- @@TRANSACTION_TYPE@@ -->
        </select>
        <div class="label">Recipient</div>
        <input type="text" id="recipient" />
        <div class="label">Notes</div>
        <textarea rows="4" cols="30" id="notes"></textarea>
        <table class="editor-table">
            <thead>
                <tr>
                    <th>Transaction Amount</th>
                    <th id="amount"></th>
                </tr>
                <tr>
                    <th>Remaining Amount</th>
                    <th id="remainingAmount"></th>
                </tr>
                <tr>
                    <th>Budget Line</th>
                    <th>Amount</th>
                </tr>
            </thead>
            <tbody>
                <!-- @@EDITOR_ROWS@@ -->
                <tr>
                    <td>
                        <select>
                            <!-- @@BUDGET_LINES@@ --><option value="@@VALUE@@" @@SELECTED@@>@@TEXT@@</option><!-- @@BUDGET_LINES@@ -->
                        </select>
                    </td>
                    <td><input type="text" value = "@@Amount@@"/></td>
                </tr>
                <!-- @@EDITOR_ROWS@@ -->
            </tbody>
        </table>
        <div class="editor-button-container">
            <button type="button" value="Prev" id="prev" />
            <button type="button" value="Next" id="next" />
            <button type="button" value="Save" id="save" />
        </div>
        <!-- @@EDITOR@@ -->
    </div>
</div>
<!-- @@BODY@@ -->
