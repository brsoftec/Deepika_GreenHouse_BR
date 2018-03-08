angular.module('regit.ui', ['mgcrea.ngStrap', 'internationalPhoneNumber', 'defs', 'campaigns'])
    .value('rguView', {
        openingNotifications: false,
        openingPopover: false,
        openingMoreActions: false,
        openingContextMenu: false,
        openMoreActions: function (key, event) {
            event.stopPropagation();
            this.openingMoreActions = key;
        },
        closeMoreActions: function (key) {
            this.openingMoreActions = false;
        },
        isOpenMoreActions: function (key) {
            return this.openingMoreActions === key;
        },
        openPopover: function (popover, key, event) {
            event.stopPropagation();
            this.openingPopover = key;
            var target = $(event.target);
            var pos = target.position();
            popover.detach().insertAfter(target).css({
                left: pos.left,
                top: pos.top + target.height() + 8
            });
        },
        closePopover: function (key) {
            this.openingPopover = false;
            // popover.detach();
        }

    })
    .run(function ($moment) {
        $moment.calendarFormat = function (myMoment, now) {
            var diff = myMoment.diff(now, 'days', true);
            var nextMonth = now.clone().add(1, 'month');

            var retVal = diff < -6 ? 'sameElse' :
                diff < -1 ? 'lastWeek' :
                    diff < 0 ? 'lastDay' :
                        diff < 1 ? 'sameDay' :
                            diff < 2 ? 'nextDay' :
                                diff < 7 ? 'nextWeek' :
                                    // introduce thisMonth and nextMonth
                                    (myMoment.month() === now.month() && myMoment.year() === now.year()) ? 'thisMonth' :
                                        (nextMonth.month() === myMoment.month() && nextMonth.year() === myMoment.year()) ? 'nextMonth' : 'sameElse';
            return retVal;
        };
        moment.updateLocale('en', {
            calendar: {
                lastDay: '[Yesterday] LT',
                sameDay: '[Today] LT',
                nextDay: '[Tomorrow] LT',
                lastWeek: '[Last] dddd',
                nextWeek: 'dddd LT',
                sameElse: 'll'
            }
        });

    })
    .factory('rguCache', function ($http, $q) {
        var users = [];
        return {
            getUserAsync: function (accountId) {
                var deferred = $q.defer();
                var found = users.find(function (user) {
                    return user.accountId === accountId;
                });
                if (found) {
                    deferred.resolve(found);
                } else {
                    $http.get('/Api/Users/BasicProfile', {params: {accountId: accountId}})
                        .success(function (response) {
                            var user = response.data;
                            if (user) users.push(user);
                            return deferred.resolve(user);
                        }).error(function (error) {
                          //  console.log(error)
                        console.log("Error loading basic profile", error);
                    });
                }
                return deferred.promise;
            },
            getUserAsyncById: function (userId) {
                var deferred = $q.defer();
                var found = null;
                users.forEach(function (user) {
                    if (user.id === userId) {
                        found = user;
                    }
                });
                if (found) {
                    deferred.resolve(found);
                } else {
                    $http.get('/Api/Users/BasicProfile/' + userId)
                        .success(function (response) {
                            var user = response.data;
                            if (user) users.push(user);
                            users.push(user);
                            return deferred.resolve(user);
                        }).error(function (error, status) {
                        return deferred.reject(error);
                    });
                }
                return deferred.promise;
            }
        };
    })
    .factory('rgu', function ($moment) {
        return {
            validateEmail: function (email) {
                return /[a-z0-9_\-.]+@[a-z0-9_\-.]+\.([a-z]{2,})/gi.test(email);
            },
            toTitleCase: function (str) {
                if (!str) return '';
                return str.toString().replace(/\w\S*/g, function (txt) {
                    return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
                });
            },
            deCamelize: function (text) {
                if (!text) return '';
                var result = text.replace(/([a-z])([A-Z])/g, "$1 $2");
                return result.charAt(0).toUpperCase() + result.slice(1);
            },
            guid: function () {
                return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                    var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
                    return v.toString(16);
                });
                // function s4() {
                //     return Math.floor((1 + Math.random()) * 0x10000)
                //         .toString(16)
                //         .substring(1);
                // }
                // return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
                //     s4() + '-' + s4() + s4() + s4();
            },
            parseDate: function (value) {

                function isDateStr(str) {
                    if (str.length < 6) return false;
                    if (/^[a-zA-Z]/.test(str)) return false;
                    var res = Date.parse(str);
                    return !isNaN(res);
                }

                // console.log(value)

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
                } else if (isDateStr(value)) {
                    type = 'str';
                    date = new Date(value);
                } else {
                    return false;
                }
                // if (!date.getFullYear()) return false;
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

    .directive('autofocus', ['$timeout', function ($timeout) {
        return {
            restrict: 'A',
            link: function (scope, element) {
                $timeout(function () {
                    element.focus();
                });
            }
        }
    }])

    .directive('ngRightClick', function ($parse) {
        return function (scope, element, attrs) {
            var fn = $parse(attrs.ngRightClick);

            element.on('contextmenu', function (event) {
                scope.$apply(function () {
                    event.preventDefault();
                    fn(scope, {$event: event});
                });
            });
        };
    })
    .directive('rguEventListener', function (rguView, $timeout) {
            return {
                restrict: 'A',
                link: function (scope, el, attrs) {
                    scope.rguView = rguView;
                    scope.$on('document::click', function (ev, args) {
                        $timeout(function () {
                            //rguView.openingPopover = false;
                            rguView.openingNotifications = false;
                            rguView.openingMoreActions = false;
                            if (args.event.which !== 3) {
                                rguView.openingContextMenu = false;
                            }

                        });
                    });
                    scope.$on('document::contextmenu', function (ev, args) {
                        // var e = args.event;
                        // e.preventDefault();
                        // scope.$apply(function () {
                        //     // rguView.openingContextMenu = true;
                        // });
                    });

                    scope.openPopover = function (key, event) {
                        event.stopPropagation();
                        rguView.openingPopover = key;
                        rguView.openingNotifications = false;
                    };
                    scope.closePopover = function (key) {
                        rguView.openingPopover = false;
                    };
                    scope.openMoreActions = function (key, event) {
                        event.stopPropagation();
                        rguView.openingMoreActions = key;
                    };
                    scope.closeMoreActions = function (key) {
                        rguView.openingMoreActions = false;
                    };

                }
            }
        }
    )

    .factory('rguNotify', function () {
        return {
            add: function (msg, options) {
                var opts = {
                    message: msg,
                    title: '',
                    duration: 8000,
                    location: 'br'
                };

                if (options) {

                    if (options.title) {
                        opts.title = options.title;
                    }
                    if (options.type) {
                        opts.style = options.type;
                    }
                    if (options.url) {
                        opts.url = options.url;
                    }
                }
                $.growl(opts);
            }
        }
    })

    .factory('rguAlert', function ($modal) {
        return function(message, options, respond) {
            options = options || { };
            var style = true;
            if (options.hasOwnProperty('style')) {
                style = options.style;
            }
            var actions = false;
            if (options.hasOwnProperty('actions')) {
                actions = options.actions;
            }
            var modalOptions = {
                templateUrl: '/Areas/regitUI/templates/alert.modal.html?v=2',
                locals: { message: message, style: style, respond: respond},
                placement: 'center',
                static: true,
                show: false,
                // scope: scope,
                controller: function($scope, message) {
                    $scope.message = message;
                    $scope.style = style;
                    $scope.actions = actions;

                    $scope.closeAlert = function(hideFunc, confirmed) {
                        if (confirmed && angular.isFunction(respond)) {
                            respond();
                        }
                        if (angular.isFunction(hideFunc))
                            hideFunc();
                    }
                }
            };
            var modal = $modal(modalOptions);
            modal.$promise.then(modal.show);
        };
    })

    // .directive('alertModal', function () {
    //     return {
    //         restrict: 'AC',
    //         link: function (scope, element, attrs) {
    //             if (angular.isString(scope.alertModal.message))
    //             scope.htmlMessage = scope.alertModal.message;
    //
    //
    //         }
    //     };
    // })

    .factory('rguModal', function ($modal) {
        var version = '35';
        var modals = {
            'msg.addPeopleToConversation': {templateFile: 'msg-conversation-add-people'},
            'msg.manageConversation': {templateFile: 'msg-conversation-manage'},
            'vault.doc.editor': {templateFile: 'vault-doc-editor'},
            'help.intro': {
                templateFile: 'help-intro',
                staticBackdrop: true
            },
            'help.intro.business': {
                templateFile: 'help-intro-business',
                staticBackdrop: true
            },
            'add.business': {
                templateFile: 'add-business',
                staticBackdrop: true
            },
            'claim.business': {
                templateFile: 'claim-business'
            },
            'interaction.form': {
                templateFile: 'interaction-form',
                staticBackdrop: true
            },
            'interaction.preview': {
                templateFile: 'interaction-preview'
            },
            'interaction.form.viewer': {
                templateFile: 'interaction-form-viewer'
            },
            'interaction.participant.data.viewer': {
                templateFile: 'interaction-participant-data-viewer'
            },
            'handshake.request': {
                templateFile: 'handshake-request'
            },
            'handshake.request.viewer': {
                templateFile: 'handshake-request-viewer'
            },
            'handshake.new': {
                templateFile: 'handshake-new'
            },
            'handshake.view': {
                templateFile: 'handshake-view'
            },
            'handshake.edit': {
                templateFile: 'handshake-edit'
            },
            'vault.push': {
                templateFile: 'vault-push',
                staticBackdrop: true
            },
            'billing.plan.select': {
                templateFile: 'billing-plan-select',
                staticBackdrop: true
            },
            'billing.payment.card': {
                templateFile: 'billing-payment-card',
                staticBackdrop: true
            },
            'billing.payment.pay': {
                templateFile: 'billing-payment-pay',
                staticBackdrop: true
            },
            'billing.quota.nofree': {
                templateFile: 'billing-quota-nofree',
                staticBackdrop: true
            },
            'billing.quota.expried': {
                templateFile: 'billing-quota-expired.modal',
                staticBackdrop: true
            },
            'billing.quota.limit': {
                templateFile: 'billing-quota-limit',
                staticBackdrop: true
            },
            'billing.promocode.generate': {
                templateFile: 'billing-promocode-generate',
                staticBackdrop: true
            },
            'support.case': {
                templateFile: 'support-case',
                staticBackdrop: true
            },
            'support.case.edit': {
                templateFile: 'support-case-edit',
                staticBackdrop: true
            }
        };

        return {
            openModal: function (name, scope, parms) {
                if (!(name in modals)) return;
                if (angular.isObject(parms)) {
                    scope.modalParms = parms;
                    for (var prop in parms) {
                        if (parms.hasOwnProperty(prop)) {
                            scope[prop] = parms[prop];
                        }
                    }
                }
                var templateUrl = '/Areas/regitUI/templates/' + modals[name].templateFile + '.modal.html?v=' + version;
                var modalOptions = {
                    scope: scope,
                    templateUrl: templateUrl,
                    placement: 'center',
                    show: false
                };
                if (modals[name].staticBackdrop) {
                    modalOptions.backdrop = 'static';
                }
                var modal = $modal(modalOptions);

                modal.$promise.then(modal.show);
            }
        };
    })
    .filter('userDisplayName', function () {
        return function (user) {
            if (user.displayname != "")
                return user.displayname;
            var fullName = user.firstName;
            if (user.middleName) {
                fullName += ' ' + user.middleName;
            }
            if (user.lastName) {
                fullName += ' ' + user.lastName;
            }
            return fullName;
        };
    })
    .filter('searchHighlight', function () {
        return function (text, query) {
            //text = window.encodeURIComponent(text);
            if (text)
                return text.replace(new RegExp(query, 'gi'), '<span class="search-highlight">$&</span>');
            else
                return "";
        };
    })
    .filter('titleCase', function () {
        return function (text) {
            if (!text) return '';
            return rgu.toTitleCase(text);
        }
    })
    .filter('lowerFirst', function () {
        return function (text) {
            if (!text) return '';
            return text.charAt(0).toLowerCase() + text.slice(1);
        }
    })
    .filter('friendlyTime', function ($moment, rgu) {
        var friendlyTimeFilter = function (date, options) {
            if (!date) return '';
            var parsedDate = rgu.parseDate(date);
            if (!parsedDate) return '';
            var moment = $moment.utc(date);//parsedDate.date);
            if (moment.year() < 1900) return '';
            moment.local();

            var duration = $moment.duration($moment().diff(moment));
            var days = duration.asDays();
            if (options === 'days') {
                return days < 1 ? 'today' : moment.fromNow();
            } else if (options === 'inDay') {
                return days < 1 ? 'today' : (days < 2 ? 'yesterday' : moment.format('MMMM D, YYYY'));
            } else if (options && options.hasOwnProperty('subtract')) {
                moment.subtract(options.subtract, 'minutes');
            }
            var hours = duration.asHours();
            return hours > 64 ? moment.calendar() : moment.fromNow();
        };
        friendlyTimeFilter.$stateful = true;
        return friendlyTimeFilter;
    })
    .filter('prettyTime', function ($moment, rgu) {
        var filter = function (date, options) {
            if (!date) return '';
            var parsedDate = rgu.parseDate(date);
            if (!parsedDate) return '';
            var format = 'D MMM YYYY';
            switch (options) {
                case 'ymd':
                    format = 'YYYY/MM/DD';
                    break;
                case 'ymd-time':
                    format = 'YYYY/MM/DD h:mm a';
                    break;
                case 'full':
                    format = 'MMMM D, YYYY';
                    break;
                case 'full-time':
                    format = 'MMMM D, YYYY, h:mm a';
                    break;
                case 'long':
                    format = 'dddd, MMMM D YYYY, h:mm a';
                    break;
                case 'time':
                    format = 'h:mm a';
                    break;
                case 'ftime':
                    format = 'hh:mm a';
                    break;
            }
            var moment = $moment.utc(date);//parsedDate.date);
            moment.local();

            return moment.format(format);

        };
        // filter.$stateful = true;
        return filter;
    })
    .filter('industryToHtml', function () {
        return function(industry) {
            if (!industry) return '';
            return industry.split(',').join('<br>')
        };
    })

    .filter('locationToHtml', function () {

        var filter = function (location) {
            if (!location)
                return '';
            var html = '';
            if (location.street) {
                html += location.street;
            }
            if (location.country || location.city) {
                if (location.street) {
                    html += '<br>';
                }
                if (location.city) {
                    html += location.city;
                }
                if (location.city && location.country) {
                    html += ', ';
                }
                html += location.country || '';
            }
            if (location.zipCode) {
                html += '<br>' + location.zipCode
            }
            return html;
        };
        filter.$stateful = true;
        return filter;
    })
    .filter('profileAddress', function () {

        var addressFilter = function (profile) {
            if (!profile)
                return '';
            var html = profile.Street || '';
            html += '<br>';
            html += profile.City || '';
            if (profile.City && profile.Country) {
                html += ', ';
            }
            html += profile.Country || '';
            return html;
        };
        addressFilter.$stateful = true;
        return addressFilter;
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
                labels: '<?',
                model: '=ngModel'
            },
            template: '<label ng-repeat="value in values" class="btn btn-default btn-option" uib-btn-radio="value"'
            + ' ng-model="model" ng-class="{active: model===value}" ng-click="select($event,value)">{{labels[$index]}}</label>',
            controller: function ($scope, $element) {
                $scope.select = function (e, value) {
                    $element.find('label').removeClass('active');
                    $scope.model = value;
                    angular.element(e.target).addClass('active');
                }
            }
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
            scope.units = measureUnits[scope.measure];

            if (!scope.measure) {
                scope.measure = 'length';
            } else if (scope.measure === 'currency') {
                scope.type = 'number';
                scope.attrs = '';//'ng-pattern="/^[0-9]+(\\.[0-9]{1,3})?$/" step="1"';
            }
            if (!scope.type) {
                scope.type = 'number';
            }
            if (!scope.unitModel) {
                scope.unitModel = scope.units[0]
            }
            scope.$watch('model',function(newValue) {
                if (angular.isUndefined(newValue)) {
                    scope.model = 0;
                }
            });

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
            template: function (elem, attrs) {
                var html = '<input type="{{type}}" name="{{name}}Input" ng-model="model" ng-change="onChange()"';
                if (attrs.measure === 'currency') {
                    html += ' min="0" ng-pattern="/^[0-9]+(\.[0-9]*)?$/" step="1"';
                }
                html += ' ng-required="required" min="{{min}}" rules="number"/> '
                    + '<div rgu-picker class="picker" items="units" ng-model="unitModel"/>';
                return html;
            }

        };
    })
    .directive('rguRadioGroup', function ($timeout) {

        return {
            restrict: 'EAC',
            scope: {
                name: '@',
                type: '@?',
                values: '<?',
                labels: '=?',
                model: '=ngModel'
            },
            template: '<div class="radiogroup">'
            + '<div class="radiobtn" ng-repeat="value in values">'
            + '<input type="radio" name="{{name}}" ng-value="valueOf(value)" id="{{name}}-{{value}}" ng-model="$parent.model"'
            + ' ng-checked="valueOf(value)===model" ng-change="onChange()">'
            + '<label for="{{name}}-{{value}}">{{labels[$index]}}</label>'
            + '</div></div>',
            link: function (scope, element, attrs) {
                if (attrs.type === 'yesno') {
                    scope.values = ['Yes', 'No'];
                }
                if (!angular.isArray(scope.labels)) {
                    scope.labels = scope.values;
                } else {

                }
                scope.valueOf = function (value) {
                    if (attrs.type === 'yesno') return value === 'Yes';
                    if (attrs.type === 'boolean') return (value === scope.values[1]);
                    return value;
                };
                scope.labelOf = function (value) {
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

        };
    })
    // .directive('rguSwitch', function () {
    //     return {
    //         restrict: 'AEC',
    //         scope: {
    //             name: '@',
    //             model: '=ngModel'
    //         },
    //         template: '<span class="rgu-switch-label rgu-switch-false">NO</span>'
    //         + '<input type="checkbox" class="js-switch" ng-model="model" ui-switch="{color: \'#00adef\', secondaryColor: \'#ccc\'}" />'
    //         + '<span class="rgu-switch-label rgu-switch-true">YES</span>'
    //
    //     }
    // })
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
    .directive('rguDateDmy', function ($parse, $moment) {

        return {
            restrict: 'EAC',
            scope: {
                name: '@',
                required: '@?',
                model: '=ngModel'
            },
            templateUrl: '/Areas/regitUI/templates/date-dmy.html',
            link: function link(scope, element, attrs) {
                function isValidDate(year, month, day) {
                    return $moment({
                        y: year,
                        M: month,
                        d: day
                    }).isValid();
                }

                function parseDmy(dmy) {
                    if (angular.isDate(dmy)) return {
                        day: dmy.getDate(),
                        month: dmy.getMonth(),
                        year: dmy.getFullYear()
                    };
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

                scope.rdate = parseDmy(scope.model);

                scope.years = [];
                for (var y = 1900; y < 2040; y++) {
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

                scope.$watchCollection('rdate', function (rdate, oldValue) {
                    var day = rdate.day, month = rdate.month, year = rdate.year;
                    if (angular.isUndefined(day) || angular.isUndefined(month) || angular.isUndefined(year) || isValidDate(year, month, day)) {
                        if (angular.isUndefined(day) || !day || !isFinite(day)) {
                            day = '00';
                        } else {
                            if (day < 10) {
                                day = '0' + day;
                            } else {
                                day = day.toString();
                            }
                        }
                        if (angular.isUndefined(month) || !isFinite(month)) {
                            month = '00';
                        } else {
                            month = (month + 1);
                            if (month < 10) {
                                month = '0' + month;
                            } else {
                                month = month.toString();
                            }
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

    //new 20/10
    .directive('rguDateInput', function ($timeout, $compile, $moment, rgu) {

        return {
            restrict: 'EAC',
            scope: {
                name: '@',
                type: '@',
                indef: '@?',
                required: '@?',
                model: '=ngModel',
                before: '<',
                after: '<'

            },
            templateUrl: '/Areas/regitUI/templates/date-input.html',
            link: function (scope, element, attrs) {
                // console.log(scope.model)
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
                    }, true);
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
                    // console.log(scope.after);
                    // scope.$parent.$watchCollection(attrs.after, function (date,ov) {
                    scope.$watch('after', function (date, ov) {
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
                    }, true);
                }

                scope.view = {
                    indefinite: false
                };

                if (scope.model) {
                    var parsedDate = rgu.parseDate(scope.model);
                    if (parsedDate) {
                        scope.rdate = parsedDate.dmy;
                    }
                } else if (scope.indef) {
                    scope.view.indefinite = true;
                }
                if (!scope.rdate) {
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

                scope.onIndefChange = function () {
                    $timeout(function () {
                        if (scope.view.indefinite) {
                            scope.model = null;
                        }
                    });
                };

                function validate(rdate) {
                    var type = attrs.type;
                    var combo = type === 'combo';
                    if (combo && !rdate) return;
                    var day, month, year;
                    if (combo) {
                        day = rdate.day - 1;
                        month = rdate.month;
                        year = rdate.year;

                    } else {
                        if (angular.isDate(scope.model)) {
                            day = scope.model.getDate() - 1;
                            month = scope.model.getMonth();
                            year = scope.model.getFullYear();
                        }
                    }
                    var moment = combo ? $moment({
                        y: year,
                        M: month,
                        d: day + 1
                    }) : $moment(scope.model);
                    // console.log(moment);
                    scope.error.status = false;
                    if (scope.view.indefinite) {
                        scope.model = null;
                    } else {
                        if (combo && scope.isDirty && (day === null || month === null || year === null)) {
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
                            $timeout(function () {
                                scope.model.setDate(day + 1);
                                scope.model.setMonth(month);
                                scope.model.setFullYear(year);
                                // console.log(scope.model)
                            });
                        }
                    }
                }

                function updateDmy() {

                    scope.years = [];
                    var minYear = 1900, maxYear = 2040;
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

    .directive('input', function ($animate) {
        function link(scope, element, attrs) {
            if (element.is('.dropdown-menu input, .switch')) {
                element.removeClass('input');
            }
            else if (attrs['type'] === 'checkbox') {
                element.addClass('input');
                $animate.enabled(element, false);
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
            templateUrl: '/Areas/User/Views/Campaign/templates/searchbar.html'
        };
    })

    .directive('inputFile', function () {
        function link(scope, element, attrs) {
            element.after('<label for="' + attrs.id + '">Choose a file...</label>');
            var input = element.get(0);
            var label = input.nextElementSibling,
                labelVal = label.innerHTML;
            input.addEventListener('change', function (e) {
                var fileName = '';
                if (this.files && this.files.length > 1)
                    fileName = (this.getAttribute('data-multiple-caption') || '').replace('{count}', this.files.length);
                else
                    fileName = e.target.value.split('\\').pop();
                //
                // if (fileName)
                //     label.innerHTML = fileName;
                // else
                //     label.innerHTML = labelVal;
            });
        }

        return {
            restrict: 'AEC',
            scope: true,
            link: link
        };
    })

    .directive('rguUpload', function () {
        return {
            restrict: 'A',
            scope: true,
            link: function (scope, el, attrs) {
                var lastLabel;
                el.bind('change', function (event) {

                    var files = event.target.files;
                    //iterate files since 'multiple' may be specified on the element
                    if (files.length == 0) {
                        scope.$emit("upload:selected", {file: null, field: event.target.name});

                    } else {

                        // for (var i = 0;i<files.length;i++) {
                        //     scope.$emit("upload:selected", { file: files[i], field: event.target.name });
                        // }
                        scope.$emit("upload:selected", {file: files[files.length - 1], field: event.target.name});
                    }
                });
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
            // templateUrl: 'templates/user-picker.html'
            template: '<input ng-model="matchedUser" placeholder="Type a name" typeahead-template-url="/Areas/regitUI/templates/user-picker-item.html"'
            + ' uib-typeahead="user as user.name for user in users | filter:{name:$viewValue} | filter:filterMatches"'
            + ' typeahead-on-select="pickMatchedUser()" typeahead-show-hint="false" typeahead-min-length="0" focus>'
        };
    })

    .directive('rguLocationPicker', function ($http) {
        var apiPath = '/api/LocationService';
        return {
            restrict: 'EAC',
            scope: {
                model: '=ngModel'
            },
            templateUrl: '/Areas/regitUI/templates/location-picker.html',
            link: function (scope, elem, attrs) {
                scope.noCity = attrs.nocity;
                scope.countryLabel = attrs.countryLabel || 'Country';
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
                    console.log('Error loading country list', response);
                });
                scope.showRegion = attrs.region === 'true';
                scope.showZIP = attrs.zip === 'true';
                scope.onChangeCountry = function () {
                    var country = scope.selectedCountry;
                    if (country) {
                        if (!scope.noCity) {
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
            templateUrl: '/Areas/regitUI/templates/address-input.html',
            link: function (scope, elem, attrs) {
                scope.model = angular.isObject(scope.model) ? scope.model : {
                    address: ''
                };
            }
        };
    })

    .directive('rguRangeInput', function () {
        return {
            restrict: 'AE',
            scope: {
                name: '@',
                type: '@',
                model: '=ngModel',
                unitModel: '=unitModel',
                invalid: '='
            },
            templateUrl: '/Areas/regitUI/templates/range-input.html?v=4',

            link: function (scope, elem, attrs) {
                scope.validate = {
                    invalid: false
                };

                scope.addRange = function () {
                    var start = null;
                    if (scope.model.length > 0) {
                        start = scope.model[scope.model.length - 1][1] + 1;
                    } else {
                        start = 1;
                    }
                    var end = start < 100 ? 100 : null;
                    scope.model.push([start, end]);
                };
                scope.removeRange = function (index) {
                    scope.model.splice(index, 1);
                };
                scope.model = scope.model || [];
                if (!scope.model.length) {
                    scope.addRange();
                }

                scope.checkRange = function(oldVal, newVal) {
                    var ranges = scope.model;
                    angular.forEach(ranges, function(range) {
                        var start = range[0];
                        var end = range[1];
                        scope.validate.invalid = !start || !end || end < start;
                        scope.invalid = scope.validate.invalid;
                    });
                };

                scope.$watch('model', scope.checkRange, true);

            }
        }
    })
    .directive('rguRangeSlider', function () {
        return {
            restrict: 'AE',
            scope: {
                name: '@',
                model: '=ngModel'
            },
            // template: '<div class="range-add" ng-bind="ranges"></div>'
            // + '<vds-multirange ng-model="ranges" view="viewIndex" views="views"></vds-multirange>',
            templateUrl: '/Areas/regitUI/templates/range-slider.html',

            link: function (scope, elem, attrs) {
                // scope.probs
                scope.addItem = function () {

                };
                scope.removeItem = function () {

                };
                scope.sliders = [
                    {
                        title: 'User 1: ',
                        value: 100,
                        color: 'Red'
                    },
                    {
                        title: 'User 2: ',
                        value: 200,
                        color: '#00FF00'
                    },
                    {
                        title: 'User 3: ',
                        value: 450,
                    }
                ];
                scope.data = [986, 3125, 4553, 8588, 10780];
                scope.sliderConfig = new ngMultiPoint.ScaleConfig(1, 12000, 40, 840);
                scope.histogramConfig = new ngMultiPoint.ScaleConfig(1, 12000, 1, 100);
            }
        }
    })

    // .directive('rguCheckbox', function () {
    //     function link(scope, element, attrs) {
    //         scope.label = scope.label || '';
    //     }
    //
    //     return {
    //         restrict: 'AEC',
    //         scope: {
    //             'label': '@',
    //             'model': '=ngModel',
    //             'ngDisabled': '&',
    //             'onChange': '&'
    //         },
    //         link: link,
    //         template: function (elem, attr) {
    //             var html = '<input type="checkbox" class="input" ng-model="model" ng-disabled="ngDisabled()" ng-change="onChange()">';
    //             if (!attr.label) {
    //                 html += '<label>&nbsp;</label>';
    //             } else {
    //                 html += '<label>{{label}}</label>';
    //             }
    //             return html;
    //         }
    //     };
    // })
    .directive('rguCheckbox', function () {
        var cmpId = 1;
        return {
            restrict: 'AEC',
            scope: {
                'label': '@',
                'model': '=ngModel',
                'ngDisabled': '&',
                'ngChange': '&'
            },
            template: function (elem, attr) {
                var html = '<div class="checkbox-switch">';
                html += '<input type="checkbox" id="checkbox{{cmpId}}" class="switch" ng-model="model" ng-disabled="ngDisabled()" ng-change="ngChange()">';
                html += '<label for="checkbox{{cmpId}}">';

                html += '</div><div class="checkbox-label">';
                html += '<label>';
                if (!attr.label) {
                    html += '&nbsp';
                    html += '&nbsp';
                } else {
                    html += attr.label;
                }
                html += '</label>';
                return html;
            },
            link: function (scope, elem, attrs) {
                scope.label = scope.label || '';
                scope.cmpId = cmpId++;
                elem.addClass('rgu-checkbox');
            }
        };
    })

    //vault

    .directive('rguSmartSelect', function () {
        return {
            restrict: 'EAC',
            scope: {
                model: '=ngModel',
                items: '<?list',
                placeholder: '@?'
            },
            templateUrl: '/Areas/regitUI/templates/rgu-smart-select.html',
            link: function (scope, elem, attrs) {
                if (attrs.type) {
                    switch (attrs.type) {
                        case 'title':
                            scope.items = ['Mr.', 'Ms', 'Mrs.', 'Dr.', 'Sir'];
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
            templateUrl: '/Areas/regitUI/templates/rgu-tags-input.html',
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
    .directive('rguAnswerInput', function () {
        return {
            restrict: 'AE',
            scope: {
                name: '@',
                type: '@',
                model: '=ngModel'
            },
            templateUrl: '/Areas/regitUI/templates/answer-input.html',

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
    .directive('rguSmartInput', function ($sce, $timeout, defLists) {
        return {
            restrict: 'EAC',
            scope: {
                placeholder: '@',
                model: '=ngModel',
                label: '@'
            },
            templateUrl: '/Areas/regitUI/templates' + '/rgu-smart-input.html',
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
                    case 'industry':
                        list = defLists.getList('industries');
                        richList = list; //$.map(list, function (industry) {
                        // return {
                        //     label: industry.name,
                        //     group: industry.cats
                        // };
                        // });
                        scope.placeholder = attrs.placeholder || 'Select industry';
                        scope.groupBy = 'group';
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
                options: '<',
                ruleFails: '=?',
                onChange: '&?',
                validate: '=?'
            },
            link: function (scope, elem, attr) {
                var type = attr.type || 'textbox';
                var name = attr.name || 'rguControl' + new Date().getUTCMilliseconds();
                var className = attr['class'];
                var required = angular.isDefined(attr.required) ? 'true' : 'false';

                if (type === 'static') {
                    //  Render Static field (fixed text)
                    var staticControl = angular.element('<div/>')
                        .addClass(className)
                        .addClass('static-control')
                        .html(attr.value);
                    $compile(staticControl)(scope);
                    elem.replaceWith(staticControl);
                }
                else if (type === 'textbox') {
                    if (attr.rules === 'phone' || angular.isDefined(scope.options) && scope.options === 'phone') {
                        //  Render Text field
                        // scope.phoneCode = '';

                        var input = angular.element('<rgu-phone-input/>')
                            .addClass(className)
                            .attr({
                                'name': name,
                                'rules': attr.rules,
                                'rule-fails': 'ruleFails',
                                'validate': 'validate',
                                'ng-model': 'model',
                                'phone-type': 'full',
                                'phone-code': 'phoneCode',
                                'full-number': 'fullNumber',
                                'ng-required': required,
                                'placeholder': attr.placeholder
                            });
                        if (angular.isFunction(scope.onChange)) {
                            input.attr('ng-change', 'onChange()');
                        }
                        if (angular.isDefined(attr.autofocus)) {
                            input.attr('autofocus', 'true');
                        }
                        $compile(input)(scope);
                        elem.replaceWith(input);
                    } else {
                        //  Render Text field
                        var input = angular.element('<input/>')
                            .addClass(className)
                            .attr({
                                'name': name,
                                'type': 'text',
                                'rules': attr.rules,
                                'rule-fails': 'ruleFails',
                                'ng-model': 'model',
                                'ng-required': required,
                                'placeholder': attr.placeholder
                            });
                        if (angular.isFunction(scope.onChange)) {
                            input.attr('ng-change', 'onChange()');
                        }
                        if (angular.isDefined(attr.autofocus)) {
                            input.attr('autofocus', 'true');
                        }

                        $compile(input)(scope);
                        elem.replaceWith(input);
                    }
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
                    type = 'combo'; //type === 'datecombo' ? 'combo' : 'datepicker';
                    var dateInput = angular.element('<rgu-date-input/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'type': type,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    if (scope.options === 'indef') {
                        dateInput.attr('indef', 'true');
                    }
                    $compile(dateInput)(scope);
                    elem.replaceWith(dateInput);
                }
                else if (type === 'datedmy') {
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
                }
                else if (type === 'checkbox') {
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
                } else if (type === 'contact') {
                    //  Render Emergency Contact selector
                    var selector = angular.element('<emergency-contact-selector/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'type': scope.options,
                            'ng-model': 'model',
                            'contacts': 'options',
                            'ng-required': required
                        });
                    if (attr.onChange) {
                        selector.attr('on-change', attr.onChange);
                    }
                    $compile(selector)(scope);
                    elem.replaceWith(selector);
                }
                else if (type === 'qa') {
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
                                'ng-options': 'choice for choice in options',
                                'ng-model': 'model',
                                'ng-required': required
                            });
                        var wrapper = angular.element('<div class="select"/>')
                            .addClass('choice-select');
                        wrapper.append(select);
                        $compile(wrapper)(scope);
                        elem.replaceWith(wrapper);
                    }

                }

                else if (type === 'numinput') {
                    //  Render input group (number input with unit)
                    var inputGroup = angular.element('<rgu-input-group/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'measure': scope.options,
                            'rules': attr.rules,
                            'ng-model': 'model.amount',
                            'unit-model': 'model.unit',
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
                            'type': attr.options,
                            'placeholder': attr.placeholder,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    $compile(smartSelect)(scope);
                    elem.replaceWith(smartSelect);
                }
                else if (type === 'tagsinput') {
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
                }
                else if (type === 'history') {
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
                }
                else if (type === 'range') {
                    //  Render Range Select (dropdown)
                    if (!angular.isArray(scope.options)) return '';
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
                            // 'ng-options': 'index as range disable when !newModel&&index!==model for (index,range) in ranges',
                            'ng-options': 'index as range for (index,range) in ranges',
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

                }
                else if (type === 'doc') {
                    //  Render document control
                    var docControl = angular.element('<rgu-doc-control/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    $compile(docControl)(scope);
                    elem.replaceWith(docControl);
                }

                else if (type === 'location') {
                    //  Render Location picker (country, city, etc.)
                    var locationPicker = angular.element('<rgu-location-picker/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'rules': attr.rules,
                            'ng-model': 'model',
                            'ng-required': required
                        });
                    if (scope.options === 'nocity' || attr.subtype === 'nocity') {
                        locationPicker.attr('nocity', 'true');
                    }
                    if (attr.onChange) {
                        locationPicker.attr('on-change', attr.onChange);
                    }
                    $compile(locationPicker)(scope);
                    elem.replaceWith(locationPicker);
                } else if (type === 'address') {
                    //  Render address input
/*                    var addressInput = angular.element('<rgu-address-input/>')
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
                    elem.replaceWith(addressInput);*/

                    //  Render Text field
                    var input = angular.element('<input/>')
                        .addClass(className)
                        .attr({
                            'name': name,
                            'type': 'text',
                            'rules': attr.rules,
                            'rule-fails': 'ruleFails',
                            'ng-model': 'model',
                            'ng-required': required,
                            'placeholder': 'Street #, Street Name, Unit #'
                        });
                    if (angular.isFunction(scope.onChange)) {
                        input.attr('ng-change', 'onChange()');
                    }

                    $compile(input)(scope);
                    elem.replaceWith(input);
                }
            }
        };
    })

    .directive('rules', function () {
        return {
            restrict: 'A',
            require: "ngModel",
            ruleFails: '=',
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
                        elem.message.empty();
                    }
                    if (angular.isDefined(scope.ruleFails)) {
                        scope.ruleFails = undefined;
                    }
                    return true;
                }

                function failRule(rule) {
                    var name = rule ? rule.name : 'value';
                    var text = name === 'require' ? 'This field is required. Please enter a value' :
                        'Please enter a valid ' + name;
                    if (name === 'require' && rules.length > 1) return false;
                    var id = 'msg-' + (attrs.name || 'formField-' + Date.now());
                    if (elem.message) {
                        elem.message.empty();
                    } else {
                        elem.message = angular.element('<div id="' + id + '" class="rules-message"/>').insertAfter(elem);
                    }
                    var msgText = angular.element('<div class="rules-message-text balloon"/>');
                    msgText.html(text);
                    elem.message.append(msgText);
                    // ctrl.$rollbackViewValue();
                    scope.$parent.isFormInvalid = true;
                    if (angular.isDefined(scope.ruleFails)) {
                        scope.ruleFails = rule;
                    }

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
                    var message = elem.message;
                    if (message) {
                        message.remove();
                        delete elem.message;
                    }
                    if (ctrl.$isEmpty(modelValue)) {
                        return passRule(rules.phone);
                    }
                    if (/^\+*[0-9 \-\(\)]+$/.test(viewValue)) {
                        return passRule(rules.phone);
                    }
                    return failRule(rules.phone);
                }

                function validateEmail(modelValue, viewValue) {
                    var message = elem.message;
                    if (message) {
                        message.remove();
                        delete elem.message;
                    }

                    if (ctrl.$isEmpty(modelValue)) {
                        return passRule(rules.email);
                    }
                    if (/^[-a-z0-9~!$%^&*_=+}{\'?]+(\.[-a-z0-9~!$%^&*_=+}{\'?]+)*@([a-z0-9_][-a-z0-9_]*(\.[-a-z0-9_]+)*\.(aero|arpa|biz|com|coop|edu|gov|info|int|mil|museum|name|net|org|pro|travel|mobi|today|[a-z]{2,10})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,5})?$/i.test(viewValue)) {
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
            templateUrl: '/Areas/regitUI/templates' + '/doc-control.html',
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
                    $timeout(function () {
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
                    {
                        label: 'Basic',
                        amount: 25
                    },
                    {
                        label: 'Advanced',
                        amount: 25
                    },
                    {
                        label: 'Elite',
                        amount: 25
                    },
                    {
                        label: 'Eminent',
                        amount: 25
                    }
                ];

                scope.$watch('strength', function (ov, nv) {
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
    })

    .directive('rguPhoneInput', function ($timeout) {

        return {
            restrict: 'EA',
            templateUrl: '/Areas/regitUI/templates/phone-input.html?v=1',
            scope: {
                model: '=ngModel',
                phoneCode: '=?',
                placeholder: '@',
                name: '@',
                phoneType: '@',
                initialCountry: '@?',
                addPlus: '@?',
                validate: '=?',
                locked: '<'
            },
            link: function (scope, elem, attrs) {
                scope.name = scope.name || 'phoneInput';
                scope.control = scope['form' + scope.name][scope.name];
                scope.model = scope.model || '';
                scope.newModel = !scope.model;
                scope.phone = scope.model.toString();
                scope.phoneCode = scope.phoneCode || '';
                scope.initing = true;
                if (angular.isUndefined(scope.validate)) {
                    scope.validate = {invalid: false};
                }

                scope.isFull = (scope.phoneType === 'full');
                if (scope.isFull) {
                    // if (scope.model.length && scope.model.charAt(0) === '+') {
                    //     scope.model = scope.model.substring(1);
                    // }
                    // console.log(scope.phone)
                    //scope.fullNumber = scope.model;
                } else {
                }

                var inputElInited = false;

                var watchInput = scope.$watch('inputElement', function (el) {
                    if (el) {
                        inputElInited = true;
                        if (scope.isFull) {
                            // scope.model = el.intlTelInput('getNumber');
                            // console.log(scope.model)

                            if (scope.model) {
                                el.val(scope.model);
                                $timeout(function () {
                                    var countryData = el.intlTelInput("getSelectedCountryData");
                                    if (countryData && countryData.hasOwnProperty('dialCode')) {
                                        scope.phoneCode = countryData.dialCode;
                                    }
                                    // console.log(scope.phoneCode);
                                    scope.initing = false;
                                }, 500);
                            }

                        } else {
                            el.val(scope.phone);
                            $timeout(function () {
                                scope.model = el.val();
                                scope.initing = false;
                            }, 500);
                        }
                        watchInput();
                    }
                });

                scope.$watch('phone', function (newValue) {
                    if (newValue) {

                        if (newValue.charAt(0) === '+') {
                            newValue = newValue.substring(1);
                        }
                        if (scope.isFull) {
                            // console.log(scope.phoneCode,newValue);
                            if (scope.phoneCode) {

                            } else if (scope.newModel) {
                                scope.phoneCode = '65';
                                var el = scope.inputElement;
                                if (el) {
                                    var countryData = el.intlTelInput("getSelectedCountryData");
                                    if (countryData && countryData.hasOwnProperty('dialCode')) {
                                        scope.phoneCode = countryData.dialCode;
                                    }
                                }
                            }
                            scope.model = '+' + scope.phoneCode + newValue;

                        } else {
                            scope.model = newValue;
                            // scope.inputElement.val(newValue);
                        }

                        // console.log(newValue, scope.model)
                    }
                }, true);

                scope.$watch('phoneCode', function (newValue) {
                    if (newValue) {
                        // console.log('cc', newValue)
                        // scope.changedCC = true;
                        // if (scope.fullNumber) {
                        //     scope.model = scope.fullNumber;
                        // }
                        // scope.model = '+' + newValue + scope.phoneValue;
                        // scope.changedCC = true;
                        // newValue.val(scope.phone);
                        //
                        // console.log('phonecode', newValue);

                        $timeout(function () {
                            // scope.model = scope.inputElement.val();
                            scope.control.$validate();
                        }, 200);
                    }
                }, true);
                if (angular.isObject(scope.validate)) {
                    scope.$watch('control.$error', function (newValue) {
                        if (newValue) {
                            var invalid = newValue.hasOwnProperty('internationalPhoneNumber') && newValue.internationalPhoneNumber;
                            scope.validate.invalid = invalid;
                        }
                    }, true);
                }

            }
        }
    })
    .directive('rguPhoneTextInput', function () {
        return {
            restrict: 'EA',
            templateUrl: '/Areas/regitUI/templates/phone-text-input.html',
            scope: {
                model: '=ngModel',
                name: '@'
            },
            link: function (scope, elem, attrs) {
                scope.name = scope.name || 'phoneTextInput';
                scope.model = scope.model || '';
                scope.phone = scope.model.toString();
                // scope.fullNumber = '';

                if (scope.model && scope.model.charAt(0) === '+') {
                    scope.model = scope.model.substring(1);
                }
                scope.cc = '';
                scope.$watch('phone', function (newValue) {
                    if (newValue) {
                        // console.log('phone', newValue)
                        if (newValue.charAt(0) === '+') {
                            newValue = newValue.substring(1);
                        } else if (!scope.changedCC) {
                            // newValue = '65' + newValue;
                        }
                        scope.model = '+' + scope.cc + newValue;
                        scope.phoneValue = newValue;
                    }
                }, true);
                scope.changedCC = false;
                scope.$watch('cc', function (newValue) {
                    if (newValue) {
                        console.log('cc', newValue)
                        scope.changedCC = true;
                        // if (scope.fullNumber) {
                        //     scope.model = scope.fullNumber;
                        // }
                        scope.model = '+' + newValue + scope.phoneValue;
                        scope.changedCC = true;
                    }
                }, true);
                // scope.$watch('fullNumber', function (newValue) {
                //     if (newValue) {
                //         console.log('fullNumber', newValue)
                //         scope.model = newValue;
                //     }
                // }, true);

            }
        }
    })

    .directive('rguWorkTime', function () {

        return {
            restrict: 'EA',
            templateUrl: '/Areas/regitUI/templates/work-time.html',
            scope: {
                model: '='
            },
            link: function (scope, elem, attrs) {
                var weekdayNames = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
                scope.addRange = function () {
                    var from = new Date();
                    from.setHours(7);
                    from.setMinutes(0);
                    var to = new Date();
                    to.setHours(18);
                    to.setMinutes(0);
                    var weekdays = [];
                    angular.forEach(weekdayNames, function (name) {
                        weekdays.push({
                            name: name,
                            open: !scope.model.ranges.length && name !== 'Sat' && name !== 'Sun'
                        });
                    });
                    scope.model.ranges.push({
                        from: from,
                        to: to,
                        weekdays: weekdays
                    });
                };
                scope.model = scope.model || {};
                if (!scope.model.ranges) {
                    scope.model.ranges = [];
                    scope.addRange();
                }

                scope.removeRange = function (range, index) {
                    scope.model.ranges.splice(index, 1);
                };

                function closeDayOtherRanges(exceptRange, againstWeekday) {
                    angular.forEach(scope.model.ranges, function (range) {
                        if (range === exceptRange) {
                            return;
                        }
                        angular.forEach(range.weekdays, function (weekday) {
                            if (weekday.name === againstWeekday.name) {
                                weekday.open = false;
                                range.allDays = false;
                            }
                        });
                    });
                }

                scope.toggleDay = function (range, weekday) {
                    weekday.open = !weekday.open;
                    if (weekday.open) {
                        closeDayOtherRanges(range, weekday);
                    }
                };
                scope.toggleAllDays = function (range) {
                    range.allDays = !range.allDays;
                    angular.forEach(range.weekdays, function (weekday) {
                        weekday.open = range.allDays;
                        if (range.allDays) {
                            closeDayOtherRanges(range, weekday);
                        }
                    });
                };
            }
        }
    })

    .filter('profileWorkHours', function ($moment) {
        return function (profile) {

            var from = profile.WorkHourFrom, to = profile.WorkHourTo, days = profile.Workdays;
            if (!from || !to)
                return '';
            from = $moment(from), to = $moment(to);
            var html = from.format('hh:mm a') + ' - ' + to.format('hh:mm a');
            if (days && days.length) {
                html += '<br>' + days.join(', ')
            }
            return html;
        };
    })
    .filter('workTime', function ($moment) {
        return function (workTime) {
            if (!workTime) return '';
            if (workTime.open247) return 'Open 24/7';
            var html = '';
            angular.forEach(workTime.ranges, function (range) {
                angular.forEach(range.weekdays, function (weekday) {
                    if (weekday.open) {
                        html += weekday.name + ' ';
                    }
                });
                var from = $moment(range.from);
                var to = $moment(range.to);
                html += ': ' + from.format('hh:mm a') + ' - ' + to.format('hh:mm a');
                html += '<br>';
            });

            return html;
        }
    })

    .directive('interactionBadge', function (campaignTypeNames, campaignTypeAbbrs) {

        return {
            restrict: 'E',

            scope: {
                model: '<'
            },

            template: '<span class="interaction-badge interaction-type-{{model.Type.toLowerCase()}} '
            + 'interaction-status-{{model.Status.toLowerCase()}}">'
            + '{{abbr}}</span>',

            link: function link(scope, element, attrs) {
                scope.$watchCollection('model', function (newVal) {
                    //var type = scope.type, status = scope.status;
                    var c = newVal;

                    var type = c.Type, status = c.Status;
                    scope.type = type;
                    scope.status = status;

                    var typeIndex = $.inArray(type, campaignTypeNames);
                    if (typeIndex >= 0) {
                        scope.abbr = campaignTypeAbbrs[typeIndex];
                    }
                    //  $scope.abbr = campaignTypeAbbrs[typeIndex];
                    var statusIndexes = {
                        'active': 0,
                        'pending': 1,
                        'inactive': 2,
                        'expired': 3
                    };

                    scope.statusIndex = statusIndexes[status];
                });
            }

        };
    })
    .directive('handshakeStatusBadge', function () {

        return {
            restrict: 'E',

            scope: {
                model: '<'
            },

            template: '<div class="handshake-status-badge" bs-tooltip data-title="{{getHandshakeStatusDesc()}}">' +
            '<div class="handshake-status handshake-status-{{getHandshakeStatus()}}"'
            + '{{model.Status}}</div></div>',

            link: function link(scope, el, attrs) {
                scope.getHandshakeStatus = function () {
                    if (scope.model == null)
                        return 'none';
                    var status = scope.model.Status;

                    if (!scope.model.isaccecpt)
                        return 'pending';
                    if (!scope.model.IsJoin)
                        return 'paused';
                    if (!scope.model.IsChange)
                        return 'none';
                    if (status === 'acknowledged')
                        return 'ack';
                    if (status === 'Not acknowledged')
                        return 'nak';
                    return '';
                };
                scope.getHandshakeStatusDesc = function () {

                    if (scope.model == null)
                        return 'Sync inactive';
                    if (!scope.model.isaccecpt)
                        return 'Pending';
                    if (!scope.model.IsJoin)
                        return 'Sync paused';
                    var status = scope.model.Status;

                    if (!scope.model.IsChange)
                        return 'Sync inactive';
                    if (status === 'acknowledged')
                        return 'Business has acknowledged';
                    if (status === 'Not acknowledged')
                        return 'Business has not acknowledged';
                    return '';
                }
            }

        };
    })

    .directive('rguCarousel', function () {

        return {
            restrict: 'A',

            link: function (scope, el, attrs) {
                el.jcarousel({
                    items: '.carousel-slide'
                });

                $('.jcarousel-prev').jcarouselControl({
                    target: '-=1'
                });

                $('.jcarousel-next').jcarouselControl({
                    target: '+=1'
                });
            }
        };
    })

    .directive('privacyControl', function (rguView, $timeout) {
        var guid = 0;
        return {
            restrict: 'EA',
            templateUrl: '/Areas/regitUI/templates/privacy-control.html',
            scope: {
                model: '=ngModel',
                hidable: '@',
                ngChange: '&'
            },
            link: function (scope, elem, attrs) {
                scope.controlId = (++guid).toString();
                scope.view = {
                    openingSelector: false,
                    selectedPrivacy: scope.model
                };
                if (angular.isDefined(attrs.hidable)) {
                    scope.view.hidable = true;
                }
                scope.rguView = rguView;
                scope.openPrivacySelector = function ($event) {
                    $event.stopPropagation();
                    rguView.openingMoreActions = scope.controlId;
                };
                scope.closePrivacySelector = function () {
                    rguView.openingMoreActions = false;
                };
                scope.selectPrivacy = function (value) {
                    scope.model = value;
                    scope.view.selectedPrivacy = value;
                    scope.closePrivacySelector();
                    if (angular.isFunction(scope.ngChange)) {
                        $timeout(function () {
                            scope.ngChange();
                        });
                    }
                };

            }
        };
    })

    .directive('emergencyContactSelector', function ($timeout) {
        return {
            restrict: 'EA',
            templateUrl: '/Areas/regitUI/templates/emergency-contact-selector.html',
            scope: {
                model: '=ngModel',
                contacts: '<',
                ngChange: '&'
            },
            link: function (scope, elem, attrs) {
                scope.relationships = ['Father', 'Mother', 'Son', 'Daughter', 'Brother', 'Sister', 'Cousin', 'Husband', 'Wife', 'Uncle', 'Aunt', 'Niece',
                    'Nephew', 'Grandmother', 'Grandfather', 'Friend', 'Father-in-law', 'Mother-in-law', 'In-law', 'Other'];
                if (angular.isDefined(scope.contacts) && (scope.contacts === null || scope.contacts === 'preview')) {
                    scope.contacts = [
                        {
                            displayName: 'Contact 1',
                            priority: 1,
                            relationship: 'Relationship 1'
                        },
                        {
                            displayName: 'Contact 2',
                            priority: 2,
                            relationship: 'Relationship 2'
                        },
                        {
                            displayName: 'Contact 3',
                            priority: 3,
                            relationship: 'Relationship 3'
                        }
                    ];
                }
                // scope.contacts.push(
                //     {
                //         displayName: 'Create new...',
                //         priority: 1,
                //         relationship: ''
                //     }
                // );
                // scope.friends = [
                //     {displayName: 'Friend 1'},
                //     {displayName: 'Friend 2'},
                //     {displayName: 'Friend 3'}
                // ];
                scope.creatingContact = false;
                scope.selectedContact = null;
                if (scope.contacts && scope.contacts.length) {
                    scope.selectedContact = scope.contacts[0];
                }

                scope.onChangeContact = function () {
                    if (scope.selectedContact && scope.selectedContact.displayName === 'Create new...') {
                        scope.createContact();
                    } else {
                        scope.creatingContact = false;
                    }
                };

                scope.createContact = function () {
                    scope.creatingContact = true;
                    scope.newContact = null;
                    scope.selectingFriend = false;
                };

                scope.onNewContact = function () {
                    scope.selectingFriend = true;

                };

            }
        };
    })

    .filter('properUrl', function () {
        return function (url) {
            if (!url) return '';
            if (url.charAt(0) == '/')
                return location.protocol + '//' + location.host + url;
            if (!/^https?:\/\//.test(url)) {

                return 'http://' + url;
            }

            return url;
        };
    })
    .filter('secureCCNumber', function () {
        var sCCNFilter = function (number) {
            if (!number) return '';
            if (number.length < 5) return number;
            return '********' + number.substr(number.length - 4);
        };
        sCCNFilter.$stateful = true;
        return sCCNFilter;
    })
    .filter('textToHtml', function () {
        return function (text) {
            if (!text) return '';
            //text = $('<div/>').html(text).text();
            return text.replace(new RegExp('\n', 'g'), '<br>');
        };
    })
    .filter('avatarUrl', function () {
        return function (url, business) {
            if (!url || url.indexOf('no-pic') !== -1) {
                return business ? '/Areas/Beta/img/business-profile-picture.svg' : '/Areas/Beta/img/profile-picture.svg';
            }
            return url;
        };
    })

    .filter('fieldToText', function ($moment, rgu) {
            var filterValue = function (field) {
                    var value = field.model;
                    var type = field.type;
                    if (angular.isUndefined(value) || value === null)
                        return ''; //<span class="vault-value-meta">(No value)</span>';
                    if (type === 'textbox')
                        return value;
                    if (type === 'range') {
                        return value === "-1" ? '' : value;
                    }

                if (type === 'doc') {
                    if (!angular.isArray(field.model) || !field.model.length) return '';
                    var docs = field.model;
                    var doc = docs[0];
                    var html = '<ol class="field-doc-list">';
                    angular.forEach(docs, function (doc) {
                        if (!doc.selected) return;
                        var fname = doc.fileName
                        html += '<li><a target="_blank" href="' + doc.filePath + '/' + fname + '">';
                        if (/\.(jpe?g|png|gif|bmp)$/i.test(fname)) {
                            html += '<img src="' + doc.filePath + '/' + fname + '">';
                        } else {
                            html += fname;
                        }
                        html += '</a></li>';
                    });
                    html += '</ol>';
/*                    var html = '<a class="static-doc-item" target="_blank" href="' + field.pathfile + fname + '">';
                    if (/\.(jpe?g|png|gif|bmp)$/i.test(fname)) {
                        html += '<img src="' + field.pathfile + fname + '">';
                    } else {
                        html += fname;
                    }
                    html += '</a>';*/
                    return html;
                }

                    if (angular.isArray(value))
                        return value.join(', ');

                    if (type === 'date' || type === 'datecombo') {
                        var parsedDate = rgu.parseDate(value);
                        if (parsedDate) {
                            // console.log(value);
                            var datestr = $moment(parsedDate.date).format('D MMMM YYYY');
                            if (datestr === 'Invalid date' || datestr === 'Invalid Date') {
                                if (field.options === 'indef') {
                                    return 'Indefinite';
                                }
                                datestr = '';

                            }
                            return datestr;
                        } else {
                            return '';
                        }
                    }
                    if (angular.isObject(value)) {
                        if (value.hasOwnProperty('country') && value.hasOwnProperty('city')) {
                            var country = value.country || '', city = value.city || '';
                            var text = city;
                            if (country && city) {
                                text += ', ';
                            }
                            text += country;
                            return text;
                        }
                        if (value.hasOwnProperty('address')) {
                            return value.address.hasOwnProperty('address') ? value.address.address : value.address;
                        }
                        // return '<span class="vault-value-meta">(Data)</span>'
                        return '***';

                    }

                    return value.toString();
                }
            ;
            filterValue.$stateful = true;
            return filterValue;
        })
    .directive('htmlFromText', function () {
        var ellipsis = '...';
        var truncateSize = 280;

        function truncateText(text) {
            if (text.length <= truncateSize) return text;
            text = text.substring(0, truncateSize);
            text = text.replace(/\w+$/, '');
            return text + ellipsis;
        }

        return {
            restrict: 'EAC',
            scope: {
                expandable: '@',
                model: '<ngModel'
            },
            templateUrl: '/Areas/regitUI/templates/html-text.html?v=3',
            link: function (scope, elem, attrs) {
                scope.view = {
                    expandable: !!scope.expandable,
                    expanded: false
                };
                var text = scope.model ? scope.model.toString() : '';
                if (text.length <= truncateSize) {
                    scope.view.expandable = false;
                }
                if (scope.view.expandable) {
                    scope.htmlBase = truncateText(text);
                    scope.htmlBase = scope.htmlBase.replace(new RegExp('\n', 'g'), '<br>');
                }
                scope.htmlFull = text.replace(new RegExp('\n', 'g'), '<br>');
                scope.setView = function (more) {
                    scope.view.expanded = more;
                };

            }
        };
    })

    .directive('countryInput', function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                angular.element(element).countrySelect({
                    responsiveDropdown: true,
                    preferredCountries: ['sg', 'us', 'vn']
                });
            }
        };
    })
    .directive('profileAvatarLink', function () {
        return {
            restrict: 'C',
            link: function (scope, element, attrs) {
                angular.element(element).fancyboxPlus();
            }
        };
    })
    .directive('rguSpinner', function () {
        return {
            restrict: 'E',
            // template: '<span class="rgu-spinner"></span>',
            template: '',
            link: function (scope, element, attrs) {
/*                var opts = {
                    lines: 7 // The number of lines to draw
                    , length: 3 // The length of each line
                    , width: 3 // The line thickness
                    , radius: 4 // The radius of the inner circle
                    , scale: 1 // Scales overall size of the spinner
                    , corners: 1 // Corner roundness (0..1)
                    , color: '#000' // #rgb or #rrggbb or array of colors
                    , opacity: 0.25 // Opacity of the lines
                    , rotate: 0 // The rotation offset
                    , direction: 1 // 1: clockwise, -1: counterclockwise
                    , speed: 1 // Rounds per second
                    , trail: 60 // Afterglow percentage
                    , fps: 20 // Frames per second when using setTimeout() as a fallback for CSS
                    , zIndex: 2e9 // The z-index (defaults to 2000000000)
                    , className: 'spinner' // The CSS class to assign to the spinner
                    , top: '50%' // Top position relative to parent
                    , left: '50%' // Left position relative to parent
                    , shadow: false // Whether to render a shadow
                    , hwaccel: false // Whether to use hardware acceleration
                    , position: 'relative' // Element positioning
                };  */
                var opts = {
                    lines: 20 // The number of lines to draw
                    , length: 4 // The length of each line
                    , width: 12 // The line thickness
                    , radius: 25 // The radius of the inner circle
                    , scale: 0.2 // Scales overall size of the spinner
                    , corners: 1 // Corner roundness (0..1)
                    , color: '#00adef' // #rgb or #rrggbb or array of colors
                    , opacity: 0 // Opacity of the lines
                    , rotate: 0 // The rotation offset
                    , direction: 1 // 1: clockwise, -1: counterclockwise
                    , speed: 1 // Rounds per second
                    , trail: 60 // Afterglow percentage
                    , className: 'spinner' // The CSS class to assign to the spinner

                };
                var spinner = new Spinner(opts).spin(element.get(0));
            }
        };
    })

    .filter('jsPathToHtml', function (rgu) {
        return function (jsPath) {
            if (!jsPath) return '';
            jsPath = rgu.deCamelize(jsPath);

            var pathParts = jsPath.split('.');
            pathParts = $.map(pathParts, function(part) {
                return rgu.toTitleCase(part);
            });
            // pathParts.splice(0, 1);
            return pathParts.join(' &raquo; ');
        };
    });






