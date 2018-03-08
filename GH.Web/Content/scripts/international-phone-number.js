(function () {
    "use strict";
    angular.module("internationalPhoneNumber", []).constant('ipnConfig', {
        allowExtensions: false,
        autoFormat: false,
        formatOnDisplay: false,
        autoHideDialCode: false,
        autoPlaceholder: false,
        customPlaceholder: null,
        initialCountry: "",
        defaultCountry: "",
        geoIpLookup: function (callback) {
            $.get("//ipinfo.io", function () {
            }, "jsonp").always(function (resp) {
                var countryCode = (resp && resp.country) ? resp.country : "";
                callback(countryCode);
            });
        },
        nationalMode: false,
        numberType: "MOBILE",
        onlyCountries: void 0,
        preferredCountries: ['sg', 'vn', 'us'],
        separateDialCode: true,
        skipUtilScriptDownload: false,
        utilsScript: "/Content/scripts/utils.js"
    }).directive('internationalPhoneNumber', [
        '$timeout', 'ipnConfig', function ($timeout, ipnConfig) {
            return {
                restrict: 'A',
                require: '^ngModel',
                scope: {
                    ngModel: '=',
                    country: '=phoneCode',
                    fullNumber: '=?',
                    inputElement: '='
                },
                link: function (scope, element, attrs, ctrl) {

                    scope.inputElement = element;
                    var handleWhatsSupposedToBeAnArray, options, read, watchOnce;
                    if (ctrl) {
                        if (scope.ngModel && scope.ngModel.length) {
                            element.val(scope.ngModel);
                        }
                        if (element.val() !== '') {
                            $timeout(function () {
                                element.intlTelInput('setNumber', element.val());
                                return ctrl.$setViewValue(element.val());
                            }, 0);
                        }
                    }
                    read = function () {

                        return ctrl.$setViewValue(element.val());
                    };
                    handleWhatsSupposedToBeAnArray = function (value) {
                        if (value instanceof Array) {
                            return value;
                        } else {
                            return value.toString().replace(/[ ]/g, '').split(',');
                        }
                    };
                    options = angular.copy(ipnConfig);
                    angular.forEach(options, function (value, key) {
                        var option;
                        if (!(attrs.hasOwnProperty(key) && angular.isDefined(attrs[key]))) {
                            return;
                        }
                        option = attrs[key];
                        if (key === 'preferredCountries') {
                            return options.preferredCountries = handleWhatsSupposedToBeAnArray(option);
                        } else if (key === 'onlyCountries') {
                            return options.onlyCountries = handleWhatsSupposedToBeAnArray(option);
                        } else if (typeof value === "boolean") {
                            return options[key] = option === "true";
                        } else {
                            return options[key] = option;
                        }
                    });
                    watchOnce = scope.$watch('ngModel', function (newValue) {
                        return scope.$$postDigest(function () {

                            if (newValue !== null && newValue !== void 0 && newValue.length > 0) {

                                if (newValue.charAt(0) !== '+') {
                                    newValue = '+' + newValue;
                                }
                                ctrl.$modelValue = newValue;
                            }
                            element.intlTelInput(options);
                            if (!(options.skipUtilScriptDownload || attrs.skipUtilScriptDownload !== void 0 || options.utilsScript)) {
                                element.intlTelInput('loadUtils', 'utils.js');
                            }

                            return watchOnce();
                        });
                    });
                    if ($.fn.intlTelInput) {
                        scope.countryData = $.fn.intlTelInput.getCountryData();
                    }

                    scope.$watch('country', function (newValue) {

                        if (newValue) {
                            var code = newValue;
                            if (code.charAt(0) === '+') {
                                code = code.substring(1);
                            }
                            var cc = '';
                            angular.forEach(scope.countryData, function (country) {
                                if (code === country.dialCode) {
                                    cc = country.iso2;
                                }
                            });

                            $timeout(function () {
                                element.intlTelInput("setCountry", cc);
                                scope.$parent['form' + attrs.name][attrs.name].$validate();
                            });
                            return newValue;
                        }

                    }, true);

                    element.on("countrychange", function (e, countryData) {
                        if (!countryData) return;
                        if (angular.isDefined(scope.fullNumber)) {
                            scope.fullNumber = element.intlTelInput('getNumber');
                            console.log(scope.fullNumber);
                        }
                        if (scope.isFull) {
                            // scope.ngModel = element.val();
                        }
                        scope.$parent['form' + attrs.name][attrs.name].$validate();
                        $timeout(function () {
                            if (!countryData || !countryData.hasOwnProperty('dialCode')) return;
                            scope.country = countryData.dialCode;
                            if (attrs['addPlus']) {
                                scope.country = '+' + scope.country;
                            }
                            // element.intlTelInput("setCountry", '+' +scope.country);

                            // scope.$parent['form' + attrs.name][attrs.name].$validate();

                        });
                    });

                    ctrl.$formatters.push(function (value) {
                        if (!value) {
                            return value;
                        }

                        element.intlTelInput('setNumber', value);

                        return element.val();
                    });
                    ctrl.$parsers.push(function (value) {
                        if (!value) {
                            return value;
                        }
                        return value;
                        //return value.replace(/[^\d]/g, '');
                    });
                    ctrl.$validators.internationalPhoneNumber = function (value) {
                        if (!value) return true;
                        var selectedCountry = element.intlTelInput('getSelectedCountryData');
                        // if (!value || (selectedCountry && selectedCountry.dialCode === value)) {
                        if (!selectedCountry || !selectedCountry.hasOwnProperty('dialCode')) return false;
                        if (selectedCountry.dialCode === value) {
                            return true;
                        }

                        if (!/^\+*\d+$/.test(value)) {
                            return false;
                        }

                        return element.intlTelInput("isValidNumber");
                    };
                    element.on('blur keyup change', function (event) {
                        return scope.$apply(read);
                    });
                    return element.on('$destroy', function () {
                        element.intlTelInput('destroy');
                        return element.off('blur keyup change');
                    });
                }
            };
        }
    ]);

}).call(this);
