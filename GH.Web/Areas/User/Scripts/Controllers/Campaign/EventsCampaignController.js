
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'oitozero.ngSweetAlert', 'ui.select', 'ui.bootstrap', 'CommonDirectives', 'ngTagsInput', 'UserModule', 'SocialModule', 'NotificationModule'], true);
myApp.controller('EventsCampaignController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'CommonService',
function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, commonService) {

    $scope.initializeController = function () {

        $scope.alerts = [];
        $scope.messageBox = "";

        $scope.GetTargetNetWorkList();
        $scope.GetLocationTypeList();
        $scope.GetGenderList();
        $scope.GetContinentsList();
        $scope.GetBudgetUnitList();

        var id = commonService.GetQuerystring("campaignId");
        if (id != null)
            $scope.GetEventCapaignById(id);
        else
        {
            $scope.TargetNetworkSelected = $scope.TargetNetWorkList[0];
            $scope.LocationTypeSelected = $scope.LocationTypeList[0];
            $scope.ContinentSelected = $scope.ContinentsList[0];
            $scope.BudgetUnitSelected = $scope.BudgetUnitList[0];
            $scope.EventCampaign = $scope.InitEventCampaign();
        }

    }

    $scope.InitEventCampaign = function () {

        var eventCampaign = new Object();

        eventCampaign.Id = null;
        eventCampaign.BusinessId = 0;
        eventCampaign.CampaignName = "";

        eventCampaign.CampaignStatus = "";
        eventCampaign.DataType = "";
        eventCampaign.CampaignType = "";
        eventCampaign.Description = "";
        eventCampaign.Gender = "";

        eventCampaign.FromAge = 10;
        eventCampaign.ToAge = 100;

        eventCampaign.StartDate = new Date();
        eventCampaign.EndDate = new Date();

        eventCampaign.TargetNetwork = $scope.TargetNetworkSelected;
        eventCampaign.LocationType = $scope.LocationTypeSelected;
        eventCampaign.ContinentType = $scope.ContinentSelected;

        eventCampaign.CountryId = 0;
        eventCampaign.CountryName = "";
        eventCampaign.RegionId = 0;
        eventCampaign.RegionName = "";
        eventCampaign.CityId = 0;
        eventCampaign.CityName = "";

        eventCampaign.Budget = 0;
        eventCampaign.UnitBudget = $scope.BudgetUnitSelected;
        eventCampaign.FlashCost = 0;
        eventCampaign.IsFlash = new Date();

        eventCampaign.People = 0;
        eventCampaign.MaxPeople = 0;
        eventCampaign.DisplayOnBuzFeed = true;
        eventCampaign.AllowCreateQRCode = false;
        eventCampaign.Image = "";
        eventCampaign.UrlLink = "";


        return eventCampaign;

    };

    $scope.GetEventCapaignById = function (campaignId) {

        var data = new Object();
        data.CampaignId = campaignId;
        $http.post('/api/CampaignNewService/GetCampaignById', data)
            .success(function (response) {
                $scope.EventCampaign = response.CampaignObject;
            })
            .error(function (errors, status) {

            });

    }

    //$scope.GetAllCountry = function () {

    //    dataService.getAllCountry()
    //        .then(function (response) {
    //            $scope.userCountries = response;
    //        });
    //}

    //$scope.onCountrySelect = function (country) {
    //    dataService.getCityByCountry(country.Code)
    //        .then(function (response) {
    //            $scope.cities = response;
    //        });
    //}

    $scope.CampaignStatus = function (campaignStatus) {

        switch (campaignStatus) {
            case ("All"):
                return "All";
            case ("Pending"):
                return "Pending";
            case ("Actived"):
                return "Actived";
            case ("InActived"):
                return "InActived";
            case ("Approved"):
                return "Approved";
            case ("Denied"):
                return "Denied";
            case ("Expired"):
                return "Expired";
            default:
                return "All";
        }

    }

    $scope.GetBudgetUnitList = function () {
        var list = [{ Id: "USD", Name: "USD" },
                    { Id: "SGD", Name: "SGD" },
                    { Id: "VND", Name: "VND" }];
        $scope.BudgetUnitList = list;
    }

    $scope.GetTargetNetWorkList = function () {
        var list = [{ Id: "Public", Name: "Regit Network (Public)" },
                    { Id: "Private", Name: "Regit Customers (Private)" }];
        $scope.TargetNetWorkList = list;
    }

    $scope.GetLocationTypeList = function () {
        var list = [{ Id: "0", Name: "Global" },
                    { Id: "1", Name: "Continent" },
                    { Id: "2", Name: "Country/City" }];
        $scope.LocationTypeList = list;
    }

    $scope.GetContinentsList = function () {
        var list = [{ Id: "0", Name: "Asia" },
                    { Id: "1", Name: "Europe" },
                    { Id: "2", Name: "America" },
                    { Id: "3", Name: "Africa" }];
        $scope.ContinentsList = list;
    }

    $scope.GetGenderList = function () {
        // http://plnkr.co/edit/h5e5OgFCqv28MPy4tEaM?p=preview
        var list = [{ Id: "2", Name: "All" },
                    { Id: "1", Name: "Male" },
                    { Id: "0", Name: "Female" }];
        $scope.GendersList = list;
    }

    $scope.GetDataType = function (dataType) {

        switch (dataType) {
            case ("InUsed"):
                return "InUsed";
            case ("Draft"):
                return "Draft";
            case ("Template"):
                return "Template";
            default:
                return "";
        }

    }

    $scope.GetCampaignType = function (campaignType) {

        switch (campaignType) {
            case ("Registration"):
                return "Registration";
            case ("Advertising"):
                return "Ads";
            case ("Event"):
                return "Event";
            case ("Contest"):
                return "Contest";
            case ("Survey"):
                return "Survey";
            case ("Polls"):
                return "Polls";
            default:
                return "";
        }

    }

    

    $scope.OnSaveAsTemplate = function (campaign) {
        var dataType = "Template";
        if (campaign.Id != null)
            $scope.UpdateCampaign(campaign, dataType);
        else
            $scope.InsertCampaign(campaign, dataType);
        window.location.href = "/Campaign/CampaignList";
    }

    $scope.OnSaveAsDraft = function (campaign) {
        var dataType = "Draft";
        if (campaign.Id != null)
            $scope.UpdateCampaign(campaign, dataType);
        else
            $scope.InsertCampaign(campaign, dataType);
        window.location.href = "/Campaign/CampaignList";
    }

    $scope.OnSave = function (campaign) {
        var dataType = "InUsed";
        if (campaign.Id != null)
            $scope.UpdateCampaign(campaign, dataType);
        else
            $scope.InsertCampaign(campaign, dataType);

        window.location.href = "/Campaign/CampaignList";
    }

    $scope.UpdateCampaign = function (campaign, dataType) {

        var data = campaign;
        data.DataType = $scope.GetDataType(dataType);
        $http.post('/api/CampaignNewService/UpdateCampaign', data)
            .success(function (response) {

            })
            .error(function (errors, status) {

            });
    }

    $scope.InsertCampaign = function (campaign, dataType) {

        var data = campaign;
        data.CampaignType = $scope.GetCampaignType("Event");
        data.DataType = $scope.GetDataType(dataType);
        data.CampaignStatus = $scope.CampaignStatus("Pending");

        $http.post('/api/CampaignNewService/InsertCampaign', data)
            .success(function (response) {

            })
            .error(function (errors, status) {

            });
    }

    //$scope.tags = [
    //{ text: 'just' },
    //{ text: 'some' },
    //{ text: 'cool' },
    //{ text: 'tags' }
    //];
    //$scope.loadTags = function (query) {
    //    return $http.get('/tags?query=' + query);
    //};

    $scope.myDate = new Date();

    $scope.minDate = new Date(
        $scope.myDate.getFullYear(),
        $scope.myDate.getMonth(),
        $scope.myDate.getDate()
        );

    $scope.maxDate = new Date(
        $scope.myDate.getFullYear(),
        $scope.myDate.getMonth() + 2,
        $scope.myDate.getDate()
        );

}]);

