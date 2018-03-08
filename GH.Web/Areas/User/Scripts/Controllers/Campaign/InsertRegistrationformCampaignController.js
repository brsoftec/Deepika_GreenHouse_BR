

var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.getController('InsertRegistrationformCampaignController',
['$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'InformationVaultService','NotificationService',
function ($rootScope, $http, userManager, sweetAlert, authService, alertService, informationVaultService, notificationService) {


    $scope.InitData = function () {
        $scope.Vault = new Object();
         $scope.formFields = [];
        $scope.GetVaultTreeForRegistration();
        if (CampaignService.CampaignAdvertising != null)
          $scope.formFields = CampaignService.CampaignAdvertising.fields;
    }

    $scope.GetVaultTreeForRegistration = function () {
        var CampaignModelView = new Object();


        ajaxService.ajaxPost(CampaignModelView, applicationConfiguration.urlwebapi + "api/CampaignService/GetVaultTreeForRegistration", $scope.GetVaultTreeForRegistrationSuccess,
            $scope.GetVaultTreeForRegistrationError);

    }
    $scope.GetVaultTreeForRegistrationSuccess = function (response) {
        $scope.Vault = response.TreeVault.vaultTreeForRegistration;
    }
    $scope.GetVaultTreeForRegistrationError = function (response) {
        
    }
    $scope.NexttoPreview = function () {
        if (CampaignService.CampaignAdvertising == null)
            CampaignService.CampaignAdvertising = new Object();

        CampaignService.CampaignAdvertising.fields = $scope.formFields;
        $location.path("Campaign/InsertRegistrationCampaignPreview");

    }
    $scope.Back = function ()
    {
        $location.path("Campaign/InsertRegistrationCampaign");
    }
    $scope.successFunction = function () {
        CampaignService.CampaignAdvertising = null;
        $location.path("Campaign/ManagerCampaign");
    }

    $scope.failFunction = function () {
        alertService.renderSuccessMessage(response.ReturnMessage);
        $scope.messageBox = alertService.returnFormattedMessage();
        $scope.alerts = alertService.returnAlerts();
    }
    $scope.SaveAsDraft = function () {
        if (CampaignService.CampaignAdvertising == null)
            CampaignService.CampaignAdvertising = new Object();
        CampaignService.CampaignAdvertising.fields = $scope.formFields;
        CampaignService.CampaignAdvertising = "Draft";
        CampaignService.InsertCampaignRegistration(CampaignService.CampaignAdvertising, ajaxService, applicationConfiguration, $scope.successFunction, $scope.failFunction)
    }
    $scope.InitData();
    

    $scope.addText = "";

    $scope.deleteField = function (index, array) {
        if (array === $scope.formFields) {
            array.splice(index, 1);
        }
    };

    $scope.onDrop = function ($event, $data, array) {
        if (array === $scope.formFields) {
            array.push($data);
        }
    };

}]);
