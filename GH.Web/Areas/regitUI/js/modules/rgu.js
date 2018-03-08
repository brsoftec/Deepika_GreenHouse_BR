angular.module('rgu', ['angular-momentjs'])
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
    .factory('rguNotify', function () {
        return {
            add: function () {
            }
        };
    })
    .factory('rguModal', function () {
        return {
            open: function () {
            }
        };
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
                            users.push(user);
                            return deferred.resolve(user);
                        }).error(function (error) {
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
    .filter('friendlyTime', function ($moment, rgu) {
        var friendlyTimeFilter = function (date, options) {
            if (!date) return '';
            var parsedDate = rgu.parseDate(date);
            if (!parsedDate) return '';

            var moment = $moment.utc(date);//parsedDate.date);
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
    .filter('properUrl', function () {
        return function (url) {
            if (!url) return '';

            if (!/^https?:\/\//.test(url)) {

                return 'http://' + url;
            }

            return url;
        };
    })

;








