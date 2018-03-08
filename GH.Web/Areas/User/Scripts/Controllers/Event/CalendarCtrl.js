
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.getController("CalendarCtrl", function ($scope, $rootScope, $moment, calendarService, ConfirmDialogService) {

        $scope.utcOffsetText = calendarService.utcOffsetText();

        $scope.dayHours = calendarService.getDayHours();
       
        $scope.activeDay = $rootScope.activeDay || calendarService.getDayfromquery();
        
        $scope.updateDayEvents = function () {
            $scope.eventsByHour = [];
            for (var hour = 0; hour < 23; hour++) {
                $scope.eventsByHour[hour] = calendarService.getEventsByHour($scope.activeDay,hour, $rootScope.events);
            }
        };

        $scope.updateWeekCalendar = function () {
            var weekStart = $moment($scope.activeDay);
            weekStart.startOf('week').isoWeekday();
            $scope.weekDays = [];
            for (var i = 0; i < 7; i++) {
                var day = $moment(weekStart);
                day.add(i, 'day');
                day.eventsByHour = [];
                for (var hour = 0; hour < 23; hour++) {
                    day.eventsByHour[hour] = calendarService.getEventsByHour(day,hour, $rootScope.events);
                }
                $scope.weekDays.push(day);
            }
        };

        $scope.gotoView = function (view) {
            $scope.activeView = view;
            if (view === 'today') {
                $scope.activeDay = $moment();
                $scope.updateDayEvents();

            } else if (view === 'day') {
                $scope.activeDay = $rootScope.activeDay || $moment();
                $scope.updateDayEvents();
            }
            else if (view === 'week') {
                $scope.updateWeekCalendar();
            }
        };

        $scope.gotoEvent = function(event) {
            window.location.href= '/EventCalendar/CreateEvent/?eventid=' + event.id;
        };

        $scope.newEvent = function () {
            var date = $scope.activeDay.date();
            var month = $scope.activeDay.month();
            var year = $scope.activeDay.year();
            window.location.href = '/EventCalendar/CreateEvent?year=' + year + "&month=" + month + "&date=" + date;
        };
        $scope.deleteEvent = function (eventid) {

            ConfirmDialogService.Open("Do you want to delete this event?", function () {
                calendarService.deleteEvent(eventid).then(function (response) {

                    $scope.reload();
                    $rootScope.$broadcast('reloadminicalendar');

                }, function (errors) {
                });
            });

            
        };

        $scope.$on('calendarChangedActiveDay', function() {
            $scope.activeDay = $rootScope.activeDay || $moment();
            if ($scope.activeView==='today') {
                $scope.activeView = 'day';
                $scope.updateDayEvents();
            } else if ($scope.activeView==='day') {
                $scope.updateDayEvents();
            }
            else if ($scope.activeView==='week') {
                $scope.updateWeekCalendar();
            }
        });

        $scope.reload = function () {
            calendarService.getAllEvents().then(function (events) {
                $rootScope.events = events;
                $scope.events = events;
                if (!$rootScope.activeDay) {
                    $scope.gotoView('today');
                }
                else {
                    $scope.gotoView($scope.activeView || 'day');
                }
            }, function (errors) {
            });
        }

        if ($rootScope.events == undefined || $rootScope.events == null) {
            $scope.reload();
        }
        else {
            $scope.events=$rootScope.events;
               if (!$rootScope.activeDay) {
                    $scope.gotoView('today');
                }
                else {
                    $scope.gotoView($scope.activeView || 'day');
                }
        }
    });



