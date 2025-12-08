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


