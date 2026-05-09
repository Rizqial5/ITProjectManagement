// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function initFormValidation(formId) {
        var form = document.getElementById(formId);
    if (!form) return null;

    var rulesObj = { };

    // ambil semua input/textarea/select di dalam form
    var inputs = form.querySelectorAll("input[name], textarea[name], select[name]");
    inputs.forEach(function (input) {
            var field = input.getAttribute("name");
    var rules = { };

    // Required
    if (input.hasAttribute("data-val-required")) {
        rules["required"] = true;
            }

    // Min Length
    if (input.hasAttribute("data-val-length-min")) {
        rules["minLength"] = parseInt(input.getAttribute("data-val-length-min"));
            }

    // Max Length
    if (input.hasAttribute("data-val-length-max")) {
        rules["maxLength"] = parseInt(input.getAttribute("data-val-length-max"));
            }

    // Email
    if (input.hasAttribute("data-val-email")) {
        rules["email"] = true;
            }

    // Range
    if (input.hasAttribute("data-val-range-min") || input.hasAttribute("data-val-range-max")) {
        rules["range"] = [
            parseInt(input.getAttribute("data-val-range-min") || "0"),
            parseInt(input.getAttribute("data-val-range-max") || "999999")
        ];
            }

    // Regex / Pattern
    if (input.hasAttribute("data-val-regex-pattern")) {
        rules["regex"] = new RegExp(input.getAttribute("data-val-regex-pattern"));
            }

            if (Object.keys(rules).length > 0) {
        rulesObj[field] = rules;
            }
        });

    var options = {rules: rulesObj };

    // langsung return instance validator
    return new ej.inputs.FormValidator("#" + formId, options);
}


function showUpdateDateDialog() {
    var dialog = document.getElementById("updateDateDialog").ej2_instances[0];

    dialog.show();
}

function closeDateDialog() {
    var dialog = document.getElementById("updateDateDialog")?.ej2_instances?.[0];
    if (dialog) dialog.hide();
    var container = document.getElementById("updateDialog");
    if (container) container.innerHTML = "";
}

// Handle manual redirects from JSON responses
document.body.addEventListener("htmx:afterRequest", function (evt) {
    try {
        var xhr = evt.detail.xhr;
        // Only parse if it's a JSON response
        if (xhr && xhr.getResponseHeader("Content-Type") && xhr.getResponseHeader("Content-Type").includes("application/json")) {
            var json = JSON.parse(xhr.responseText);
            if (json && json.success === true && json.redirectUrl) {
                window.location.href = json.redirectUrl;
            }
        }
    } catch (err) {
        // Silently fail if not JSON or no redirectUrl
    }
});

function handleUpdateDateResponse(evt) {
    if (evt.detail.target.id !== "updateDateForm") return;

    try {
        var xhr = evt.detail.xhr;
        if (xhr && xhr.status === 200) {
            var response = xhr.responseText;
            var json = JSON.parse(response);

            if (json && json.success === true && json.redirectUrl) {
                closeDateDialog();
                // We use a small delay or skip toast because redirect will happen
                window.location.href = json.redirectUrl;
            } else if (json && json.success === false) {
                var errorEl = document.getElementById("updateDateError");
                if (errorEl) errorEl.innerText = json.errorMessage || "An error occurred.";
            }
        }
    } catch (err) {
        console.error("Error handling update date response:", err);
    }
}

// Global HTMX listeners for Modals and Debugging
document.addEventListener('DOMContentLoaded', function () {
    // Force HTMX to process the main content area to ensure all dynamic elements are registered
    if (typeof htmx !== 'undefined') {
        const mainContent = document.getElementById('main-content') || document.getElementById('app-root');
        if (mainContent) {
            htmx.process(mainContent);
        }
    }
});

document.body.addEventListener('htmx:responseError', function (evt) {
    console.error('[HTMX] Response Error:', evt.detail.xhr.status, evt.detail.xhr.statusText);
});

document.body.addEventListener('htmx:sendError', function (evt) {
    console.error('[HTMX] Send Error: Network issue or request blocked.');
});

document.body.addEventListener('htmx:beforeRequest', function (evt) {
    // Request started
});