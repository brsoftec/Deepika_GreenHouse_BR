var myApp2 = angular.module("regitPublic");

myApp2.controller("VerifyEmailController",
['$scope', '$rootScope', '$timeout', 'SweetAlert', 'AuthorizationService',
    function ($scope, $rootScope, $timeout, _sweetAlert, _authService) {
    $scope.onSuccess = function(email) {
        window.ga('send', 'event', 'Signup', 'Email verified, signup completed', email);
               $timeout(function () {
                    window.location.href = '/User/SignIn';
                }, 3000);

};
    $scope.resendEmailVerify = function (email) {
        _authService.SendVerifyEmail(email)
            .then(function () {
                __common.swal(_sweetAlert, $rootScope.translate('success'), $rootScope.translate('New_Verification_Email_Sent'), 'success');
            },
                function (errors) {
                    __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                });
    };
    $scope.onFailure = function(email) {
        window.ga('send', 'event', 'Signup', 'Email not verified', email);
};


                
    }
]);