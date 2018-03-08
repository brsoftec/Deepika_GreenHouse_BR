
var myApp = getApp("myApp", ['ngRoute', 'TranslationModule', 'UserModule', 'SocialModule', 'CommonDirectives', 'oitozero.ngSweetAlert', 'ui.select', 'ui.bootstrap', 'BusinessAccountModule', 'NotificationModule', 'ngTouch', 'bootstrapLightbox', 'angular-carousel'], true);

myApp.filter("trustUrl", ['$sce', function ($sce) {
    return function (recordingUrl) {
        return $sce.trustAsResourceUrl(recordingUrl);
    };
}]);


myApp.getController('PushVaultBusinessController', ['$scope', '$rootScope', '$sce', 'BusinessAccountService', 'SweetAlert', 'UserManagementService', '$uibModal', '$http', function ($scope, $rootScope, $sce, _baService, _sweetAlert, UserManagementService, $uibModal, $http) {

    $scope.profile = {};
    $scope.pushtovaults = [];
    
    $scope.selectPushToVault = function (pushtovault) {
        $scope.showingPushToVaultSelect = false;
        $scope.openPushToVaultForm($scope.profile, pushtovault);
    };
    $scope.loadPushVault = function () {
        
        $http.get('/Api/BusinessAccount/GetPushVaultBusiness')
            .success(function (rs) {
                $scope.profile.Id = rs.Id;
                $scope.profile.DisplayName = rs.DisplayName;
                $scope.pushtovaults = rs.ListPushToVault;
            }).error(function (errors, status) {
             __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
            })
    }

      $scope.loadPushVault();
    //
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

    $scope.openPushToVaultForm = function (profile,srfi) {
        var data = new Object();
        data.BusinessUserId = profile.Id;
        data.CampaignId = srfi.CampaignId;
        data.CampaignType = "PushToVault";
      
        var dataresult;

        $http.post("/api/CampaignService/GetUserInformationForCampaignEmpty", data)
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

                    var modalInstance = $uibModal.open({
                        animation: $scope.animationsEnabled,
                        templateUrl: 'modal-feed-open-reg-pushtovault.html',
                        controller: 'RegistrationOnPushToVaultBusToUserController',
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

    $scope.$on('reloadfollowstatus', function () {
        if (!$scope.profile.Followed) {
            $scope.profile.Followed = true
            $scope.profile.NumberOfFollowers = $scope.profile.NumberOfFollowers + 1;

        }
    });
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
    ///

    $scope.getInteractionAbbr = function (campaign) {
        switch (campaign.CampaignType) {
            case "Registration":
                return "reg";
            case "Advertising":
                return "ad";
            case "Event":
                return "evt";

        }

        return "";

    }

    $scope.openReg = function (campaign) {
        var data = new Object();
        data.BusinessUserId = campaign.BusinessUserId;
        data.CampaignId = campaign.CampaignId;
        data.CampaignType = campaign.CampaignType;
        data.campaign = campaign;

        var dataresult;

        $http.post("/api/CampaignService/GetUserInformationForCampaign", data)
                .success(function (response) {
                    dataresult = response;
                    var fields = response.ListOfFields, campaignFields = response.Campaign.Fields;

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
                                var m = [];
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
                        "data":[
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
                            { "id": "10", "name": "undefined" }
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
                        controller: 'RegistrationOnNewFeedController',
                        size: "",
                        backdrop: 'static',

                        resolve: {
                            registerPopup: function () {
                                return {
                                    ListOfFields: response.ListOfFields
                                    , BusinessUserId: response.BusinessUserId
                                    , CampaignId: response.CampaignId
                                    , CampaignType: response.CampaignType
                                    , campaign: campaign
                                    , BusinessIdList: $scope.BusinessIdList
                                    , CampaignIdInPostList: $scope.CampaignIdInPostList
                                    //,currentBusinessId: businessId
                                    //,currentCampaignType: campaignType
                                };
                            }
                        }
                    });

                    modalInstance.result.then(function (campaign) { }, function () { });
                })
                .error(function (errors, status) {
                    //$scope.listcampaigns = response.NewFeedsItemsList;
                    $scope.CampaignUserId = response.UserId;
                    $scope.CurrentBusinessUserId = response.BusinessUserId;

                });

    };

    $scope.OnDeregister = function (campaign) {
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

    };

    $scope.isRegister = function (campaign) {
        return ($.inArray(campaign.CampaignId, $scope.CampaignIdInPostList) === -1);

        //return !campaign.isHidden && ($.inArray(campaign.BusinessUserId, $scope.BusinessCampaignIdList) === -1 ||
        //          $.inArray(campaign.CampaignId, $scope.CampaignIdInPostList) === -1);

    };

    $scope.isTargeted = function (campaign) {

        if (campaign.TargetNetwork === "Public")
            return true;

        return $scope.isShowCampaign(campaign);

    };

    $scope.isShowCampaign = function (campaign) {

        return ($.inArray(campaign.BusinessUserId, $scope.BusinessIdList) !== -1);

    };

    $scope.isFollowing = function (campaign) {

        return ($.inArray(campaign.BusinessUserId, $scope.BusinessIdList) !== -1);
    };

    $scope.OnFollowing = function (e, userId, businessId) {
        //console.log(businessId);
        //e.preventDefault();
        this.RemoveBusinessFromUser(userId, businessId);
        if ($scope.profile.Followed) {
            $scope.profile.Followed = false;
            $scope.profile.NumberOfFollowers = $scope.profile.NumberOfFollowers - 1;

        }
    };

    $scope.OnFollow = function (e, userId, businessId) {
        //console.log(businessId);
        //e.preventDefault();
        this.AddBusinessMember(userId, businessId);
        if (!$scope.profile.Followed) {
            $scope.profile.Followed = true;
            $scope.profile.NumberOfFollowers = $scope.profile.NumberOfFollowers + 1;

        }
    };

    $scope.OnHoverFolow = function (e) {
        //console.log(e);
        $(e.target).text('Unfollow').addClass('btn-unfollow');

    };

    $scope.OnMouseOutFolow = function (e) {
        $(e.target).text('Following').removeClass('btn-unfollow');
    };

    //Description: Follow with Business User
    $scope.AddBusinessMember = function (userId, businessUserId) {
        var data = new Object();
        //var deferer = $q.defer();
        data.UserId = userId;
        data.BusinessUserId = businessUserId;
        //data.CampaignId = campaignId;
        //data.CampainType = campainType;
        var dataresult;

        $http.post("/api/BusinessUserSystemService/AddBusinessMember", data)
            .success(function (response) {
                $scope.BusinessIdList.push(businessUserId);
                //$scope.BusinessCampaignIdList.push(businessUserId);

            })
            .error(function (errors, status) {

              //  $scope.listcampaigns = response.NewFeedsItemsList;
            });
    };

    //Description: UnFollow with Business User
    $scope.RemoveBusinessFromUser = function (userId, businessUserId) {
        var data = new Object();
        //var deferer = $q.defer();
        data.UserId = userId;
        data.BusinessUserId = businessUserId;
        //data.CampaignId = campaignId;
        var dataresult;

        $http.post("/api/BusinessUserSystemService/RemoveBusinessFromUser", data)
            .success(function (response) {

                $scope.BusinessIdList = $.grep($scope.BusinessIdList, function (id) {
                    return id !== businessUserId;
                });

                //$scope.BusinessCampaignIdList = $.grep($scope.BusinessCampaignIdList, function (id) {
                //    return id !== businessUserId;
                //});
                //$scope.BusinessCampaignIdList = $.grep($scope.CampaignIdInPostList, function (id) {
                //    return id !== businessUserId;
                //});
                //$scope.$apply(function() {
                //    $.grep($scope.BusinessIdList, function (id) {
                //        return id === businessUserId;
                //    });
                //});

            })
            .error(function (errors, status) {

               // $scope.listcampaigns = response.NewFeedsItemsList;
                //$scope.CampaignUserId = response.UserId;
                //$scope.CurrentBusinessUserId = response.BusinessUserId;
                //__promiseHandler.Error(errors, status, deferer);
            });
    };

    //#endregion InputHandler

   //  $scope.loadPushVault();

}]);

