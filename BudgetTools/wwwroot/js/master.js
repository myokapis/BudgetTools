(function masterPageScript() {

    $(document).ready(function () {
        bindNavigationEvents();
    });

    master.postForm = function (url, onSuccess, onError, addData) {
        let form = $("#page-scope")[0];
        let formData = new FormData(form);

        if (addData) addData(formData);

        $.ajax({
            url: url,
            type: "POST",
            cache: false,
            data: formData,
            dataType: 'json',
            contentType: false,
            processData: false,
            success: onSuccess,
            error: onError
        });
    }

    function bindNavigationEvents() {
        $(".nav-button").click(navClick);
    }

    function navClick() {
        
        let elem = $(this);
        let form = $("#page-scope");
        $(form).attr("action", elem.val());
        form.submit();
    }

    master.getNumber = function (value, defaultValue = 0.0) {
        if (!value) return defaultValue;
        let sign = value.match(/\(.*\)|-.*/) === null ? 1 : -1;
        let number = Number(value.toString().replace(/[^.0-9]/g, ""));
        return Math.abs(number) * sign;
    }

    master.getFixedNumeric = function (value) {
        let number = isNaN(value) ? master.getNumber(value, NaN) : value;
        if (isNaN(number)) return value;
        return Math.round(number * 100.0) / 100.0;
    }

    master.openErrorDialog = function (options, message) {
        $("#errorDialog #errorMessage").text(message);
        console.debug(options);
        $("#errorDialog").dialog(options).dialog("open");
    }

    master.setCurrency = function (elem, value) {
        let cleanValue = master.getFixedNumeric(value === undefined ? $(elem).val() : value);
        let isNegative = cleanValue < 0.0;
        let curValue = Number(cleanValue).toLocaleString(undefined, { style: "currency", currency: "USD" });
        if (isNegative) curValue = "(" + curValue.replace("-", "") + ")";

        if ($(elem).prop("nodeName") === "INPUT") {
            $(elem).val(curValue);
        }
        else {
            $(elem).text(curValue);
        }
    }

    //function getPageScope() {
    //    return {
    //        BankAccountId: $("#bankAccountId").val(),
    //        CurrentPeriodId: $("#currentPeriodId").val(),
    //        PeriodId: $("#periodId").val()
    //    };
    //}

})(window.master = window.master || {});
