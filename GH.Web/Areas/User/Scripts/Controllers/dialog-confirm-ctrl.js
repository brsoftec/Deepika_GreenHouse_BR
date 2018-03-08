
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select','NotificationModule'], true);
myApp.controller('DialogConfirmController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert','AuthorizationService','alertService','$uibModalInstance','Content',
function ($scope, $rootScope, $http, userManager, sweetAlert,authService,alertService,$uibModalInstance,Content) {
    
        $scope.Message = Content.Message;
        $scope.OK = function () {
            $uibModalInstance.close();
        }

        $scope.Cancel = function () {
            $uibModalInstance.dismiss('cancel');
        }

    }]);



