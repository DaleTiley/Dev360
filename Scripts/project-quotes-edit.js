// /Scripts/project-quotes-edit.js
document.addEventListener('DOMContentLoaded', function () {
    // Show spinner on Edit form submit + normalize decimals
    var form = document.querySelector('form[action*="/ProjectQuotes/Edit"]');
    if (form) {
        form.addEventListener('submit', function () {
            // normalize South African commas to dots
            ['RoofArea', 'FloorArea'].forEach(function (name) {
                var el = form.querySelector('[name="' + name + '"]');
                if (el && el.value) el.value = el.value.replace(',', '.');
            });
            if (typeof showSpinner === 'function') showSpinner('Saving quote…');
        });
    }

    // Spinner when opening a quote from lists
    document.addEventListener('click', function (e) {
        if (e.target.closest('.btn-open-quote')) {
            if (typeof showSpinner === 'function') showSpinner('Opening…');
        }
    });
});

// Accept 123, 123.4, 123,45, -123.45
if (window.jQuery && $.validator && $.validator.methods) {
    $.validator.methods.number = function (value, element) {
        if (this.optional(element)) return true;
        return /^-?\d+(?:[.,]\d+)?$/.test(value.trim());
    };
    // If you use Range, make it comma/dot tolerant too:
    var _range = $.validator.methods.range;
    $.validator.methods.range = function (value, element, param) {
        if (value && typeof value === "string") value = value.replace(',', '.');
        return _range.call(this, value, element, param);
    };
}

// Normalize values before submit so the server gets dots
document.addEventListener('submit', function (e) {
    var form = e.target;
    ['RoofArea', 'FloorArea'].forEach(function (name) {
        var el = form.querySelector('[name="' + name + '"]');
        if (el && el.value) el.value = el.value.replace(',', '.');
    });
}, true);
