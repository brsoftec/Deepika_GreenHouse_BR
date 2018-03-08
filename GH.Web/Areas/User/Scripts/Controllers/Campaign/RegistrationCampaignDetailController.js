
var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule', 'ang-drag-drop'], true);
myApp.getController('RegistrationCampaignDetailController', [
    '$scope', '$rootScope', '$uibModal', '$http', 'DashboardService', '$sce', '$q', 'SweetAlert', 'NotificationService', 'CampaignService', 'CommonService','alertService',
    function ($scope, $rootScope, $uibModal, $http, dashboardService, $sce, $q, sweetAlert, notificationService, CampaignService, CommonService, alertService) {

        $scope.initializeController = function () {
            $scope.title = "Home Page";
            $scope.TermsConditions = false;
            $scope.listcampaigns = [];
            $scope.existUserInBusiness = [];
            $scope.campaign = new Object();
            var fullcampign = new Object();
            var data = new Object();
            var dataresult;
            
            fullcampign._id = CommonService.GetQuerystring("campaignid");
            if (fullcampign._id !== "") {
                data.CampaignId = fullcampign._id;

                $http.post("/api/CampaignService/GetActiveCampaignbyCampaignId", data)
                .success(function (response) {
                    $scope.campaign = response.NewFeedsItemsList[0];
                        var fields = response.ListOfFields;
                        var campaignFields = response.NewFeedsItemsList[0].Fields;

                    $.each(campaignFields, function (fieldIndex, campaignField) {
                        $.each(campaignField, function (index, field) {
                            if (field._name === 'optional') {
                                response.NewFeedsItemsList[fieldIndex].optional = field._value;
                            }
                        });
                    });

                    $scope.BusinessIdList = response.BusinessIdList;
                    $scope.BusinessCampaignIdList = response.BusinessCampaignIdList;
                    $scope.CampaignIdInPostList = response.CampaignIdInPostList;
                    //$scope.CurrentBusinessUserId = response.BusinessUserId;
                })
                .error(function (errors, status) {

                });

                //CampaignService.GetCampaignById(fullcampign._id).then(function (response) {
                //    fullcampign = response.Campaign;
                //    $scope.campaign = response.Campaign.campaign;
                //    //$scope.InitData();
                //}, function (errors) {

                //});
            }

        };

        $scope.isRegister = function (campaign) {
            return ($.inArray(campaign.CampaignId, $scope.CampaignIdInPostList) === -1);

        };

        $scope.OnDeregister = function (campaign, termsConditions) {
            if (termsConditions) {
            var data = new Object();
            data.BusinessUserId = campaign.BusinessUserId;
            data.CampaignId = campaign.CampaignId;
            data.CampaignType = campaign.CampaignType;
            data.campaign = campaign;

            var dataresult;

            $http.post("/api/CampaignService/DeRegisterCampaign", data)
                    .success(function (response) {
                        dataresult = response;
                        campaign.MembersOfBusinessNbr--;
                        $rootScope.$broadcast('reloadminicalendar');
                        $scope.CampaignIdInPostList = $.grep($scope.CampaignIdInPostList, function (id) {
                            return id !== campaign.CampaignId;

                        });
                    })
                    .error(function (errors, status) {

                    });
            }
            else {
                alertService.renderErrorMessage("You need Agree  Terms & Conditions");
                $scope.messageBox = alertService.returnFormattedMessage();
                $scope.alerts = alertService.returnAlerts();
            }

        };

        $scope.closeAlert = function (index) {
            alerts.splice(index, 1);
        };

        $scope.RegisterCampaign = function (campaign, termsConditions) {
            if (termsConditions) {
                var data = new Object();
                //data.UserId = $scope.DelegationSelected.FromAccountId;
                data.BusinessUserId = campaign.BusinessUserId;
                data.CampaignId = campaign.CampaignId;
                data.CampaignType = campaign.CampaignType;
                if (campaign.CampaignType == "Event")
                {
                    var event=new Object();
                    event.type = "Event Campaign";
                    event.name = campaign.Name;
                    event.description = campaign.Description;
                    event.detail=new Object();
                    event.detail.timetype = "More days";
                    event.detail.starttime = campaign.starttime;
                    event.detail.startdate = campaign.startdate;
                    event.detail.endtime = campaign.endtime;
                    event.detail.enddate = campaign.enddate;
                    event.detail.location = campaign.location;
                    event.detail.theme = campaign.theme;
                    event.UserSettings = new Object();
                    event.UserSettings.note = "";
                    event.UserSettings.syncgoogle = true;
                   
                    data.StrEvent = JSON.stringify(event);

                }


                $http.post('/api/CampaignService/RegisterCampaign', data)
                    .success(function(response) {
                        //$scope.campaign.isHidden = true;
                        //$scope.campaign.MembersOfBusinessNbr++;

                        //if ($scope.CampaignIdInPostList) $scope.CampaignIdInPostList.push($scope.CampaignId);
                        //$uibModalInstance.close(response);
                        $rootScope.$broadcast('reloadminicalendar');
                        if ($scope.CampaignIdInPostList)
                            $scope.CampaignIdInPostList.push(campaign.CampaignId);
                        $scope.BusinessIdList.push(campaign.BusinessUserId);
                    })
                    .error(function(errors, status) {

                    });
            }
            else {
                alertService.renderErrorMessage("You need Agree  Terms & Conditions");
                $scope.messageBox = alertService.returnFormattedMessage();
                $scope.alerts = alertService.returnAlerts();
            }
        }

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

myApp.controller('formregisterController',
    ['$scope', '$routeParams', '$uibModalInstance', 'campaign',
function ($scope, $routeParams, $uibModalInstance, campaign) {

    $scope.campaign = campaign;
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


