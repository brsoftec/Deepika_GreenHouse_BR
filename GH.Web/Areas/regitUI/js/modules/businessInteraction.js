﻿angular.module('businessInteraction', ['interaction'])
    .constant('interactionStatusNames', ['Pending', 'Active', 'Inactive', 'Expired'])
    .factory('businessInteractionService', function () {
        return {
        };
    })
    .filter('interactionTypeAlias', function () {
        return function (field) {
            if (!field) return '';
            switch(field.type.toLowerCase()) {
                case 'registration':
                    return 'REG';
                case 'event':
                    return 'EVT';
                case 'handshake':
                    return 'HS';
                case 'broadcast': case 'advertising':
                    return 'BC';
                case 'pushtovault':
                    return 'PUSH';
                default:
                    return field.type.toUpperCase();
            }
        }
    })
    .directive('interactionEditStatic', function (rgu) {
        var cmpId = 0;
        return {
            restrict: 'E',
            scope: {
                type: '@',
                model: '<ngModel',
                options: '<?'
            },
            templateUrl: '/Areas/regitUI/templates/interaction-edit-static.html?v=1',
            link: function (scope, elem, attrs) {
                var model = scope.model;
                if (angular.isUndefined(model) || angular.isString(model) && !model.length) {
                    scope.render="empty";
                    return;
                }
                scope.cmpId = ++cmpId;
                var type = attrs.type;
                scope.render = "raw";
                if (angular.isString(attrs.label) && attrs.label.length) {
                    scope.label = attrs.label;
                }
                if (!type || type==='text') {
                } else if (type === 'title') {
                    scope.render = 'title';
                    scope.title = rgu.toTitleCase(model);
                } else if (type === 'html') {
                    scope.render = 'html';
                } else if (type === 'image') {
                    scope.render = 'image';
                } else if (type === 'url') {
                    scope.render = 'url';
                } else if (type === 'money') {
                    scope.render = 'money';
                    scope.currency = attrs.currency || 'USD';
                } else if (type === 'static') {
                    scope.render = 'text';
                    scope.text = attrs.text || '';
                } else if (type === 'label') {
                    scope.render = 'label';
                    scope.text = scope.options[model];
                }
            }
        };
    })

    .directive('locationCriteria', function ($http) {

        return {
            restrict: 'EA',
            scope: {
                model: '=ngModel'
            },
            templateUrl: '/Areas/regitUI/templates/location-criteria.html?v=1',
            link: function (scope, elem, attrs) {
                scope.model = angular.isObject(scope.model) ? scope.model : {
                    country: '',
                    area: ''
                };
                $http({
                    method: 'POST',
                    url: '/api/LocationService/GetAllCountries'
                }).then(function (response) {
                    scope.countries = response.data.Countries;

                    function findCountryByCode(cc) {
                        if (!cc) return null;
                        var found = null;
                        angular.forEach(scope.countries, function (country) {
                            if (cc === country.Code) {
                                found = country;
                            }
                        });
                        return found;
                    }
                    function findCountryByName(name) {
                        if (!name) return null;
                        var found = null;
                        angular.forEach(scope.countries, function (country) {
                            if (name === country.Name) {
                                found = country;
                            }
                        });
                        return found;
                    }

                    var cc = scope.model.country;
                    scope.selectedCountry = findCountryByCode(cc);
                    scope.onChangeCountry();
                }, function (response) {
                    console.log('Error loading country list', response);
                });
                scope.onChangeCountry = function () {
                    var cc = scope.selectedCountry;
                };
            }
        };
    })

.directive('interactionStub', function () {

        return {
            restrict: 'EA',
            scope: {
                interaction: '<ngModel',
                participants: '@'
            },
            templateUrl: '/Areas/regitUI/templates/interaction-stub.html?v=2',
            link: function (scope, elem, attrs) {
            }
        };
    });

