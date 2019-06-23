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
    <link href="~/Content/AppLayout.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="Content/jquery-ui.min.css">
    <link rel="stylesheet" href="Content/master.css">
    <script src="Scripts/jquery-1.10.2.min.js"></script>
    <script src="jquery-ui.min.js"></script>
    <script type="text/javascript" src="Scripts/master.js"></script>
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
            <input type="button" class="nav-button" value="Categories"/>
        </div>
    </div>
    <div class="body-div">
    @@BODY@@
    </div>
    <div class="footer-div">
        <div>Copyright: Gene Graves 2017-2019 </div>
    </div>
  @@TAIL@@
</body>
</html>

