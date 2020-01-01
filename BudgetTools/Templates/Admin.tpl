<!-- @@HEAD@@ -->
<link rel="stylesheet" href="Content/Admin.css" />
<script type="text/javascript" src="Scripts/App/admin.js"></script>
<!-- @@HEAD@@ -->

<!-- @@BODY@@ -->
@@SELECTOR@@
<div class="content-section-div">
    <div class="menu-section-div">
        <p>Menu</p>
        <ul>
            <li id="closeCurrentPeriod">Close Current Period</li>
            <li id="transferBalance">Transfer Balance</li>
        </ul>
    </div>
    <div class="editor-section-div">
        <!-- @@EDITOR@@ -->
        <!-- @@CLOSE_PERIOD@@ -->
        <div>
            <div>Close Current Period</div>
            <button id="closePeriod">Close</button>
            <table id="messages">
                <thead>
                    <tr>
                        <th>Messages</th>
                    </tr>
                </thead>
                <tbody>
                <!-- @@MESSAGE@@ -->
                <tr>
                    <td>@@TEXT@@</td>
                </tr>
                <!-- @@MESSAGE@@ -->
                </tbody>
            </table>
        </div>
        <!-- @@CLOSE_PERIOD@@ -->
        <!-- @@TRANSFER_BALANCE@@ -->
        <div id="from-to-div">
            <!-- @@GRID_CONTAINER@@ -->
            <div class="grid-container">
                <div class="grid-label">Transfer @@FROM_TO@@:</div>
                <select id="bankAccount@@FROM_TO@@">
                    <!-- @@ACCOUNT@@ --><option value="@@VALUE@@" @@SELECTED@@>@@TEXT@@</option><!-- @@ACCOUNT@@ -->
                </select>
                <table id="budgetLines@@FROM_TO@@" class="budgetLineTable">
                    <tbody>
                        <!-- @@ROW@@ -->
                        <tr data-id="@@BudgetLineId@@">
                            <td>@@BudgetLineName@@</td>
                            <td style="text-align: right">@@Balance@@</td>
                        </tr>
                        <!-- @@ROW@@ -->
                    </tbody>
                </table>
            </div>
            <div class="transfer-spacer"></div>
            <!-- @@GRID_CONTAINER@@ -->
            <div class="amount-container">
                <div>
                    <label>Amount</label>
                    <input id="amount" type="text" />
                </div>
                <div class="notes-container">
                    <label>Notes</label>
                    <textarea id="transferNotes" rows="5" cols="80"></textarea>
                </div>
                <div class="messages">
                    <div id="actionButtons">
                        <button id="transfer">Transfer</button>
                    </div>
                    <table id="transferMessages">
                        <thead>
                            <tr>
                                <th>Messages</th>
                            </tr>
                        </thead>
                        <tbody>
                        <!-- @@TRANSFER_MESSAGE@@ -->
                        <tr>
                            <td>@@TEXT@@</td>
                        </tr>
                        <!-- @@TRANSFER_MESSAGE@@ -->
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
        
        <!-- @@TRANSFER_BALANCE@@ -->
        <!-- @@EDITOR@@ -->
    </div>
</div>
<!-- @@BODY@@ -->
