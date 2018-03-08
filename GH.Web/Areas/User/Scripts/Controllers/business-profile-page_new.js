var myApp = getApp("myApp", ['ngRoute', 'TranslationModule', 'UserModule', 'SocialModule', 'CommonDirectives', 'oitozero.ngSweetAlert', 'ui.select', 'ui.bootstrap', 'BusinessAccountModule', 'NotificationModule', 'ngTouch', 'bootstrapLightbox', 'angular-carousel'], true);

myApp.filter("trustUrl", ['$sce', function ($sce) {
    return function (recordingUrl) {
        return $sce.trustAsResourceUrl(recordingUrl);
    };
}]);

myApp.getController('BusinessProfileControllerNew', ['$scope', '$rootScope', '$sce', 'rguModal', 'BusinessAccountService', 'SweetAlert', 'UserManagementService', '$uibModal', '$http', 'formService','interactionFormService', 'AuthorizationService', 'fileUpload', function ($scope, $rootScope, $sce, rguModal,_baService, _sweetAlert, UserManagementService, $uibModal, $http, formService, interactionFormService, _authService, fileUpload) {
    $scope.privacy = {
        'PhotoUrl': 'public',
        'PictureAlbum': 'public',
        'Address': 'public',
        'Phone': 'public',
        'Email': 'public',
        'Website': 'public',
        'WorkTime': 'public',
        'Profile': 'public'
    };
    $scope.profile = {};
    $scope.profiles = { viewedProfile: {} };
    $scope.srfis = [];
    //1.get srfis
    $scope.loadSrfi = function (businesId) {
        $http.get('/api/interactions/get/srfis?userid=' + businesId)
          .success(function (response) {
              $scope.srfis = response;
             
          }, function (errors) {
              __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
          })

    }
    $scope.companyDetails = {};
    $scope.originalAlbum = [];
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
        _baService.GetPublicProfileFull(id).then(function (profile) {
            $scope.profile = profile;
            var businessId = profile.BusId;
            $scope.loadSrfi(businessId);
            _baService.GetCompanyObjectDetailsById(businessId).then(function (response) {
                $scope.companyDetails = response;
            });
            _baService.GetPictureAlbum(id).then(function (album) {
                $scope.originalAlbum = album;
                $scope.slides = $.map(album, function (url, index) {
                    return {
                        id: index,
                        url: url
                    };
                })
            });

            // Privacy
            _authService.GetPrivacy(businessId).then(function (rs) {
                    if (rs && angular.isArray(rs.ListField)) {
                        for (var i = 0; i < rs.ListField.length; i++) {

                            if (rs.ListField[i].Field == 'PhotoUrl')
                                $scope.privacy.PhotoUrl = rs.ListField[i].Role;
                            else if (rs.ListField[i].Field == 'PictureAlbum')
                                $scope.privacy.PictureAlbum = rs.ListField[i].Role;
                            else if (rs.ListField[i].Field == 'Address')
                                $scope.privacy.Address = rs.ListField[i].Role;
                            else if (rs.ListField[i].Field == 'Phone')
                                $scope.privacy.Phone = rs.ListField[i].Role;
                            else if (rs.ListField[i].Field == 'Email')
                                $scope.privacy.Email = rs.ListField[i].Role;
                            else if (rs.ListField[i].Field == 'Website')
                                $scope.privacy.Website = rs.ListField[i].Role

                            else if (rs.ListField[i].Field == 'WorkTime')
                                $scope.privacy.WorkTime = rs.ListField[i].Role;
                            else if (rs.ListField[i].Field == 'Profile')
                                $scope.privacy.Profile = rs.ListField[i].Role;
                        }
                    }


                },
                function (errors) {
                });

            $scope.profile.Description = $sce.getTrustedHtml(__common.GetNewLineCharInHtml($scope.profile.Description));

        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    }
  
    $scope.carousel = {Index: 0};

    $scope.follow = function () {
        _baService.FollowBusiness({Id: $scope.profile.Id}).then(function (followSummary) {
            $scope.profile.NumberOfFollowers = followSummary.NumberOfFollowers;
            $scope.profile.Followed = followSummary.Followed;
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    }

    $scope.unfollow = function () {
        _baService.UnfollowBusiness({Id: $scope.profile.Id}).then(function (followSummary) {
            $scope.profile.NumberOfFollowers = followSummary.NumberOfFollowers;
            $scope.profile.Followed = followSummary.Followed;
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })
    };

    $scope.openSRFIForm = function (profile, srfi) {

        var interaction = $scope.interaction = {
            id: srfi.Id,
            type: 'srfi',
            business: {
                accountId: profile.BusId,
                name: profile.DisplayName,
                avatar: profile.Avatar
            },
            name: srfi.campaign.name,
            description: srfi.campaign.description,
            verb: srfi.campaign.verb,
            fields: srfi.campaign.fields
        };
        formService.openInteractionForm(interaction, $scope);
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
                            //var m = [];
                            //$(response.ListOfFields[index].modelarrays).each(function (index, fname) {
                            //    m.push({
                            //        fname: fname
                            //    });

                            //})
                            //response.ListOfFields[index].model = m;
                            //break;
                            var m = [];
                            if (!response.ListOfFields[index].qa) {
                                $(response.ListOfFields[index].modelarrays).each(function (index, fname) {
                                    m.push({
                                        fname: fname
                                    });

                                })
                            }

                            response.ListOfFields[index].model = m;
                            break;
                    }
                });

                // START
                // re-order form fields by vault group
                var myfields = [];
                var vaultgroup = {
                    "data": [
                        {"id": "0", "name": ".basicInformation"},
                        {"id": "1", "name": ".contact"},
                        {"id": "2", "name": ".address"},
                        {"id": "3", "name": ".financial"},
                        {"id": "4", "name": ".governmentID"},
                        {"id": "5", "name": ".family"},
                        {"id": "6", "name": ".membership"},
                        {"id": "7", "name": ".employment"},
                        {"id": "8", "name": ".education"},
                        {"id": "9", "name": ".others"},
                        {"id": "10", "name": "Custom"},
                        {"id": "11", "name": "undefined"}
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

                modalInstance.result.then(function (campaign) {
                }, function () {
                });
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
        this.RemoveBusinessFromUser(userId, businessId);
        if ($scope.profile.Followed) {
            $scope.profile.Followed = false;
            $scope.profile.NumberOfFollowers = $scope.profile.NumberOfFollowers - 1;

        }
    };

    $scope.OnFollow = function (e, userId, businessId) {
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

    $scope.AddBusinessMember = function (userId, businessUserId) {
        var data = new Object();
        data.UserId = userId;
        data.BusinessUserId = businessUserId;
        var dataresult;

        $http.post("/api/BusinessUserSystemService/AddBusinessMember", data)
            .success(function (response) {
                $scope.BusinessIdList.push(businessUserId);

            })
            .error(function (errors, status) {

            });
    };

    //Description: UnFollow with Business User
    $scope.RemoveBusinessFromUser = function (userId, businessUserId) {
        var data = new Object();
        data.UserId = userId;
        data.BusinessUserId = businessUserId;
        var dataresult;

        $http.post("/api/BusinessUserSystemService/RemoveBusinessFromUser", data)
            .success(function (response) {

                $scope.BusinessIdList = $.grep($scope.BusinessIdList, function (id) {
                    return id !== businessUserId;
                });
            })
            .error(function (errors, status) {
            
            });
    };

    //#endregion InputHandler



}]);