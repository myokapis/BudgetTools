<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Budget Tools</title>
    <!--
    @Styles.Render("~/Content/css")
    @Scripts.Render("~/bundles/modernizr")
    -->
    <link rel="stylesheet" href="Content/AppLayout.css" />
    <link rel="stylesheet" href="Content/jquery-ui.min.css" />
    <link rel="stylesheet" href="Content/master.css" />
    <script src="https://code.jquery.com/jquery-1.12.4.min.js" type="text/javascript"></script>
    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.min.js" type="text/javascript"></script>
    <script src="Scripts/App/master.js" type="text/javascript"></script>
    <!-- TODO: add js reference for the button actions and common navigation -->
    @@HEAD@@
</head>
<body>
    <div class="header-div">
        <div class="nav-button-container">
            <input type="button" class="nav-button" value="Import"/>
            <input type="button" class="nav-button" value="Transactions"/>
            <input type="button" class="nav-button" value="Budget"/>
            <input type="button" class="nav-button" value="Balances"/>
            <input type="button" class="nav-button" value="Admin"/>
        </div>
    </div>
    <div class="body-div">
    @@BODY@@
    </div>
    <div class="footer-div">
        <div>Copyright: Gene Graves 2017-2020 </div>
    </div>
  @@TAIL@@
</body>
</html>

