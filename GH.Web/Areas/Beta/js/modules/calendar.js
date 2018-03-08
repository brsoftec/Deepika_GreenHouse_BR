angular.module('calendar', ['angular-momentjs'])

    .factory('calendarService', function ($moment) {
        var events = [];

        function generateSampleEvents() {
            for (var i = 1; i < 20; i++) {
                var month = $moment().month();
                var day = Math.floor(Math.random() * 30 + 1);
                var hour = Math.floor(Math.random() * 22 + 1);
                var minute = Math.floor(Math.random() * 4) * 15;
                var start = $moment([2016, month, day, hour, minute]);
                var end = $moment(start);
                end.add(Math.floor(Math.random() * 8) * 15, 'minute');

                var event = {
                    id: i,
                    source: 'User created',
                    summary: 'Event #' + i,
                    description: 'Description of Event #' + i,
                    location: 'Location of Event #' + i,
                    theme: 'Theme #' + i,
                    organizer: 'Personal',
                    note: 'User notes about event',
                    duration: 'One time',
                    start: {
                        date: start.toDate(),
                        dateTime: start.toDate()
                    },
                    end: {
                        date: end.toDate(),
                        dateTime: start.toDate()
                    }
                };
                events.push(event);
            }
        }

        generateSampleEvents();

        //  SERVICE FUNCTIONS   //
        return {
            utcOffsetText: function () {
                var offset = Math.round($moment().utcOffset() / 60);
                return 'GMT ' + (offset >= 0 ? '+' : '-') + offset;
            },
            isToday: function (day) {
                return $moment().isSame(day, 'day');
            },
            getDayHours: function () {
                var moment = $moment();
                var hours = [];
                for (var hour = 0; hour < 24; hour++) {
                    moment.hour(hour).minute(0);
                    hours.push(moment.format('hh:mm a'));
                }
                return hours;
            },

            //      EVENTS

            getAllEvents: function () {
                return events;
            },
            getEventsByDay: function (day) {
                day = $moment(day);
                var dayEvents = [];
                angular.forEach(events, function (event) {
                    if (day.isSame(event.start.date, 'day')) {
                        dayEvents.push(event);
                    }
                });
                return dayEvents;
            },
            getEventsByHour: function (day, hour) {
                var time = $moment(day).hour(hour);
                var hourEvents = [];
                angular.forEach(events, function (event) {
                    if (day.isSame(event.start.date, 'day') && time.isSame(event.start.date, 'hour')) {
                        hourEvents.push(event);
                    }
                });
                return hourEvents;
            },
            getEventById: function(id) {
                return events[id];
            },
            saveEvent: function(event) {
                //  Save to backend
            },
            deleteEvent: function(id) {
                //  Delete event
                events.slice(id,1);
            }
        };
    })
    .filter('fullDate', function ($moment) {
        return function (date) {
            return $moment(date).format('dddd, MMMM DD YYYY');
        }
    })
    .filter('fullTime', function ($moment) {
        return function (time) {
            return $moment(time).format('hh:mm a');
        }
    })
    .filter('shortTime', function ($moment) {
        return function (time) {
            return $moment(time).format('h:m');
        }
    })
    .filter('dayCell', function () {
        return function (day) {
            return day.date();
        }
    })
    .filter('monthHeader', function ($sce) {
        return function (month) {
            var html = '<span class="month-name">' + month.format('MMMM') + '</span> ' + month.format('YYYY');
            return $sce.trustAsHtml(html);
        }
    })
    .filter('weekDayHeader', function () {
        return function (moment) {
            return moment.format('ddd, DD MMM');
        }
    })
    .directive('miniCalendar', function ($rootScope, $location, $moment, calendarService) {
        return {
            restrict: 'EAC',
            templateUrl: RegitTemplatePath + '/mini-calendar.html',
            link: function (scope, elem, attrs) {
                scope.monthInitials = ['S', 'M', 'T', 'W', 'T', 'F', 'S'];
                scope.utcOffsetText = calendarService.utcOffsetText();
                scope.isToday = calendarService.isToday;
                scope.showingDayCellPopup = false;
                scope.$on('$routeChangeSuccess', function(e, current, pre) {
                    scope.inCalendar = $location.path()==='/Calendar';
                });

                scope.isActiveDay = function (day) {
                    return day.isSame(scope.activeDay, 'day');
                };
                scope.notInActiveMonth = function (moment) {
                    return !moment.isSame(scope.activeMonth, 'month');
                };
                scope.gotoDay = function (day) {
                    scope.activeDay = day;
                    $rootScope.activeDay = day;
                    $rootScope.$broadcast('calendarChangedActiveDay');
                    return false;
                };
                scope.gotoMonth = function (month) {
                    if (month === 'prev') {
                        scope.activeMonth.subtract(1, 'month');
                    }
                    else if (month === 'next') {
                        scope.activeMonth.add(1, 'month');
                    }
                    scope.updateMonthCalendar();
                };
                scope.gotoCalendar = function (day) {
                    $location.path('/Calendar');
                    scope.showingDayCellPopup = false;
                    return false;
                };
            },
            controller: function ($scope, calendarService) {
                $scope.activeDay = $moment();
                $scope.activeMonth = $moment();
                $scope.updateMonthCalendar = function () {
                    var monthStart = $moment($scope.activeMonth).startOf('month');
                    var monthEnd = $moment($scope.activeMonth).endOf('month');
                    var viewStart = monthStart;
                    viewStart.startOf('week').isoWeekday();
                    var viewEnd = monthEnd;
                    viewEnd.endOf('week').isoWeekday();
                    $scope.weeks = [];
                    var week = [];
                    for (var day = viewStart, count = 0; day.isSameOrBefore(viewEnd, 'day'); day.add(1, 'day')) {
                        var moment = $moment(day);
                        moment.events = calendarService.getEventsByDay(day);
                        moment.hasEvents = !!moment.events.length;
                        week.push(moment);
                        if (count % 7 == 6) {
                            $scope.weeks.push(week);
                            week = [];
                        }
                        count++;
                    }
                };
                $scope.updateMonthCalendar();

            }

        };
    });




