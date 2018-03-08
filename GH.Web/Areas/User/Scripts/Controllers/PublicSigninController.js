//var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'TranslationModule', 'UserModule', 'oitozero.ngSweetAlert', 'CommonDirectives'], true);
// if (typeof(regitGlobal !== 'undefined') && regitGlobal.hasOwnProperty('ngApp')) {
//     var regitPublic = angular.module(regitGlobal.ngApp);
// } else {
//     var regitPublic = angular.module('regitPublic');
// }
//var regitPublic = angular.module(regitGlobal.ngApp);
var regitPublic = angular.module('regitPublic');
regitPublic.run(['$rootScope', '$cookies', 'AuthorizationService', function ($rootScope, $cookies, _authService) {
    $rootScope.authorized = false;

    $rootScope.authorized = _authService.IsAuthorized();
}]);

regitPublic.controller("PublicSigninController", ['$scope', '$rootScope', '$http', 'AuthorizationService', '$cookies', function ($scope, $rootScope, $http, _authService, $cookies) {
    $scope.providers = [];
    $scope.provider = {};

    $scope.signModel = {Email: null, Password: null};

    $scope.view = {
        openingSignupPicker: false
    };

    $scope.showMessage = function (msg) {
        $('.public-signin-message-text').html(msg);
    };

    $scope.signIn = function () {

        if (!$scope.signModel.Email) {
            $scope.showMessage('Please enter email and password');
            return;
        }
        var re = /^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;

        if (!re.test($scope.signModel.Email)) {
            $scope.showMessage('Invalid email');
            return;
        }
        if (!$scope.signModel.Password || $scope.signModel.Password.trim().length < 6) {
            $scope.showMessage('Invalid email or password');
            return;
        }
        $scope.showMessage('Signing in...');
        _authService.SignInLocal($scope.signModel.Email, $scope.signModel.Password)
            .then(function () {

                    var hideIntro = window.localStorage.getItem('hideIntro');

                    if (hideIntro && hideIntro !== 'permanent') {
                        window.localStorage.removeItem('hideIntro');
                    }
                    $rootScope.authorized = _authService.IsAuthorized();
                      window.location = "/";
                },
                function (errors) {
                        console.log(errors)
                    if (errors.error_description == 'EMAIL_NOT_VERIFIED') {
                        window.location.href = '/User/VerifyingEmail?email=' + $scope.signModel.Email;
                    } else if (errors.error_description == 'DISABLE_USER_BY_MAIL') {
                        window.location.href = '/User/DisableUserByEmail?email=' + $scope.signModel.Email;
                    } else {
                        $scope.showMessage(errors.error_description);
                    }
                });

        return true;
    }


}]);

