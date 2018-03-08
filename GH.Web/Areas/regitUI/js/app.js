angular.module('regitApp', ['ngRoute', 'ngMessages', 'ngSanitize', 'ui.bootstrap', 'ui.select', 'mgcrea.ngStrap',
    'angular-momentjs', 'ang-drag-drop', 'CommonDirectives', 'users', 'regit.ui', 'calendar', 'location', 'windows', 'msg', 'vault', 'campaigns'])
   .controller('MainCtrl', function ($scope, $route, $routeParams, $location) {
        $scope.$route = $route;
        $scope.$location = $location;
        $scope.$routeParams = $routeParams;

        $scope.languages = [{Name: 'English'}, {Name: 'Vietnamese'}];
        $scope.language = $scope.languages[0];
        $scope.currentUserProfile = {AccountType: 'Personal'};

        $scope.demoDate = new Date();
        $scope.dateFormat = 'mm-dd-yyyy';
        $scope.dateOptions = {showWeeks: false};
        $scope.datePicker = {opened: false};
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
    // angular.bootstrap(document.querySelector('.msg-app'), ['msgApp']);