
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule', 'ang-drag-drop','TranslationModule'], true);
myApp.getController('RegistrationCampaignPublishController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService', 'CountryCityService', 'fileUpload', 'dateFilter', 'CommonService', '$uibModal','$document',
function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, notificationService, countryCityService, fileUpload, dateFilter, CommonService, $uibModal,$document) {
    $scope.openReg = function () {
        window.location.href = "/User/SignIn?ReturnUrl=%2F";
    }
    $scope.AllowCreateQrCode = $("#allowqrcode").val();
    $scope.PublicURL = $("#publilink").val();

}]);

myApp.controller('formFieldEditCtrl', 
function ($scope, $uibModalInstance, field) {
    $scope.field = field;
    $scope.ok = function () {
        $uibModalInstance.close(field);
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
});

myApp.controller('RegisformregisterController',
    ['$scope', '$routeParams', '$uibModalInstance', 'campaign',
function ($scope, $routeParams, $uibModalInstance, campaign) {

    $scope.campaign = campaign;


    $scope.filterRequired = function (field) {
        return !field.optional && !field.membership;
    };
    $scope.filterOptional = function (field) {
        return !!field.optional;
    };
    $scope.filterMembership = function (field) {
        return !!field.membership;
    };
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
}]);

myApp.getController('datetimecontroller',
    ['$scope', '$routeParams',
function ($scope, $routeParams) {
    $scope.dateformat = 'yyyy-MM-dd';
    $scope.datetime = "";
    $scope.openeddatetime = false;
    $scope.opendatetime = function () {
        $scope.openeddatetime = true;
    };


}]);


