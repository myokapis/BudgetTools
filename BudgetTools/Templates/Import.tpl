<!-- @@HEAD@@ -->
<link rel="stylesheet" href="Content/Import.css" />
<script type="text/javascript" src="Scripts/App/import.js"></script>
<!-- @@HEAD@@ -->

<!-- @@BODY@@ -->
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
