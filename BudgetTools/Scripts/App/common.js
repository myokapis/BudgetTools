function getNumber(value, defaultValue = 0.0) {
    if (!value) return defaultValue;
    var sign = value.match(/\(.*\)|-.*/) === null ? 1 : -1;
    var number = Number(value.toString().replace(/[^.0-9]/g, ""));
    return Math.abs(number) * sign;
}

function getFixedNumeric(value) {
    var number = isNaN(value) ? getNumber(value, NaN) : value;
    if (isNaN(number)) return value;
    return Math.round(number * 100.0) / 100.0;
}

function setCurrency(elem, value) {
    var cleanValue = getFixedNumeric(value === undefined ? $(elem).val() : value);
    var isNegative = cleanValue < 0.0;
    var curValue = Number(cleanValue).toLocaleString(undefined, { style: "currency", currency: "USD" });
    if (isNegative) curValue = "(" + curValue.replace("-", "") + ")";

    if ($(elem).prop("nodeName") === "INPUT") {
        $(elem).val(curValue);
    }
    else {
        $(elem).text(curValue);
    }
}
