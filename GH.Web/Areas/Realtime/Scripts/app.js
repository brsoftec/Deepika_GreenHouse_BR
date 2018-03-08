angular.module('myApp', ['ngRoute', 'ui.bootstrap', 'UserModule', 'regit.ui', 'windows', 'msg',
    'ui.rCalendar', 'kendo.directives', 'ngTagsInput', 'highcharts-ng'])
    .controller('MainCtrl', function ($scope, $route, $routeParams, $location) {
        $scope.$route = $route;
        $scope.$location = $location;
        $scope.$routeParams = $routeParams;

        $scope.demoDate = new Date();
        $scope.dateFormat = 'mm-dd-yyyy';
        $scope.dateOptions = { showWeeks: false };
        $scope.datePicker = { opened: false };
        $scope.openDatePicker = function () {
            $scope.datePicker.opened = true;
        };

    })

    .controller('ModalCtrl', function ($scope, $uibModalInstance) {
        $scope.submit = function () {
            $uibModalInstance.close(0);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };
    });



