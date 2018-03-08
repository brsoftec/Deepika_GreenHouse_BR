angular.module("regitMain")
    .controller("CalendarCtrl", function ($scope, $rootScope, $location, $moment, calendarService) {

        $scope.utcOffsetText = calendarService.utcOffsetText();

        $scope.dayHours = calendarService.getDayHours();

        $scope.activeDay = $rootScope.activeDay || $moment();

        $scope.events = calendarService.getAllEvents();

        $scope.showingEventPopup = false;

        $scope.updateDayEvents = function () {
            $scope.eventsByHour = [];
            for (var hour = 0; hour < 23; hour++) {
                $scope.eventsByHour[hour] = calendarService.getEventsByHour($scope.activeDay,hour);
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
                    day.eventsByHour[hour] = calendarService.getEventsByHour(day,hour);
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
            $location.path('/Calendar/Event/Edit/' + event.id);
        };

        $scope.newEvent = function() {
            $location.path('/Calendar/Event/New');
        };

        $scope.deleteEvent = function(id) {
            calendarService.deleteEvent(event.id);
            $scope.showingEventPopup = false;
        };


        if (!$rootScope.activeDay) {
            $scope.gotoView('today');
        }
        else {
            $scope.gotoView($scope.activeView || 'day');
        }

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

    });



