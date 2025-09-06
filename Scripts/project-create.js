/* project-create.js */
(function () {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    function init() {
        function buildAddressString() {
            // Collect the best address we can from the form
            var parts = [];
            function add(id) { var v = (document.getElementById(id)?.value || "").trim(); if (v) parts.push(v); }
            add("SiteName");
            add("StreetAddress");
            add("SiteAddress1");
            add("SiteAddress2");
            add("SiteCity");
            add("SiteProvince");
            add("SitePostcode");
            return parts.join(", ");
        }
        function toGoogleMapsUrl(q) {
            if (!q) return "";
            // Use a search URL; embed will be derived on Details
            return "https://www.google.com/maps?q=" + encodeURIComponent(q);
        }
        // open button href sync
        function syncOpenBtn() {
            var url = (document.getElementById("GoogleMapUrl")?.value || "").trim();
            var a = document.getElementById("gm-open");
            if (a) a.href = url || toGoogleMapsUrl(buildAddressString());
        }

        document.getElementById("gm-generate")?.addEventListener("click", function () {
            var addr = buildAddressString();
            var url = toGoogleMapsUrl(addr);
            var box = document.getElementById("GoogleMapUrl");
            if (box) { box.value = url; box.dispatchEvent(new Event("input")); }
            syncOpenBtn();
        });

        ["SiteName", "StreetAddress", "SiteAddress1", "SiteAddress2", "SiteCity", "SiteProvince", "SitePostcode", "GoogleMapUrl"]
            .forEach(function (id) {
                var el = document.getElementById(id);
                if (el) el.addEventListener("input", syncOpenBtn);
            });

        // initial
        syncOpenBtn();

        // Bind to either form (Create or Edit)
        var form = document.getElementById('projectCreateForm') || document.getElementById('projectEditForm');
        if (!form) return;

        var isEdit = form.id === 'projectEditForm';
        var checkCodeUrl = form.getAttribute('data-check-code-url') || '';

        // Tag MVC requireds
        Array.prototype.forEach.call(form.querySelectorAll('[data-val-required]'), function (el) {
            el.classList.add('js-required');
        });

        function showSpin() { if (typeof showSpinner === 'function') showSpinner(); }
        function hideSpin() { if (typeof hideSpinner === 'function') hideSpinner(); }
        function markInvalid(el, on) { if (el) el.classList.toggle('is-invalid', !!on); }

        function addToModal(messages, titleHtml) {
            var list = document.getElementById('missingFieldsList');
            if (!list) return;
            list.innerHTML = '';
            if (titleHtml) {
                var li = document.createElement('li');
                li.innerHTML = titleHtml;
                list.appendChild(li);
            }
            (messages || []).forEach(function (m) {
                var item = document.createElement('li');
                item.textContent = m;
                list.appendChild(item);
            });
        }
        function showValidationModal() {
            var modalEl = document.getElementById('validationModal');
            if (modalEl && window.bootstrap && bootstrap.Modal) {
                bootstrap.Modal.getOrCreateInstance(modalEl).show();
            }
        }

        function checkProjectCodeUnique(code) {
            // Only check on Create
            if (isEdit || !checkCodeUrl) return Promise.resolve(true);
            return $.get(checkCodeUrl, { code: code, _: Date.now() })
                .then(function (res) { return !!(res && res.ok === true); })
                .catch(function () { return true; });
        }

        // ---------- submit flow ----------
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            Array.prototype.forEach.call(form.querySelectorAll('.is-invalid'), function (el) {
                el.classList.remove('is-invalid');
            });

            var codeEl = document.getElementById('ProjectCode');
            var codeVal = (codeEl ? codeEl.value : '').trim();

            showSpin();

            var duplicateCheck = codeVal ? checkProjectCodeUnique(codeVal) : Promise.resolve(true);

            duplicateCheck.then(function (unique) {
                if (!unique) {
                    hideSpin();
                    markInvalid(codeEl, true);
                    addToModal([], '<strong>Project Code already exists</strong>');
                    showValidationModal();
                    setTimeout(function () { codeEl && codeEl.focus(); }, 150);
                    return;
                }

                var missing = [];
                var issues = [];

                // 1) Skip hidden/readonly/etc in the generic "required" scan
                var requiredSelector = '[data-val-required], .js-required';

                Array.prototype.forEach.call(form.querySelectorAll(requiredSelector), function (el) {
                    var type = (el.getAttribute('type') || '').toLowerCase();

                    // ignore hidden, readonly, disabled, or explicitly skipped fields
                    if (
                        type === 'hidden' ||
                        el.readOnly ||
                        el.disabled ||
                        el.matches('[data-skip-required], [data-skip-required="1"], .js-skip-required')
                    ) return;

                    // ProjectCode is server-generated
                    if (el.id === 'ProjectCode') return;

                    // ContactName is only required when client is NOT Cash Sale
                    if (el.id === 'ContactName') {
                        var clientName = (document.getElementById('ClientName')?.value || '').trim().toUpperCase();
                        // not required if client is Cash Sale OR not selected (server defaults to Cash Sale)
                        if (clientName === '' || clientName === 'CASH SALE') return;
                    }

                    var val = (el.value || '').trim();
                    if (!val) {
                        el.classList.add('is-invalid');
                        var label =
                            (el.id && document.querySelector('label[for="' + el.id + '"]')?.textContent) ||
                            el.getAttribute('data_label') || el.name || el.id || 'Required field';
                        missing.push(label);
                    }
                });


                var emailEl = document.getElementById('ContactEmail');
                var email = emailEl ? (emailEl.value || '').trim() : '';
                if (email && !/^\S+@\S+\.\S+$/.test(email)) {
                    emailEl.classList.add('is-invalid');
                    issues.push('Contact Email looks invalid');
                }

                var phoneEl = document.getElementById('ContactPhone');
                var phone = phoneEl ? (phoneEl.value || '').trim() : '';
                if (phone && !/^[\d\s()+-]{7,}$/.test(phone)) {
                    phoneEl.classList.add('is-invalid');
                    issues.push('Contact Phone looks invalid');
                }

                if (missing.length || issues.length) {
                    hideSpin();
                    var items = [];
                    if (missing.length) items = items.concat(missing);
                    items = items.concat(issues);
                    addToModal(items, '<strong>Required Fields Missing</strong>');
                    showValidationModal();
                    setTimeout(function () {
                        var firstInvalid = form.querySelector('.is-invalid');
                        if (firstInvalid) firstInvalid.focus();
                    }, 150);
                    return;
                }

                hideSpin();
                form.submit();
            }).catch(function () {
                hideSpin();
                addToModal(['Could not verify Project Code uniqueness. Please try again.'],
                    '<strong>Validation problem</strong>');
                showValidationModal();
            });
        });

        // live clear invalid as user types/changes
        form.addEventListener('input', clearInvalid);
        form.addEventListener('change', clearInvalid);
        function clearInvalid(e) {
            var el = e.target;
            if (el.classList.contains('is-invalid') && (el.value || '').trim()) {
                el.classList.remove('is-invalid');
            }
        }

        // ---------------- LOOKUPS (shared) ----------------

        // Openers (used by onclick in the view)
        window.openCustomerSearchModal = function () {
            var el = document.getElementById('customerSearchModal');
            if (!el) return console.warn('customerSearchModal not found');
            bootstrap.Modal.getOrCreateInstance(el).show();
        };
        window.openContactSearchModal = function () {
            var el = document.getElementById('contactSearchModal');
            if (!el) return console.warn('contactSearchModal not found');
            bootstrap.Modal.getOrCreateInstance(el).show();
        };

        // Minimal loaders that hit your existing LookupsController actions
        function loadCustomers(q) {
            q = q || '';
            $.get(window.lookupUrls.customerRows, { q: q, page: 1, pageSize: 25 })
                .done(function (html) { $('#customerRows').html(html); });
        }
        function loadContacts(q) {
            q = q || '';
            var clientId = $('#ClientId').val();
            $.get(window.lookupUrls.contactRows, { clientId: clientId, q: q, page: 1, pageSize: 25 })
                .done(function (html) { $('#contactRows').html(html); });
        }

        // Auto-load when a modal is shown (so it’s never empty)
        document.addEventListener('shown.bs.modal', function (e) {
            if (e.target.id === 'customerSearchModal') {
                $('#customerSearchInput').val('');
                loadCustomers('');
                $('#customerSearchInput').trigger('focus');
            }
            if (e.target.id === 'contactSearchModal') {
                $('#contactSearchInput').val('');
                loadContacts('');
                $('#contactSearchInput').trigger('focus');
            }
        }, false);

        // Debounced search boxes
        function debounce(fn, t) { let h; return function () { clearTimeout(h); h = setTimeout(() => fn.apply(this, arguments), t || 250); }; }
        $(document).on('input', '#customerSearchInput', debounce(function () {
            loadCustomers($(this).val().trim());
        }, 250));
        $(document).on('input', '#contactSearchInput', debounce(function () {
            loadContacts($(this).val().trim());
        }, 250));

        // ------ select/clear (unchanged) ------
        window.selectCustomer = function (id, name) {
            $('#ClientId').val(id);
            $('#ClientName').val(name);
            $('#ContactId, #ContactName, #ContactPhone, #ContactEmail').val('');
            bootstrap.Modal.getInstance(document.getElementById('customerSearchModal'))?.hide();
        };
        window.selectContact = function (id, name, phone, email) {
            $('#ContactId').val(id);
            $('#ContactName').val(name);
            if (phone) $('#ContactPhone').val(phone);
            if (email) $('#ContactEmail').val(email);
            bootstrap.Modal.getInstance(document.getElementById('contactSearchModal'))?.hide();
        };
        window.clearCustomer = function () {
            $('#ClientId').val('');
            $('#ClientName').val('');
            $('#ContactId, #ContactName, #ContactPhone, #ContactEmail').val('');
        };
        window.clearContact = function () {
            $('#ContactId').val('');
            $('#ContactName').val('');
            $('#ContactPhone').val('');
            $('#ContactEmail').val('');
        };
    }
})();
