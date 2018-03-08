

var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'oitozero.ngSweetAlert', 'CommonDirectives', 'ui.select', 'UserModule', 'SocialModule', 'NotificationModule'], true);
myApp.getController('CampaignListController',
['$scope', '$rootScope', '$http', 'SweetAlert', 'UserManagementService', 'AuthorizationService', 'alertService',
function ($scope, $rootScope, $http, sweetAlert, userManager, authService, alertService) {

    $scope.initializeController = function () {

        $scope.alerts = [];
        $scope.messageBox = "";
        $scope.CampaignList = [];
        $scope.BuzCampaignList = [];
        $scope.DraftCampaignList = [];
        $scope.TemplateCampaignList = [];
      
        $scope.GetCampaignTypeList();
        $scope.GetCampaignStatusList();
        $scope.CampaignTypeSelected = $scope.CampaignTypeList[0];
        $scope.CampaignStatusSelected = $scope.CampaignStatusList[0];
        $scope.GetCampaignList();
    }

    $scope.GetCampaignTypeList = function () {
        var list = ["All", "Registration", "Advertising", "Event", "Survey", "Polls", "Contest"];
        $scope.CampaignTypeList = list;

    }

    $scope.GetCampaignStatusList = function () {
        var list = ["All Status", "Active", "InActive", "Pending", "Expired"];
        $scope.CampaignStatusList = list;

    }

    $scope.GetClassByExpiredStatus = function (expiredStatus) {

        switch (expiredStatus) {
            case ("Expired"):
                return "campaigns-expires campaigns-expires-soon";
            default:
                return "campaigns-expires";
        }

    }


    $scope.GetClassByStatus = function (status) {

        switch (status) {
            case ("Expired"):
                return "campaigns-row-expired";
            case ("Pending"):
                return "campaigns-status campaigns-status-pending";
            case ("Inactive"):
                return "campaigns-status campaigns-status-inactive";
            case ("Active"):
                return "campaigns-status campaigns-status-active";
            default:
                return "";
        }
    }

    $scope.GetCampaignList = function () {
        var campaignList = [];
        var data = new Object();

        $http.post('/api/CampaignNewService/GetCampaignList', data)
            .success(function (response) {
                $scope.CampaignList = response.CampaignList;
                $scope.BuzCampaignList = response.BuzCampaignList;
                $scope.DraftCampaignList = response.DraftCampaignList;
                $scope.TemplateCampaignList = response.TemplateCampaignList;
            })
            .error(function (errors, status) {

            });
        return campaignList;
    }

    $scope.OnEdit = function (campaignItem) {
        //var campaignId = campaignItem.CampaignId;
        var campaignId = campaignItem.Id;
        window.location.href = "/Campaign/EventsCampaign?campaignId=" + campaignId;
    }

    $scope.OnBoost = function (campaignItem) {
    
    }

    $scope.OnDuplicate = function (campaignItem) {
        var data = new Object();
        data.Id = campaignItem.Id;
        $http.post('/api/CampaignNewService/CloneCampaign', data)
            .success(function (response) {
                $scope.GetCampaignList();
            })
            .error(function (errors, status) {
                $scope.GetCampaignList();
            });
       
    }


    $scope.OnDrawAnalyticChart = function (campaignItem) {
        var campaignId = "";
        var campaignName = "";
        window.location.href = "/BusinessUserSystem/AnalyticsCampaign?CampaignIdValue=" + campaignId + "&CampaignNameValue=" + campaignName;
    }


    $scope.OnChangeCampaignType = function(campaignType) {
        
    }

    $scope.OnChangeCampaignType = function (campaignType) {

    }

    $scope.closeAlert = function (index) {
        $scope.alerts.splice(index, 1);
    };

}]);

