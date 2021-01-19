(function balancesPageScript() {

    $(document).ready(function () {
        disableBankAccount();
        setHeaders();
        bindEvents();
    });

    function bindEvents() {
        $("#periodId").change(changePageScope);
    }

    function changePageScope() {

        master.postForm("LoadPeriodBalances",
            function (data, textStatus, jqXHR) {
                $(".content-section-div").html(data.html);
                disableBankAccount();
                setHeaders();
            },
            function (xhr, textStatus, errorThrown) {
                alert('Error: ' + xhr.statusText);
                alert(errorThrown);
                alert(xhr.responseText);
            }
        );

        //var data = {
        //    periodId: $("#periodId").val()
        //};

        //// save the values
        //$.ajax({
        //    url: 'Balances/ChangePeriod',
        //    type: "POST",
        //    data: JSON.stringify(data),
        //    dataType: 'html',
        //    contentType: 'application/json; charset=utf-8',
        //    success: function (data, textStatus, jqXHR) {
        //        $(".content-section-div").html(data);
        //        disableBankAccount();
        //        setHeaders();
        //    },
        //    error: function (xhr, textStatus, errorThrown) {
        //        alert('Error: ' + xhr.statusText);
        //        alert(errorThrown);
        //        alert(xhr.responseText);
        //    }
        //});
    }

    function disableBankAccount() {
        $("#bankAccountId").prop("disabled", true);
    }

    function setHeaders() {
        let dataCells = $("#balances tbody tr:first-child td");
        if (dataCells.length == 0) return;

        let hdrCells = $(".tableheader-div").find("div");
        let totalCells = $(".totals-div").find("div");

        dataCells.each(function (index, dataCell) {
            let width = $(dataCell).outerWidth();
            $(hdrCells[index]).width(width - 0);
            $(totalCells[index]).width(width - 0);
        });
    }

})();
