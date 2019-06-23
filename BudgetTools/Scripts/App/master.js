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