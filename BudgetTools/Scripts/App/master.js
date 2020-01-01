$(document).ready(function () {
    bindNavigationEvents();
});

function bindNavigationEvents() {
    $(".nav-button").click(navClick);
}

function navClick() {
    var elem = $(this);
    var url = '/' + elem.val();
    window.location.replace(url);
}

function getNumber(value) {
    if (!value) return 0.0;
    var sign = value.match(/\(.*\)|-.*/) === null ? 1 : -1;
    var number = Number(value.toString().replace(/[^.0-9]/g, ""));
    return Math.abs(number) * sign;
}

function getFixedNumeric(value) {
    if (isNaN(value)) return value;
    return Math.round(value * 100.0) / 100.0;
}

function setCurrency(elem, value) {
    var cleanValue = getFixedNumeric(value);
    var isNegative = cleanValue < 0.0;
    var curValue = Number(cleanValue).toLocaleString(undefined, { style: "currency", currency: "USD" });
    if (isNegative) curValue = "(" + curValue.replace("-", "") + ")";

    if (elem.prop("nodeName") === "INPUT") {
        elem.val(curValue);
    }
    else {
        elem.text(curValue);
    }
}
