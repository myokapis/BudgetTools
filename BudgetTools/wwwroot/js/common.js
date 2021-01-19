// TODO: roll these methods into master and expose them as public objects on the master object

//function getNumber(value, defaultValue = 0.0) {
//    if (!value) return defaultValue;
//    let sign = value.match(/\(.*\)|-.*/) === null ? 1 : -1;
//    let number = Number(value.toString().replace(/[^.0-9]/g, ""));
//    return Math.abs(number) * sign;
//}

//function getFixedNumeric(value) {
//    let number = isNaN(value) ? getNumber(value, NaN) : value;
//    if (isNaN(number)) return value;
//    return Math.round(number * 100.0) / 100.0;
//}

//function setCurrency(elem, value) {
//    let cleanValue = getFixedNumeric(value === undefined ? $(elem).val() : value);
//    let isNegative = cleanValue < 0.0;
//    let curValue = Number(cleanValue).toLocaleString(undefined, { style: "currency", currency: "USD" });
//    if (isNegative) curValue = "(" + curValue.replace("-", "") + ")";

//    if ($(elem).prop("nodeName") === "INPUT") {
//        $(elem).val(curValue);
//    }
//    else {
//        $(elem).text(curValue);
//    }
//}

