

var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.getController('CustomerManagementController',
['$scope','$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService', 'NotificationService', '$uibModal', 
function ($scope,$rootScope, $http, userManager, sweetAlert, authService, alertService, informationVaultService, notificationService, $uibModal) {


    $scope.ActionToCampaignList = function () {
        //window.location.href = "/BusinessUserSystem/AnalyticsCampaign?CampaignIdValue=" + campaignId;
        window.location.href = "/Campaign/CampaignList";
    }

}]);

