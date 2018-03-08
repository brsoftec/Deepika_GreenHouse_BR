
var myApp = getApp("myApp", ['ngRoute', 'TranslationModule', 'UserModule', 'SocialModule', 'CommonDirectives', 'oitozero.ngSweetAlert', 'ui.select', 'ui.bootstrap', 'BusinessAccountModule', 'NotificationModule', 'ngTouch', 'bootstrapLightbox', 'angular-carousel'], true);

myApp.filter("trustUrl", ['$sce', function ($sce) {
    return function (recordingUrl) {
        return $sce.trustAsResourceUrl(recordingUrl);
    };
}]);

myApp.getController('BusinessProfileController', ['$scope', '$rootScope', '$sce', 'BusinessAccountService', 'SweetAlert', 'UserManagementService', '$uibModal', '$http', function ($scope, $rootScope, $sce, _baService, _sweetAlert, UserManagementService, $uibModal, $http) {

    $scope.profile = {};
    $scope.srfis = [
    ];
    UserManagementService.GetCurrentUserProfile().then(function (profile) {
        if (profile) {
            $scope.currentUserProfile = profile;
        }

    });
    $scope.selectSRFI = function (srfi) {
        $scope.showingSRFISelect = false;
        $scope.openSRFIForm($scope.profile, srfi);
    };
    $scope.loadProfile = function (id) {
        $scope.BaID = id;
        _baService.GetPublicProfile(id).then(function (profile) {
            $rootScope.$broadcast('BUSINESS_ACCOUNT_PROFILE_LOADED', profile.Id)
            $scope.profile = profile;
            if ($scope.profile.ListCampaign.length > 0)
            {
                $scope.srfis = $scope.profile.ListCampaign;
            }
                           
            $scope.profile.Description = $sce.getTrustedHtml(__common.GetNewLineCharInHtml($scope.profile.Description));
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    }

    $scope.carousel = { Index: 0 };

    $scope.follow = function () {
        _baService.FollowBusiness({ Id: $scope.profile.Id }).then(function (followSummary) {
            $scope.profile.NumberOfFollowers = followSummary.NumberOfFollowers;
            $scope.profile.Followed = followSummary.Followed;
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    }

    $scope.unfollow = function () {
        _baService.UnfollowBusiness({ Id: $scope.profile.Id }).then(function (followSummary) {
            $scope.profile.NumberOfFollowers = followSummary.NumberOfFollowers;
            $scope.profile.Followed = followSummary.Followed;
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    }

    $scope.openSRFIForm = function (profile,srfi) {
        var data = new Object();
        data.BusinessUserId = profile.BusId;
        data.CampaignId = srfi.Id;
        data.CampaignType = "SRFI";

      
        var dataresult;

        $http.post("/api/CampaignService/GetUserInformationForCampaign", data)
                .success(function (response) {
                    dataresult = response;
                    var fields = response.ListOfFields;

                    $(response.ListOfFields).each(function (index) {
                        response.ListOfFields[index].membership = response.ListOfFields[index].membership == "true" ? true : false;
                        response.ListOfFields[index].selected = true;
                        switch (response.ListOfFields[index].type) {
                            case "date":
                            case "datecombo":
                                response.ListOfFields[index].model = new Date(response.ListOfFields[index].model);
                                break;
                            case "location":
                                var mode = {
                                    country: response.ListOfFields[index].model,
                                    city: response.ListOfFields[index].unitModel
                                }
                                response.ListOfFields[index].model = mode;
                                break;
                            case "address":
                                var mode = {
                                    address: response.ListOfFields[index].model,
                                    address2: response.ListOfFields[index].unitModel
                                }
                                response.ListOfFields[index].model = mode;
                                break;
                             case "doc":
                                        var m=[];
                                    $(response.ListOfFields[index].modelarrays).each(function (index, fname) {
                                       m.push({
                                            fname: fname
                                        });

                                    })
                                     response.ListOfFields[index].model = m;
                                    break;
                        }
                    });

                    // START
                    // re-order form fields by vault group
                    var myfields = [];
                    var vaultgroup = {
                        "data": [
                                    { "id": "0", "name": ".basicInformation" },
                                    { "id": "1", "name": ".contact" },
                                    { "id": "2", "name": ".address" },
                                    { "id": "3", "name": ".financial" },
                                    { "id": "4", "name": ".governmentID" },
                                    { "id": "5", "name": ".family" },
                                    { "id": "6", "name": ".membership" },
                                    { "id": "7", "name": ".employment" },
                                    { "id": "8", "name": ".education" },
                                    { "id": "9", "name": ".others" },
                                    { "id": "10", "name": "Custom" },
                                    { "id": "11", "name": "undefined" }
                            ]
                    };

                    var data = vaultgroup.data;

                    for (var i in data) {
                        var name = data[i].name;
                        $(response.ListOfFields).each(function (index) {
                            if (response.ListOfFields[index].jsPath.startsWith(name) && name != '') {
                                myfields.push(response.ListOfFields[index]);
                            }
                        });
                    }

                    response.ListOfFields = myfields;
                    // END 
                    // re-order form fields by vault group

                    var modalInstance = $uibModal.open({
                        animation: $scope.animationsEnabled,
                        templateUrl: 'modal-feed-open-reg.html',
                        controller: 'RegistrationOnProfileController',
                        size: "",
                        backdrop: 'static',

                        resolve: {
                            registerPopup: function () {
                                return {
                                    ListOfFields: response.ListOfFields
                                    , BusinessUserId: response.BusinessUserId
                                    , CampaignId: response.CampaignId
                                    , CampaignType: response.CampaignType
                                    ,campaign :srfi
                                    
                                    //,currentBusinessId: businessId
                                    //,currentCampaignType: campaignType
                                };
                            }
                        }
                    });

                    modalInstance.result.then(
                        function (campaign) {
                            if (!$scope.profile.Followed) {
                                $scope.profile.Followed = true
                                $scope.profile.NumberOfFollowers = $scope.profile.NumberOfFollowers + 1;

                            }
                        },
                        function () {

                        });
                })
                .error(function (errors, status) {
                    $scope.listcampaigns = response.NewFeedsItemsList;
                    $scope.CampaignUserId = response.UserId;
                    $scope.CurrentBusinessUserId = response.BusinessUserId;

                });

    };

    $scope.IsShowProfile = function () {
        if ($scope.profile.BusinessPrivacies) {
            if ($scope.profile.BusinessPrivacies.Privacy == 0) {
                return true;
            }
        }
        if ($scope.currentUserProfile) {
            if ($scope.BaID == $scope.currentUserProfile.Id) {
                return true;
            }
        }
        return false;
    };
}]);