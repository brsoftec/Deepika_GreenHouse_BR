angular.module('regit.ui', ['mgcrea.ngStrap', 'NgSwitchery','defs'])
    .factory('rgu', function ($moment) {
        return {
            parseDate: function (value) {
                var type, date;

                if (angular.isDate(value)) {
                    type = 'date';
                    date = value;

                } else if (value === 'today') {
                    type = 'date';
                    date = new Date();
                } else if (!angular.isString(value)) {
                    return false;
                }
                else if (/^\d{1,2}-\d{1,2}-\d\d\d\d$/.test(value)) {
                    type = 'dmy';
       
                    date = $moment(value, 'D-M-YYYY').toDate();
                } else if (/^\d{1,2}-\d{1,2}-\d\d$/.test(value)) {
                    type = 'dmys';
                    date = $moment(value, 'D-M-YY').toDate();
                } else if (/^\d\d\d\d-\d{1,2}-\d{1,2}$/.test(value)) {
                    type = 'ymd';
                    date = $moment(value, 'YYYY-M-D').toDate();
                } else {
                    return false;
                }
                return {
                    type: type,
                    date: date,
                    dmy: {
                        day: date.getDate(),
                        month: date.getMonth(),
                        year: date.getFullYear()
                    }
                };
            }
        };
    })
    .factory('rguModal', function ($modal) {
        var modals = {
            'msg.addPeopleToConversation': {templateFile: 'msg-conversation-add-people'},
            'msg.manageConversation': {templateFile: 'msg-conversation-manage'},
            'vault.doc.editor': {templateFile: 'vault-doc-editor'}
        };

        return {
            openModal: function (name, scope, parms) {
                if (!(name in modals)) return;
                if (angular.isDefined(parms)) {
                    scope.modalParms = parms;
                }
                var templateUrl = RegitTemplatePath + '/' + modals[name].templateFile + '.modal.html';
                var modal = $modal({
                    scope: scope,
                    templateUrl: templateUrl,
                    placement: 'center',
                    show: false
                });
                modal.$promise.then(modal.show);
            }
        };
    })

    .directive('h1', function () {
        function link(scope, element, attrs) {
            if (element.hasClass('rule')) return;
            element.addClass('rule').wrapInner('<span/>');
        }

        return {
            restrict: 'E',
            link: link
        };
    })
    .directive('rguOptionGroup', function () {
        function link(scope, element, attrs) {
            element.addClass('btn-group');
        }

        return {
            restrict: 'EAC',
            link: link,
            scope: {
                values: '<',
                model: '=ngModel'
            },
            template: '<label ng-repeat="value in values" class="btn btn-default btn-option" uib-btn-radio="value"'
            + ' ng-model="model" ng-class="{active: model===value}" ng-click="select($event,value)">{{value}}</label>',
            controller: function ($scope, $element) {
                $scope.select = function (e, value) {
                    $element.find('label').removeClass('active');
                    $scope.model = value;
                    angular.element(e.target).addClass('active');
                }
            }
        };
    })

    .directive('rguCheckbox', function () {
        function link(scope, element, attrs) {
        }

        return {
            restrict: 'AEC',
            scope: {
                'label': '@',
                'model': '=ngModel',
                'onChange': '&'
            },
            link: link,
            template: '<input type="checkbox" class="input" ng-model="model" ng-change="onChange()">'
            + '<label>{{label}}</label>'
        };
    })

    .directive('rguSelect', function () {
        function link(scope, element, attrs) {
            var container = angular.element('<div class="select" />');
            if (attrs.length) {
                container.attr(attrs);
            }
            if (element.hasClass('short')) {
                container.addClass('short');
                element.removeClass('short');
            }
            if (element.hasClass('inline')) {
                container.addClass('inline');
                element.removeClass('inline');
            }
            element.wrap(container);
            /*            if (attrs.className) {
             container += ' ' + attrs.className + '"';
             }
             if (attrs.style) {
             container += ' style="' + attrs.style + '"';
             }
             container += ' />';
             element.wrap(container);
             element.replaceWith(container);*/
        }

        return {
            restrict: 'AC',
            link: link
            // replace: true,
            // transclude:true,
            //  template: '<div class="select" ng-translude></div>'
        };
    })

    .directive('rguSteps', function () {
        function link(scope, element, attrs) {
            element.addClass('steps');
        }

        return {
            restrict: 'EAC',
            link: link,
            template: '<ul>'
            + '<li ng-repeat="s in steps" ng-class="{active: s==step, last: $last, passed: s<step}">{{s}}</li>'
            + '</ul>'
        };
    })

    .directive('rguInputGroup', function () {
        function link(scope, element, attrs) {

            var measureUnits = {
                length: ['cm', 'm', 'ft', 'in'],
                weight: ['kg', 'lbs'],
                currency: ['USD', 'SGD'],
                shoesize: ['US', 'Euro', 'UK']
            };
            if (!scope.type) {
                scope.type = 'number';
            }
            if (!scope.measure) {
                scope.measure = 'length';
            }
            scope.units = measureUnits[scope.measure];
            if (!scope.unitModel) {
                scope.unitModel = scope.units[0]
            }
            scope.onInputChange = function () {
                //scope.$parent.$eval(scope.onchange);
                console.log('a');
                //scope.$parent.onChange();
            }
        }

        return {
            restrict: 'EAC',
            link: link,
            scope: {
                name: '@',
                type: '@',
                width: '@',
                measure: '@',
                model: '=ngModel',
                unitModel: '=unitModel',
                onChange: '&',
                required: '@?',
                min: '@?'
            },
            template: '<input type="{{type}}" name="{{name}}Input" ng-model="model" ng-change="$parent.onInputChange()" ' +
            'ng-required="required" min="{{min}}" rules="number"/>'
            + '<div rgu-picker class="picker" items="units" ng-model="unitModel"/>'

        };
    })
    .directive('rguRadioGroup', function ($timeout) {
        function link(scope, element, attrs) {
            if (attrs.type === 'yesno') {
                scope.values = ['Yes', 'No'];
            }
            scope.valueOf = function (value) {
                if (attrs.type === 'yesno') return value === 'Yes';
                if (attrs.type === 'boolean') return (value === scope.values[1]);
                return value;
            };
            scope.isChecked = function (value) {
                if (attrs.type === 'yesno') return value === 'Yes';
                return value;
            };
            scope.onChange = function () {
                if (!attrs.onChange) return;
                $timeout(function () {
                    scope.$parent.$parent.$eval(attrs.onChange)
                });
            }
        }

        return {
            restrict: 'EAC',
            link: link,
            scope: {
                name: '@',
                type: '@?',
                values: '<?',
                model: '=ngModel'
            },
            template: '<div class="radiogroup">'
            + '<div class="radio" ng-repeat="value in values">'
            + '<input type="radio" name="{{name}}" ng-value="valueOf(value)" id="{{name}}-{{value}}" ng-model="$parent.model"'
            + ' ng-checked="valueOf(value)===model" ng-change="onChange()">'
            + '<label for="{{name}}-{{value}}">{{value}}</label>'
            + '</div></div>'

        };
    })
    .directive('rguSwitch', function () {
        return {
            restrict: 'AEC',
            scope: {
                name: '@',
                model: '=ngModel'
            },
            template: '<span class="rgu-switch-label rgu-switch-false">NO</span>'
            + '<input type="checkbox" class="js-switch" ng-model="model" ui-switch="{color: \'#00adef\', secondaryColor: \'#ccc\'}" />'
            + '<span class="rgu-switch-label rgu-switch-true">YES</span>'

        }
    })
    .directive('rguRangeInput', function () {
        return {
            restrict: 'AE',
            scope: {
                name: '@',
                type: '@',
                model: '=ngModel',
                unitModel: '=unitModel'
            },
            templateUrl: RegitTemplatePath + '/range-input.html',

            link: function (scope, elem, attrs) {

                scope.addRange = function () {
                    var start = null;
                    if (scope.model.length > 0) {
                        start = scope.model[scope.model.length - 1][1] + 1;
                    }
                    scope.model.push([start, null]);
                };
                scope.removeRange = function (index) {
                    scope.model.splice(index, 1);
                };
                scope.model = scope.model || [];
                if (!scope.model.length) {
                    scope.addRange();
                }

            }
        }
    })
    .directive('rguAnswerInput', function () {
        return {
            restrict: 'AE',
            scope: {
                name: '@',
                type: '@',
                model: '=ngModel'
            },
            templateUrl: RegitTemplatePath + '/answer-input.html',

            link: function (scope, elem, attrs) {

                scope.addChoice = function (choice) {
                    scope.model.push({value: ''});
                };
                scope.removeChoice = function (index) {
                    scope.model.splice(index, 1);
                };
                scope.model = scope.model || [];
                if (!scope.model.length) {
                    scope.addChoice();
                }

            }
        }
    })
    .directive('rguPicker', function () {
        function link(scope, element, attrs) {
            element.addClass('picker');
        }

        return {
            restrict: 'EAC',
            link: link,
            scope: {
                items: '<',
                filter: '@?',
                required: '@?',
                placeholder: '@?',
                model: '=ngModel'
            },
            template: '<select ng-init="filterExp = filter ? \'| \' + filter : \'\';" ng-options="item as (item{{filterExp}}) for item in items" ng-model="model">'
            + '<option ng-if="placeholder" value="" class="select-heading" ng-bind="placeholder"></option>'
            + '</select>'
        };
    })

    .filter('monthName', function ($moment) {
        return function (month) {
            return $moment().month(month).format('MMMM');
        };
    })

    .directive('rguDateInput', function ($timeout, $compile, $moment, rgu) {

        return {
            restrict: 'EAC',
            scope: {
                name: '@',
                type: '@',
                required: '@?',
                model: '=ngModel',
                before: '<',
                after: '<'
            },
            templateUrl: RegitTemplatePath + '/date-input.html',
            link: function (scope, element, attrs) {
                if (attrs.before === 'today') {
                    date = new Date();
                    scope.beforeDate = rgu.parseDate(date);
                    if (attrs.type === 'combo') {
                        updateDmy();
                    } else {
                        scope.beforeMdy = $moment(date).format('MM-DD-YYYY');
                    }
                } else {
                    scope.$parent.$watch(attrs.before, function (date, ov) {
                        scope.beforeDate = date ? rgu.parseDate(date) : undefined;
                        if (attrs.type === 'combo') {
                            updateDmy();
                            validate(scope.rdate);
                        } else {
                            if (angular.isDate(date)) {
                                scope.beforeMdy = $moment(date).format('MM-DD-YYYY');
                                validate(scope.rdate);
                            }
                        }
                    });
                }
                if (attrs.after === 'today') {
                    date = new Date();
                    scope.afterDate = rgu.parseDate(date);

                    if (attrs.type === 'combo') {
                        updateDmy();
                    } else {
                        scope.afterMdy = $moment(date).format('MM-DD-YYYY');
                    }
                } else {
                    scope.$parent.$watch(attrs.after, function (date, ov) {

                        scope.afterDate = date ? rgu.parseDate(date) : undefined;

                        if (attrs.type === 'combo') {
                            updateDmy();
                            validate(scope.rdate);
                        } else {
                            if (angular.isDate(date)) {
                                scope.afterMdy = $moment(date).format('MM-DD-YYYY');
                                validate();
                            }
                        }
                    });
                }

                if (angular.isDefined(scope.model) && angular.isDate(scope.model)) {
                    scope.rdate = {
                        day: scope.model.getDate(),
                        month: scope.model.getMonth(),
                        year: scope.model.getFullYear()
                    };
                } else {
                    scope.rdate = {
                        day: null,
                        month: null,
                        year: null
                    };
                }
                scope.isDirty = false;

                scope.error = {
                    status: false,
                    msg: ''
                };
                // getDate() getMonth() getFullYear()
                function validate(rdate) {
                    var type = attrs.type;
                    var combo = type === 'combo';
                    if (combo && !rdate) return;
                    var day, month, year;
                    if (combo) {
                        day = rdate.day;
                        month = rdate.month;
                        year = rdate.year;
                    } else {
                        if (angular.isDate(scope.model)) {
                            day = scope.model.getDate();
                            month = scope.model.getMonth();
                            year = scope.model.getFullYear();
                        }
                    }
                    var moment = combo ? $moment({
                        y: year,
                        M: month,
                        d: day
                    }) : $moment(scope.model);
                    scope.error.status = false;
                    if (combo && type === scope.isDirty && (day === null || month === null || year === null)) {
                        scope.error.status = true;
                        scope.error.msg = 'Please enter day, month and year';
                    }
                    else if (!moment.isValid()) {
                        scope.error.status = true;
                        scope.error.msg = 'Please enter a valid date';
                    } else if (scope.beforeDate && moment.isAfter(scope.beforeDate.date, 'day')) {
                        scope.error.status = true;
                        scope.error.msg = 'Please enter a date not after ' + $moment(beforeDate.date).format('D MMM YYYY');
                    } else if (scope.afterDate && moment.isBefore(scope.afterDate.date, 'day')) {
                        scope.error.status = true;
                        scope.error.msg = 'Please enter a date not before ' + $moment(scope.afterDate.date).format('D MMM YYYY');
                    } else {
                        if (!angular.isDefined(scope.model) || !angular.isDate(scope.model)) {
                            scope.model = new Date();
                        }
                        scope.model.setDate(day);
                        scope.model.setMonth(month);
                        scope.model.setFullYear(year);
                    }
                }

                function updateDmy() {

                    scope.years = [];
                    var minYear = 1960, maxYear = 2040;
                    if (scope.afterDate) {
                        minYear = scope.afterDate.dmy.year;
                    }
                    if (scope.beforeDate) {
                        maxYear = scope.beforeDate.dmy.year;
                    }
                    for (var y = minYear; y <= maxYear; y++) {
                        scope.years.push(y);
                    }
                    scope.months = [];
                    for (var m = 0; m < 12; m++) {
                        scope.months.push(m);
                    }
                    scope.days = [];
                    for (var d = 1; d <= 31; d++) {
                        scope.days.push(d);
                    }

                }

                if (attrs.type === 'combo') {
                    scope.$watchCollection('rdate', function (rdate, oldValue) {
                        if (rdate === oldValue) return;
                        scope.isDirty = true;
                        validate(rdate);
                    });
                }

            }
        };
    })
    .directive('rguDateDmy', function ($parse, $moment) {
        return {
            restrict: 'EAC',
            scope: {
                name: '@',
                required: '@?',
                model: '=ngModel'
            },
            templateUrl: RegitTemplatePath + '/date-dmy.html',
            link: function link(scope, element, attrs) {
                console.log(element);
                function isValidDate(year, month, day) {
                    return $moment({
                        y: year,
                        M: month,
                        d: day
                    }).isValid();
                }

                function parseDmy(dmy) {
                    if (!angular.isString(dmy)) return {
                        day: undefined,
                        month: undefined,
                        year: undefined
                    };
                    var parts = dmy.split('-');
        
                    if (parts.length != 3) return {
                        day: undefined,
                        month: undefined,
                        year: undefined
                    };
                    var day = parts[0];
                    if (!day || !isFinite(day) || day == 0) {
                        day = undefined;
                    } else {
                        day = parseInt(day);
                    }
                    var month = parts[1];
                    if (!month || !isFinite(month) || month == 0) {
                        month = undefined;
                    } else {
                        month = parseInt(month) - 1;
                    }
                    var year = parts[2];
                    if (!year || !isFinite(year) || year == 0) {
                        year = undefined;
                    } else {
                        year = parseInt(year);
                    }
                    return {
                        day: day,
                        month: month,
                        year: year
                    }
                }

                scope.years = [];
                for (var y = 1960; y < 2040; y++) {
                    scope.years.push(y);
                }
                scope.months = [];
                for (var m = 0; m < 12; m++) {
                    scope.months.push(m);
                }
                scope.days = [];
                for (var d = 1; d <= 31; d++) {
                    scope.days.push(d);
                }
                scope.rdate = parseDmy(scope.model);
                scope.$watchCollection('rdate', function (rdate, oldValue) {
                    var day = rdate.day, month = rdate.month, year = rdate.year;
                    if (angular.isUndefined(day) || angular.isUndefined(month) || angular.isUndefined(year) || isValidDate(year, month, day)) {
                        if (angular.isUndefined(day) || !day || !isFinite(day)) {
                            day = '00';
                        } else {
                            day = day.toString();
                        }
                        if (angular.isUndefined(month) || !month || !isFinite(month)) {
                            month = '00';
                        } else {
                            month = (month + 1).toString();
                        }
                        if (angular.isUndefined(year) || !year || !isFinite(year)) {
                            year = '0000';
                        } else {
                            year = year.toString();
                        }
                        scope.model = day + '-' + month + '-' + year;
                    } else {
                    }
                });

            }

        };
    })
    .directive('rguTimeInput', function () {
        function link(scope, element, attrs) {

        }

        return {
            restrict: 'EAC',
            link: link,
            scope: {
                name: '@',
                required: '@?',
                model: '=ngModel'
            },
            template: '<input bs-timepicker name="{{name}}Input" ng-model="model" ng-required="required"'
            + ' data-placement="auto" data-autoclose="1">'

        };
    })

    // .directive('rguSelect', function () {
    //     function link(scope, element, attrs) {
    //         if (!element.parent().hasClass * 'select') {
    //             element.wrap('<div class="select"/>');
    //         }
    //     }
    //
    //     return {
    //         restrict: 'A',
    //         link: link
    //     };
    // })

    .directive('input', function () {
        function link(scope, element, attrs) {
            if (element.is('.dropdown-menu input')) {
                element.removeClass('input');
            }
            else if (attrs['type'] === 'checkbox' || attrs['type'] === 'radio') {
                element.addClass('input');
            }
        }

        return {
            restrict: 'E',
            link: link
        };
    })
    .directive('searchbar', function () {
        function link(scope, element, attrs) {
        }

        return {
            restrict: 'EAC',
            link: link,
            templateUrl: RegitTemplatePath + '/searchbar.html'
        };
    })

    .directive('inputFile', function ($timeout) {
        function link(scope, element, attrs) {
            element.after('<label for="' + attrs.id + '">Choose a file...</label>');
            var input = element.get(0);
            var label = input.nextElementSibling,
                labelVal = label.innerHTML;
            input.addEventListener('change', function (e) {
                /*                var fileName = '';
                 if (this.files && this.files.length > 1)
                 fileName = (this.getAttribute('data-multiple-caption') || '').replace('{count}', this.files.length);
                 else
                 fileName = e.target.value.split('\\').pop();
                 if (fileName) {
                 label.innerHTML = fileName;
                 scope.fileName = fileName;
                 }
                 else
                 label.innerHTML = labelVal;*/
                $timeout(function () {
                    scope.onChange();
                });
            });
        }

        return {
            restrict: 'AEC',
            link: link,
            scope: {
                'onChange': '&'
            }
        };
    })

    .directive('rguDropdownFilter', function () {
        function link(scope, element, attrs) {
        }

        return {
            restrict: 'E',
            scope: {
                id: '@',
                items: '<',
                selected: '='
            },
            link: link,
            controller: function ($scope) {
                $scope.selectItem = function (index) {
                    $scope.selected = index;
                }
            },
            template: '<div class="btn-group" uib-dropdown is-open="type.isopen" class="dropdown-filter">'
            + '<button id="btn-dd-{{id}}" type="button" class="dropdown-filter-btn" uib-dropdown-toggle ng-disabled="disabled">'
            + 'Showing <b>{{items[selected]}}</b><span class="dd-icon glyphicon glyphicon-triangle-bottom"></span></button>'
            + '<ul class="dropdown-menu" uib-dropdown-menu role="menu" aria-labelledby="btn-dd-{{id}}">'
            + '<li ng-repeat-start="item in items" ng-if="item!==\'-\'" role="menuitem">'

            + '<a href="#" ng-class="{\'menuitem-checked\' : $index==selected}" ng-click="$event.preventDefault(); selectItem($index)">{{item}}</a></li>'
            + '<li ng-repeat-end ng-if="item===\'-\'" class="divider">'
            + '</ul></div>'
        };
    })

    .directive('rguUserPicker', function ($sce, msgService) {
        function link(scope, elem, attrs) {
            if (attrs.inline) {
                elem.addClass('user-picker-inline');
            }
            var network = attrs.network || 'friends';
            scope.users = msgService.fetchUsers(network);
            scope.matchedUser = null;
            // scope.pickedUsers = angular.extend({},scope.model);
            scope.pickedUsers = scope.model;
            scope.pickMatchedUser = function ($item, $model, $label, $event) {
                scope.pickedUsers.push(scope.matchedUser);
                scope.matchedUser = null;
            };
            scope.getAvatarImgHtmlFromUser = function (user) {
                var html = '<img src="' + user.avatar + '" class="user-picker-avatar">';
                return $sce.trustAsHtml(html);
            };
            scope.filterMatches = function (user) {
                return $.inArray(user, scope.pickedUsers) < 0;
            };
            scope.unpickUser = function (user) {
                var picked = $.inArray(user, scope.pickedUsers);
                if (picked >= 0) {
                    scope.pickedUsers.splice(picked, 1);
                }
            };

            elem.focus();
        }

        return {
            restrict: 'E',
            scope: {
                network: '@',
                model: '=ngModel'
            },
            link: link,
            // templateUrl: RegitTemplatePath + '/user-picker.html'
            template: '<input ng-model="matchedUser" placeholder="Type a name" typeahead-template-url="templates/user-picker-item.html"'
            + ' uib-typeahead="user as user.name for user in users | filter:{name:$viewValue} | filter:filterMatches"'
            + ' typeahead-on-select="pickMatchedUser()" typeahead-show-hint="false" typeahead-min-length="0" focus>'
        };
    })

    .directive('rguLocationPicker', function ($http) {
        var apiPath = 'http://dev.regitsocial.com/api/LocationService';

        return {
            restrict: 'EAC',
            scope: {
                model: '=ngModel'
            },
            templateUrl: RegitTemplatePath + '/location-picker.html',
            link: function (scope, elem, attrs) {
                scope.countries = [];
                $http({
                    method: 'POST',
                    url: apiPath + '/GetAllCountries'
                }).then(function (response) {
                    scope.countries = response.data.Countries;
                    angular.forEach(scope.countries, function (country) {
                        country.cities = [];
                    });
                    scope.model = angular.isObject(scope.model) ? scope.model : {
                        country: '',
                        city: ''
                    };
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

                    var countryName = scope.model.country;
                    scope.selectedCountry = findCountryByName(countryName);
                    scope.onChangeCountry();
                }, function (response) {
                    console.log('Error loading country list');
                });
                scope.showRegion = attrs.region === 'true';
                scope.showZIP = attrs.zip === 'true';
                scope.onChangeCountry = function () {
                    var country = scope.selectedCountry;
                    if (country) {
                        var cities = country.cities;
                        if (!angular.isDefined(cities) || !cities.length) {
                            $http({
                                method: 'POST',
                                url: apiPath + '/GetCitiesById',
                                data: {CountryCode: country.Code}
                            }).then(function (response) {
                                country.cities = response.data.Cities;
                            });
                        }
                        scope.model.country = country.Name;
                    }
                };

            }
        };

    })
    .directive('rguAddressInput', function () {
        return {
            restrict: 'EAC',
            scope: {
                model: '=ngModel'
            },
            templateUrl: RegitTemplatePath + '/address-input.html',
            link: function (scope, elem, attrs) {
                scope.model = angular.isObject(scope.model) ? scope.model : {
                    address: ''
                };
            }
        };
    })
    .directive('rguSmartSelect', function () {
        return {
            restrict: 'EAC',
            scope: {
                model: '=ngModel',
                items: '<?list',
                placeholder: '@?'
            },
            templateUrl: RegitTemplatePath + '/rgu-smart-select.html',
            link: function (scope, elem, attrs) {
                if (attrs.type) {
                    switch (attrs.type) {
                        case 'title':
                            scope.items = ['Mr.', 'Ms.', 'Mrs.', 'Dr.', 'Sir'];
                            break;
                        case 'pet':
                            scope.items = ['Dog', 'Cat', 'Bird', 'Fish', 'Other'];
                            break;

                    }
                }
            }
        };
    })

    .directive('rguTagInput', function () {
        return {
            restrict: 'EAC',
            scope: {
                placeholder: '@',
                model: '=ngModel',
                list: '<',
                label: '@'
            },
            templateUrl: RegitTemplatePath + '/rgu-tags-input.html',
            link: function (scope, eleme, attrs) {
                var tags = [];
                if (angular.isString(scope.model) && scope.model.length) {
                    tags = scope.model.split(',');
                } else if (angular.isArray(scope.model)) {
                    tags = scope.model.slice(0);
                }
                scope.tags = tags;
                scope.model = scope.model || '';
                scope.$watch('tags.length', function (newVal, oldVal) {
                    if (newVal === oldVal) return;
                    if (angular.isString(scope.model)) {
                        scope.model = scope.tags.join(',');
                    } else if (angular.isArray(scope.model)) {
                        scope.model = scope.tags.slice(0);
                    }
                });
            }
        };
    })
    .directive('rguSmartInput', function ($sce, $timeout, defLists) {
        return {
            restrict: 'EAC',
            scope: {
                placeholder: '@',
                model: '=ngModel',
                label: '@'
            },
            templateUrl: RegitTemplatePath + '/rgu-smart-input.html',
            link: function (scope, elem, attrs) {

                scope.model = scope.model || '';
                scope.$watch('model', function (newVal, oldVal) {
                    var tags = [];
                    if (angular.isString(scope.model) && scope.model.length) {
                        tags = scope.model.split(',');
                    } else if (angular.isArray(scope.model)) {
                        tags = scope.model.slice(0);
                    }
                    scope.tags = $.map(tags, function (tag) {
                        return {label: tag};
                    });
                });

                var list = [], richList = [];
                switch (attrs.type) {
                    case 'color':
                        list = defLists.getList('colors');
                        richList = $.map(list, function (color) {
                            return {
                                label: color.replace(/[A-Z]/g, ' $&').slice(1),
                                prefix: $sce.trustAsHtml('<span class="richlist-color-slot" style="background-color:'
                                    + color.toLowerCase() + '">&nbsp;</span>')
                            };
                        });
                        scope.placeholder = attrs.placeholder || 'Select color';
                        break;
                    case 'food':
                        list = defLists.getList('food');
                        richList = $.map(list, function (food) {
                            return {
                                label: food.label,
                                group: food.group

                            };
                        });
                        scope.placeholder = attrs.placeholder || 'Select food';
                        scope.groupBy = 'group';
                        break;
                    case 'holiday':
                        list = defLists.getList('holidays');
                        richList = $.map(list, function (holiday) {
                            return {
                                label: holiday.label,
                                group: holiday.group
                            };
                        });
                        scope.placeholder = attrs.placeholder || 'Select holiday';
                        scope.groupBy = 'group';
                        break;
                    case 'language':
                        list = defLists.getList('languages');
                        richList = $.map(list, function (lang) {
                            return {
                                label: lang.name
                            };
                        });
                        scope.placeholder = attrs.placeholder || 'Select language';
                        break;
                    case 'movie':
                        list = defLists.getList('movies');
                        richList = $.map(list, function (movie) {
                            return {
                                label: movie.title,
                                group: movie.category_name
                            };
                        });
                        scope.placeholder = attrs.placeholder || 'Select movie';
                        scope.groupBy = 'group';
                        break;
                    case 'musicGenre':
                        list = defLists.getList('musicGenres');
                        richList = $.map(list, function (genre) {
                            return {
                                label: genre
                            };
                        });
                        scope.placeholder = attrs.placeholder || 'Select music genre';

                        break;
                    case 'song':
                        list = defLists.getList('songs');
                        richList = $.map(list, function (song) {
                            return {
                                label: song.title + ' &nbsp; - &nbsp; ' + song.artist,
                                prefix: $sce.trustAsHtml('<span class="richlist-song"><img src="'
                                    + song.img_url + '"></span>')
                            };
                        });
                        scope.placeholder = attrs.placeholder || 'Select song';
                        break;
                    case 'tvShow':
                        list = defLists.getList('tvShows');
                        richList = $.map(list, function (show) {
                            return {
                                label: show
                            };
                        });
                        scope.placeholder = attrs.placeholder || 'Select TV show';
                        break;
                    case 'religion':
                        list = defLists.getList('religions');
                        richList = $.map(list, function (religion) {
                            return {
                                label: religion
                            };
                        });
                        scope.placeholder = attrs.placeholder || 'Select religion';
                        break;
                    case 'docCat':
                        list = defLists.getList('docCats');
                        richList = $.map(list, function (docCat) {
                            return {
                                label: docCat
                            };
                        });
                        scope.placeholder = attrs.placeholder || 'Select category';
                        break;
                }
                scope.richList = richList;
                scope.tagTransform = function (newTag) {
                    return {
                        label: newTag
                    };

                };

                scope.$watch('tags.length', function (newVal, oldVal) {
                    if (newVal === oldVal) return;
                    if (angular.isString(scope.model)) {
                        var items = $.map(scope.tags, function (item) {
                            return item.label;
                        });
                        scope.model = items.join(',');
                    } else if (angular.isArray(scope.model)) {
                        scope.model = $.map(scope.tags, function (item) {
                            return item.label;
                        });
                    }
                    if (attrs.onChange) {
                        $timeout(function () {
                            scope.$parent.$parent.$eval(attrs.onChange);
                        });
                    }
                });
            }
        };
    })

    .directive('rguControl', function ($compile) {

        return {
            restrict: 'E',
            scope: {
                model: '=ngModel',
                unitModel: '=unitModel',
                onChange: '&',
                options: '<',
                before: '<'
            },
            link: function (scope, elem, attr) {
                var type = attr.type || 'textbox';
                var name = attr.name || 'rguControl';
                var className = attr['class'];
                var required = angular.isDefined(attr.required) ? 'true' : 'false';
                if (type === 'static') {
                    //  Render Static field (fixed text)
                    var staticControl = angular.element('<div/>')
                        .addClass(className)
                        .addClass('static-control')
                        .attr({
                            'name': name,
                            'ng-model': 'model',
                            'ng-required': required
                        })
                        .html(attr.value);
                    $compile(staticControl)(scope);
                    elem.replaceWith(staticControl);
                } else if (type === 'textbox') {
                    //  Render Text field
                    var input = angular.element('<input/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'type': 'text',
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    if (angular.isDefined(attr.autofocus)) {
                        input.attr('autofocus', 'true');
                    }
                    $compile(input)(scope);
                    elem.replaceWith(input);
                } else if (type === 'password') {
                    //  Render Password field
                    var password = angular.element('<input/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'type': 'password',
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    if (angular.isDefined(attr.autofocus)) {
                        password.attr('autofocus', 'true');
                    }
                    $compile(password)(scope);
                    elem.replaceWith(password);
                } else if (type === 'textarea') {
                    //  Render Text area
                    var textarea = angular.element('<textarea/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'rows': attr.rows,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    $compile(textarea)(scope);
                    elem.replaceWith(textarea);
                } else if (type === 'date' || type === 'datecombo') {
                    //  Render Date field
                    type = type === 'datecombo' ? 'combo' : 'datepicker';
                    scope.$parent.date = new Date();
                    var dateInput = angular.element('<rgu-date-input/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'type': type,
                            'before': scope.before,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    $compile(dateInput)(scope);
                    elem.replaceWith(dateInput);
                } else if (type === 'datedmy') {
                    //  Render Free Date (dmy) field
                    var dateDmy = angular.element('<rgu-date-dmy/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    $compile(dateDmy)(scope);
                    elem.replaceWith(dateDmy);
                } else if (type === 'checkbox') {
                    //  Render Boolean selector (checkbox)
                    var checkbox = angular.element('<rgu-checkbox/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'label': scope.options,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    $compile(checkbox)(scope);
                    elem.replaceWith(checkbox);
                } else if (type === 'radio') {
                    //  Render Option picker (radio)
                    var radioGroup = angular.element('<rgu-radio-group/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'values': 'options',
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    if (attr.onChange) {
                        radioGroup.attr('on-change', attr.onChange);
                    }
                    $compile(radioGroup)(scope);
                    elem.replaceWith(radioGroup);
                } else if (type === 'select') {
                    //  Render Select (dropdown)
                    var select = angular.element('<select/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'ng-options': 'option as option for option in options',
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    var wrapper = angular.element('<div class="select"/>');
                    wrapper.append(select);
                    $compile(wrapper)(scope);
                    elem.replaceWith(wrapper);
                } else if (type === 'range') {
                    //  Render Range Select (dropdown)
                    scope.ranges = $.map(scope.options, function (range) {
                        var start = range[0];
                        var end = range[1];
                        var text = '';
                        if (!angular.isNumber(start)) {
                            text = '< ' + end;
                        } else if (!angular.isNumber(end)) {
                            text = '> ' + start;
                        } else {
                            text += start + ' ... ' + end;
                        }
                        return text;
                    });
                    scope.newModel = angular.isUndefined(scope.model) || scope.model === null;
                    var select = angular.element('<select/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'ng-options': 'index as range disable when !newModel&&index!==model for (index,range) in ranges',
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    var wrapper = angular.element('<div class="select"/>')
                        .addClass('range-select');
                    wrapper.append(select);
                    if (attr.subtype === 'currency') {
                        var subwrap = wrapper;
                        wrapper = angular.element('<div/>')
                            .append(subwrap)
                            .append('<span class="range-unit-value" ng-bind="unitModel"></span>');
                    }
                    $compile(wrapper)(scope);
                    elem.replaceWith(wrapper);

                } else if (type === 'qa') {
                    //  Render Custom question answer (text or dropdown)
                    if (angular.isUndefined(attr.choices) || attr.choices === 'false') {
                        if (!angular.isString(scope.model)) {
                            scope.model = '';
                        }
                        var input = angular.element('<input/>')
                            .addClass(className)
                            .attr({
                                'name': name,
                                'type': 'text',
                                'rules': attr.rules,
                                'ng-model': 'model',
                                'ng-required': required
                            });
                        $compile(input)(scope);
                        elem.replaceWith(input);
                    } else {
                        var select = angular.element('<select/>')
                            .addClass(className)
                            .attr({
                                'name': name,
                                'ng-options': 'choice.value for choice in options',
                                'ng-model': 'model',
                                'ng-required': required
                            });
                        var wrapper = angular.element('<div class="select"/>')
                            .addClass('choice-select');
                        wrapper.append(select);
                        $compile(wrapper)(scope);
                        elem.replaceWith(wrapper);
                    }

                } else if (type === 'history') {
                    //  Render History list
                    var list = angular.element('<div/>')
                        .addClass('rgu-history')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    var ul = angular.element('<ul/>');
                    angular.forEach(scope.model, function (item) {
                        var li = angular.element('<li/>')
                            .html(item);
                        ul.append(li);
                    });
                    list.append(ul);
                    $compile(list)(scope);
                    elem.replaceWith(list);
                } else if (type === 'numinput') {
                    //  Render input group (number input with unit)
                    var inputGroup = angular.element('<rgu-input-group/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'measure': scope.options,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'unit-model': 'unitModel',
                            'ng-required': required
                        });
                    $compile(inputGroup)(scope);
                    elem.replaceWith(inputGroup);
                } else if (type === 'smartselect') {
                    //  Render Smart select (dropdown autocomplete)
                    var smartSelect = angular.element('<rgu-smart-select/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'type': 'options',
                            'placeholder': attr.placeholder,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    $compile(smartSelect)(scope);
                    elem.replaceWith(smartSelect);
                } else if (type === 'tagsinput') {
                    //  Render Tags input (free text list)
                    var tagsInput = angular.element('<rgu-tag-input/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'list': 'options',
                            'placeholder': attr.placeholder,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    $compile(tagsInput)(scope);
                    elem.replaceWith(tagsInput);
                } else if (type === 'smartinput') {
                    //  Render Smart input (suggested text list)
                    var smartInput = angular.element('<rgu-smart-input/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'type': scope.options,
                            'placeholder': attr.placeholder,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    if (attr.onChange) {
                        smartInput.attr('on-change', attr.onChange);
                    }
                    $compile(smartInput)(scope);
                    elem.replaceWith(smartInput);
                } else if (type === 'location') {
                    //  Render Location picker (country, city, etc.)
                    var locationPicker = angular.element('<rgu-location-picker/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    if (attr.onChange) {
                        locationPicker.attr('on-change', attr.onChange);
                    }
                    $compile(locationPicker)(scope);
                    elem.replaceWith(locationPicker);
                } else if (type === 'address') {
                    //  Render address input
                    var addressInput = angular.element('<rgu-address-input/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    if (attr.onChange) {
                        addressInput.attr('on-change', attr.onChange);
                    }
                    $compile(addressInput)(scope);
                    elem.replaceWith(addressInput);
                } else if (type === 'doc') {
                    //  Render document control
                    var docControl = angular.element('<rgu-doc-control/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'on-change': scope.onChange,
                            'ng-required': required
                        });
                    $compile(docControl)(scope);
                    elem.replaceWith(docControl);
                }
            }
        };
    })
    .directive('rules', function () {
        return {
            restrict: 'A',
            require: "ngModel",
            link: function (scope, elem, attrs, ctrl) {
                if (elem.prop('tagName') === 'RGU-CONTROL') return;
                var ruleStrs = attrs.rules;
                var model = attrs.ngModel;
                var rules = {};
                if (angular.isDefined(attrs.required) || angular.isDefined(attrs.ngRequired) && attrs.ngRequired === 'true') {
                    rules.require = {
                        name: 'require',
                        rule: ''
                    };
                }
                if (!ruleStrs && !rules.required) return;
                ruleStrs = ruleStrs.split(',');

                angular.forEach(ruleStrs, function (ruleStr) {
                    var ruleParts = ruleStr.split(':');
                    if (ruleParts.length == 1) {
                        ruleParts.push('');
                    }
                    rules[ruleParts[0]] = {
                        name: ruleParts[0],
                        rule: ruleParts[1]
                    };
                });

                function passRule(rule) {
                    if (elem.message) {
                        /*       elem.message.remove();
                         delete elem.message;*/
                    }
                    return true;
                }

                function failRule(rule) {
                    var name = rule ? rule.name : 'value';
                    var text = name === 'require' ? 'This field is required. Please enter a value' :
                    'Please enter a valid ' + name;
                    if (name === 'require' && rules.length > 1) return false;
                    var id = 'msg-' + (attrs.name || 'formField-' + Date.now());
                    elem.message = elem.message || angular.element('<div id="' + id + '" class="form-field-message form-field-error"/>').insertAfter(elem);
                    elem.message.html(text);
                    // ctrl.$rollbackViewValue();
                    scope.$parent.isFormInvalid = true;
                    return false;
                }

                function validateRequired(modelValue, viewValue) {

                    if (viewValue) {
                        return passRule(rules.require);
                    }
                    return failRule(rules.require);
                }

                function validateNumber(modelValue, viewValue) {
                    if (ctrl.$isEmpty(modelValue)) {
                        return passRule(rules.number);
                    }
                    if (isFinite(viewValue)) {
                        return passRule(rules.number);
                    }
                    return failRule(rules.number);
                }

                function validateInteger(modelValue, viewValue) {
                    if (ctrl.$isEmpty(modelValue)) {
                        return passRule(rules.integer);
                    }
                    if (/^\-?\d+$/.test(viewValue)) {
                        return passRule(rules.integer);
                    }
                    return failRule(rules.integer);
                }

                function validatePersonName(modelValue, viewValue) {
                    if (ctrl.$isEmpty(modelValue)) {
                        return passRule(rules.name);
                    }
                    if (/^[a-zA-Z \.\-]+$/.test(viewValue)) {
                        return passRule(rules.name);
                    }
                    return failRule(rules.name);
                }

                function validatePhoneNumber(modelValue, viewValue) {
                    if (ctrl.$isEmpty(modelValue)) {
                        return passRule(rules.phone);
                    }
                    if (/^[0-9 \-\(\)]+$/.test(viewValue)) {
                        return passRule(rules.phone);
                    }
                    return failRule(rules.phone);
                }

                function validateEmail(modelValue, viewValue) {
                    if (ctrl.$isEmpty(modelValue)) {
                        return passRule(rules.email);
                    }
                    if (/^[-a-z0-9~!$%^&*_=+}{\'?]+(\.[-a-z0-9~!$%^&*_=+}{\'?]+)*@([a-z0-9_][-a-z0-9_]*(\.[-a-z0-9_]+)*\.(aero|arpa|biz|com|coop|edu|gov|info|int|mil|museum|name|net|org|pro|travel|mobi|today|[a-z][a-z])|[a-z]{3,20}|[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,5})?$/i.test(viewValue)) {
                        return passRule(rules.email);
                    }
                    return failRule(rules.email);
                }

                angular.forEach(rules, function (rule) {
                    switch (rule.name) {
                        case 'require':
                            ctrl.$validators.require = validateRequired;
                            break;
                        case 'number':
                            ctrl.$validators.number = validateNumber;
                            break;
                        case 'integer':
                            ctrl.$validators.integer = validateInteger;
                            break;
                        case 'name':
                            ctrl.$validators.personName = validatePersonName;
                            break;
                        case 'phone':
                            ctrl.$validators.phoneNumber = validatePhoneNumber;
                            break;
                        case 'email':
                            ctrl.$validators.email = validateEmail;
                            break;
                    }
                });

            }
        }
    })

    .directive('rguDocControl', function ($timeout) {
        return {
            restrict: 'EA',
            templateUrl: RegitTemplatePath + '/doc-control.html',
            scope: {
                restrict: '@',
                docs: '=ngModel',
                onChange: '&'
            },
            link: function (scope, elem, attrs) {
                scope.addDoc = function (fname) {
                    var doc = {
                        type: '',
                        fname: fname
                    };
                    if (!scope.docs) {
                        scope.docs = [];
                    }
                    scope.docs.push(doc);
                    $timeout(function() {
                        scope.onChange();
                    });
                };

                scope.deleteDoc = function (doc) {
                    var index = $.inArray(doc, scope.docs);
                    if (index >= 0) {
                        scope.docs.splice(index, 1);
                    }
                };

                scope.saveDoc = function () {

                }

            }
        }

    })
    // templateUrl: RegitTemplatePath + '/vault-profile.html',
    .directive('vaultProfile', function () {
        return {
            restrict: 'EA',
            templateUrl: '/Areas/regitUI/templates/vault-profile.html',
            scope: {
                strength: '<ngModel'
            },
            link: function (scope, elem, attrs) {
                scope.grades = [
                    { label: 'Basic', amount: 25 },
                    { label: 'Advanced', amount: 25 },
                    { label: 'Elite', amount: 25 },
                    { label: 'Eminent', amount: 25 }
                ];

                scope.$watch('strength',function(ov,nv) {
                    scope.currentGrade = undefined;
                    var amount = 0;
                    angular.forEach(scope.grades, function (grade, index) {
                        if (angular.isDefined(scope.currentGrade)) return;
                        amount += grade.amount;
                        if (scope.strength <= amount) {
                            scope.currentGrade = index;
                        }
                    });
                });
            }
        }
    });





