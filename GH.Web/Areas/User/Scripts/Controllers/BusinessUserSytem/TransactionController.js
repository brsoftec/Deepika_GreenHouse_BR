

var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.controller('TransactionController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService', '$uibModalInstance', 
function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, notificationService, $uibModalInstance) {

    //var vm = this;
    $scope.submit = function () {
        $uibModalInstance.close(0);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
}
]);

