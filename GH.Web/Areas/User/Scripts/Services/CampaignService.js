var _userApp = getApp('UserModule');

_userApp.factory('CampaignService', [
    '$http', '$q', '$cookies', '$interval', '$timeout', function($http, $q, $cookies, $interval, $timeout) {


        return {
            CampaignAdvertising: null,

            InsertCampaignAdvertising: function(campaignAdvertising) {
                var deferer = $q.defer();
                var stringCampaignAdvertising = JSON.stringify(campaignAdvertising);
                var CampaignAdvertisingModelView = new Object();

                CampaignAdvertisingModelView.StrCampaignAdvertising = stringCampaignAdvertising;
                CampaignAdvertisingModelView.CampaignAdvertising = campaignAdvertising;
                //  CampaignAdvertisingModelView.UserId = applicationConfiguration.usercurrent.Id;
                // ajaxService.ajaxPost(CampaignAdvertisingModelView, applicationConfiguration.urlwebapi + "api/CampaignService/InsertCampaignAdvertising", successFunction, failFunction);

                $http.post('/api/CampaignService/InsertCampaignAdvertising', CampaignAdvertisingModelView)
                    .success(function(response) {
                        deferer.resolve(response);
                    })
                    .error(function(errors, status) {
                        __promiseHandler.Error(errors, status, deferer);
                    });
                return deferer.promise;
            },

            SaveCampaign: function(campaign) {
                var deferer = $q.defer();
                var stringcampaign = JSON.stringify(campaign);
                var campaignidModelView = new Object();
                campaignidModelView.StrCampaignAdvertising = stringcampaign;
                campaignidModelView.CampaignId = campaign._id;
                $http.post('/api/CampaignService/SaveCampaign', campaignidModelView)
                    .success(function(response) {
                        deferer.resolve(response);s
                    }).error(function(errors, status) {
                        __promiseHandler.Error(errors, status, deferer);
                    });

                return deferer.promise;
            },

            DeleteCampaign: function(campaignid) {
                var deferer = $q.defer();
                var campaignidModelView = new Object();
                campaignidModelView.CampaignId = campaignid;
                $http.post('/api/CampaignService/DeleteCampaign', campaignidModelView)
                    .success(function(response) {
                        deferer.resolve(response);
                    }).error(function(errors, status) {
                        __promiseHandler.Error(errors, status, deferer);
                    });

                return deferer.promise;
            },

            SetBoostAdvertising: function(campaignid) {
                var deferer = $q.defer();
                var campaignidModelView = new Object();
                campaignidModelView.CampaignId = campaignid;
                $http.post('/api/CampaignService/SetBoostAdvertising', campaignidModelView)
                    .success(function(response) {
                        deferer.resolve(response);
                    }).error(function(errors, status) {
                        __promiseHandler.Error(errors, status, deferer);
                    });

                return deferer.promise;
            },

            GetCampaignById: function(campaignid) {
                var deferer = $q.defer();
                var campaignidModelView = new Object();
                campaignidModelView.CampaignId = campaignid;
                $http.post('/api/CampaignService/GetCampaignById', campaignidModelView)
                    .success(function(response) {
                        deferer.resolve(response);
                    }).error(function(errors, status) {
                        __promiseHandler.Error(errors, status, deferer);
                    });
                return deferer.promise;
            }

        }
    }
]);
