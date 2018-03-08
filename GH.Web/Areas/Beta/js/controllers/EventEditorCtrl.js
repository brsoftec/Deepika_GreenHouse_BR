angular.module("regitMain")
    .controller("EventEditorCtrl", function ($scope, $location, $routeParams, calendarService) {
        $scope.coords = {latitude:0,longitude:0};
        var event;
        var action = 'new';
        switch (action) {
            case 'new':
                event = {
                    id: 0,
                    duration: 'One time'
                };
                break;
            case 'edit':
                event = calendarService.getEventById($routeParams.eventId);
                $scope.coords = event.coords || {latitude:0,longitude:0};
                break;
        }

        $scope.event = event;

        $scope.closeEvent = function () {
            $location.path('/Calendar');
        };
        $scope.saveEvent = function () {
            if ($scope.locationDirty) {
                $scope.event.coords = $scope.coords;

            }
            calendarService.saveEvent($scope.event);
            $scope.closeEvent();
        };


    });



