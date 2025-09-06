(function () {
    var FORM_SELECTOR = "form.needs-validation";
    var MODAL_ID = "quoteValidationModal";
    var LIST_ID = "missingFieldsList";

    function showValidationModal() {
        var el = document.getElementById(MODAL_ID);
        if (el && window.bootstrap && bootstrap.Modal) {
            bootstrap.Modal.getOrCreateInstance(el).show();
        }
    }

    function buildMissingList($form) {
        var $list = $("#" + LIST_ID);
        $list.empty();

        // Use jQuery Validate to gather invalid fields
        var validator = $form.data("validator");
        if (!validator) return;

        // Force validation to run
        $form.valid();

        // Collect distinct messages (focus on Required + key failures)
        var messages = [];
        for (var name in validator.invalid) {
            if (!validator.invalid.hasOwnProperty(name)) continue;
            var $input = $form.find("[name='" + name.replace(/([\\[\\]])/g, "\\$1") + "']");
            if (!$input.length) continue;

            // Prefer label text as field name
            var labelText = $form.find("label[for='" + $input.attr("id") + "']").text().trim();
            var msgText = validator.errorMap[name] || "Invalid value";
            messages.push(labelText ? (labelText + " – " + msgText) : msgText);
        }

        if (messages.length === 0) return false;

        messages.forEach(function (m) {
            $("<li/>").text(m).appendTo($list);
        });
        return true;
    }

    $(function () {
        var $form = $(FORM_SELECTOR);
        if (!$form.length) return;

        // On submit, intercept if invalid and show modal
        $form.on("submit", function (e) {
            // ensure validator exists
            if (!$form.data("validator")) {
                $form.removeData("validator");
                $.validator.unobtrusive.parse($form);
            }

            var hasInvalid = !$form.valid();
            if (hasInvalid) {
                hideSpinner();
                e.preventDefault();
                if (buildMissingList($form)) {
                    showValidationModal();
                }
                // focus first invalid control for convenience
                var $first = $form.find(".input-validation-error:first");
                if ($first.length) $first.focus();
            }
        });
    });
})();

(function () {
    var $container = $("#quoteFormContainer");
    var projectId = parseInt($container.data("project-id"), 10) || 0;

    var yy = null, seq = null
    var $qNo = $("#QuoteNumber"); // MVC-generated id for TextBoxFor

    function pad4(n) { return (n + "").padStart(4, "0"); }

    function letterToNum(s) {
        if (!s) return 1;
        s = (s + "").trim().toUpperCase();
        if (/^\d+$/.test(s)) return Math.max(1, parseInt(s, 10));
        if (s.length === 1 && s >= "A" && s <= "Z") return (s.charCodeAt(0) - 64); // A->1
        return 1;
    }

    function bCode(s) { // building type code
        if (!s) return "A";
        return s.trim().toUpperCase().charAt(0);
    }

    function normOpt(s) {
        var n = parseInt((s || "").trim(), 10);
        return isFinite(n) && n > 0 ? n : 1;
    }

    function buildPreview() {
        // If we don't have seq yet, show a soft placeholder
        if (yy == null || seq == null) {
            $qNo.attr("placeholder", "Qyy••••A1-1 (preview) — will finalize on save");
            return;
        }
        var bt = bCode($("#BuildingType").val());
        var opt = normOpt($("#QuotationOption").val());
        var rev = letterToNum($("#Revision").val());
        var num = "Q" + (yy + "").padStart(2, "0") + pad4(seq) + bt + opt + "-" + rev;
        // put preview in the control (still read-only)
        $qNo.val(num);
    }

    var $container = $("#quoteFormContainer");
    var projectId = parseInt($container.data("project-id"), 10) || 0;
    var nextSeqUrl = $container.data("next-seq-url");

    function fetchSeq() {
        $.getJSON(nextSeqUrl, { projectId: projectId })
            .done(function (r) { yy = r.yy; seq = r.seq; buildPreview(); })
            .fail(function () { /* keep placeholder */ });
    }


    $(function () {
        // Hook fields that affect the preview
        $("#BuildingType,#QuotationOption,#Revision").on("input change", buildPreview);

        // Fetch initial year+sequence for preview
        fetchSeq();
    });
})();
