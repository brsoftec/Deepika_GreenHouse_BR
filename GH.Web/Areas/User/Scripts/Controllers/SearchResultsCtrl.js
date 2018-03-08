var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule', 'ang-drag-drop'], true);

myApp.getController('UserSearchResultsCtrl',
    ['$scope', 'CommonService', '$rootScope', '$http', 'NetworkService', 'SweetAlert', '$interval', 'BusinessAccountService', '$uibModal',
        function ($scope, CommonService, $rootScope, $http, _networkService, _sweetAlert, $interval, _baService, $uibModal) {
            $scope.keyword = CommonService.GetQuerystring("query");
            $scope.avatarPath = '/Areas/Beta/img/avatars';
            var loadSize = 5;
            var loadSizebus = 5;
            var loadSizeInteractions = 5;

            $scope.initIndividuals = function () {
                var data = new Object();
                data.keyword = $scope.keyword;
                data.CurrentPageNumber = 1;
                data.PageSize = loadSize;
                $scope.searchResultsIndividuals = [];
                var array = [];
                $http.post('/api/SearchService/SearchMainUser', data)
                    .success(function (response) {

                        $(response.results).each(function (index, userresult) {
                            array.push({
                                id: userresult.Userid,
                                firstName: userresult.FirstName,
                                middleName: '',
                                lastName: userresult.LastName,
                                email: userresult.Email,
                                desc: userresult.Description,
                                avatar: userresult.PhotoUrl == "" ? "/Areas/User/Content/Images/no-pic.png" : userresult.PhotoUrl,
                                network: userresult.StatusFriend,
                                displayName: ((userresult.DisplayName == "" || userresult.DisplayName == null) ? (userresult.FirstName + " " + userresult.LastName) : userresult.DisplayName),
                                isMe: userresult.Userid === regitGlobal.activeAccountObjId
                            })
                        });
                        $scope.searchResultsIndividuals = array;
                    })
                    .error(function (errors, status) {
                    });
            }
            $scope.initBusinesses = function () {
                var data = new Object();
                data.keyword = $scope.keyword;
                data.CurrentPageNumber = 1;
                data.PageSize = loadSizebus;
                $scope.searchResultsBusinesses = [];
                var array = [];
                var ucbs = [];

                $http.post('/api/SearchService/SearchMainBus', data)
                    .success(function (response) {
                        $(response.results).each(function (index, userresult) {
                            array.push({
                                id: userresult.Userid,
                                displayName: ((userresult.DisplayName == "" || userresult.DisplayName == null) ? (userresult.FirstName + " " + userresult.LastName) : userresult.DisplayName),
                                email: userresult.Email,
                                desc: userresult.Description,
                                avatar: userresult.PhotoUrl,
                                following: userresult.StatusFriend == "Followed"
                            })
                        });

                        $scope.searchResultsBusinesses = array;

                        $http.get('/api/Ucb/Search', {
                            params: {
                                keyword: $scope.keyword,
                                by: 'all',
                                start: 0,
                                length: loadSizebus
                            }
                        })
                            .success(function (response) {
                                if (response && response.length) {
                                    ucbs = $.map(response, function(ucb) {
                                        return {
                                            id: ucb.id,
                                            displayName: ucb.name,
                                            avatar: ucb.avatar,
                                            desc: ucb.description,
                                            ucb: true
                                        }
                                    });
                                }
                                $scope.searchResultsBusinesses = array.concat(ucbs);
                            })
                            .error(function (errors) {
                                //console.log('Error searching UCBs:', errors);
                            });

                    })
                    .error(function (errors) {
                        console.log('Error searching businesses:', errors);
                    });
            };

            $scope.initCampaign = function () {
                $scope.listcampaigns = [];

                var data = new Object();
                data.campaignpublicid = "";
                data.CampaignType = "All";
                data.keyword = $scope.keyword;

                $http.post('/api/CampaignService/GetActiveCampaignForCurrentUser', data)
                    .success(function (response) {
                        $scope.listcampaigns = response.NewFeedsItemsList;
                        $scope.BusinessIdList = response.BusinessIdList;
                        $scope.BusinessCampaignIdList = response.BusinessCampaignIdList;
                        $scope.CampaignIdInPostList = response.CampaignIdInPostList;
                    })
                    .error(function (errors, status) {
                    });
            }


            $scope.view = {
                limitIndividuals: loadSize,
                limitBusinesses: loadSize,
                limitInteractions: loadSizeInteractions,
            }

            $scope.viewprofileuser = function (user) {
                window.location.href = "/User/Profile?id=" + user.id;
            }
            $scope.viewprofilebus = function (user) {
                if (user.ucb) {
                    window.location.href = "/Profile/UserCreated/" + user.id;
                } else {
                    window.location.href = "/BusinessAccount/Profile?id=" + user.id;
                }
            }
            $scope.goToBusinessProfile = function (campaign) {
                window.location.href = "/BusinessAccount/Profile?id=" + campaign.BusinessUserobjectId;
            }
            $scope.searchResultsInteractions = [];

            $scope.sortIndividuals = function (user) {
                if (user.network === 'trusted')
                    return 1;
                if (user.network === 'normal')
                    return 2;
                if (user.network === 'pending')
                    return 3;
                return 4;
            };
            $scope.sortBusinesses = function (business) {
                if (business.following)
                    return 1;
                return 2;
            };
            $scope.sortInteractions = function (interaction) {
                if (interaction.participated)
                    return 1;
                return 2;
            };
            $scope.loadMore = function (section) {
                switch (section) {
                    case 'individuals':
                        loadSize = loadSize + loadSize;
                        $scope.initIndividuals();
                        break;
                    case 'businesses':
                        loadSizebus = loadSizebus + loadSizebus;
                        $scope.initBusinesses();

                        break;
                    case 'interactions':
                        $scope.view.limitInteractions += loadSizeInteractions;
                        break;
                }
            }
            $scope.inviteToNetwork = function (user) {
                if (user.id) {
                    _networkService.InviteFriend({ReceiverId: user.id}).then(function () {
                        var message = $rootScope
                                .translate('A_Request_Has_Been_Sent_To_Message') + ' ' +
                            user.firstName + ' ' + user.lastName;
                        user.network = "pending";
                        __common.swal(_sweetAlert, $rootScope.translate('Ok_Title'), message, 'success');
                        $scope.invitation = null;
                    }, function (errors) {
                        __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                    })
                } else {
                    __common.swal(_sweetAlert, $rootScope.translate('Warning_Title'),
                        $rootScope.translate('Please_Choose_Friend_Message'),
                        'warning');
                }
            };

            $scope.followBusiness = function (business) {
                _baService.FollowBusiness({Id: business.id}).then(function (followSummary) {
                    business.following = true;
                }, function (errors) {
                    __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                })

            };
            $scope.unfollowBusiness = function (business) {
                _baService.UnfollowBusiness({Id: business.id}).then(function (followSummary) {
                    business.following = false;
                }, function (errors) {
                    __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                })

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
                                {"id": "10", "name": "undefined"}
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

                        var modalInstance = $uibModal.open({
                            animation: $scope.animationsEnabled,
                            templateUrl: 'modal-feed-open-reg.html',
                            controller: 'RegistrationOnNewFeedController',
                            size: "",

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
                                    };
                                }
                            }
                        });

                        modalInstance.result.then(function (campaign) {
                        }, function () {
                        });
                    })
                    .error(function (errors, status) {
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
            $scope.initIndividuals();
            $scope.initBusinesses();
            $scope.initCampaign();
        }]);

myApp.getController('BusSearchResultsCtrl',
    ['$scope', 'CommonService', '$rootScope', '$http', 'NetworkService', 'SweetAlert', '$interval', 'BusinessAccountService', '$uibModal',
        function ($scope, CommonService, $rootScope, $http, _networkService, _sweetAlert, $interval, _baService, $uibModal) {
            $scope.keyword = CommonService.GetQuerystring("query");
            $scope.avatarPath = '/Areas/Beta/img/avatars';
            var loadSize = 5;
            var loadSizebus = 5;
            var loadSizeInteractions = 5;
            $scope.initIndividuals = function () {
                var data = new Object();
                data.keyword = $scope.keyword;
                data.CurrentPageNumber = 1;
                data.PageSize = loadSize;
                $scope.searchResultsIndividuals = [];
                var array = [];
                $http.post('/api/SearchService/SearchMainUser', data)
                    .success(function (response) {
                        $(response.results).each(function (index, userresult) {
                            array.push({
                                displayName: ((userresult.DisplayName == "" || userresult.DisplayName == null) ? (userresult.FirstName + " " + userresult.LastName) : userresult.DisplayName),
                                id: userresult.Userid,
                                firstName: userresult.FirstName,
                                middleName: '',
                                lastName: userresult.LastName,
                                email: userresult.Email,
                                desc: userresult.Description,
                                avatar: userresult.PhotoUrl == "" ? "/Areas/User/Content/Images/no-pic.png" : userresult.PhotoUrl,
                                following: userresult.StatusFriend == "Followed"
                            })
                        });
                        $scope.searchResultsIndividuals = array;

                        // deferer.resolve(response);
                    })
                    .error(function (errors, status) {
                    });
            }
            $scope.initBusinesses = function () {
                var data = new Object();
                data.keyword = $scope.keyword;
                data.CurrentPageNumber = 1;
                data.PageSize = loadSizebus;
                $scope.searchResultsBusinesses = [];
                var array = [];
                $http.post('/api/SearchService/SearchMainBus', data)
                    .success(function (response) {
                        $(response.results).each(function (index, userresult) {
                            array.push({
                                id: userresult.Userid,
                                displayName: ((userresult.DisplayName == "" || userresult.DisplayName == null) ? (userresult.FirstName + " " + userresult.LastName) : userresult.DisplayName),
                                email: userresult.Email,
                                desc: userresult.Description,
                                avatar: userresult.PhotoUrl == "" ? "/Areas/User/Content/Images/no-pic.png" : userresult.PhotoUrl
                            })
                        });
                        $scope.searchResultsBusinesses = array;

                        // deferer.resolve(response);
                    })
                    .error(function (errors, status) {
                    });
            }

            $scope.initCampaign = function () {
                $scope.listcampaigns = [];

                var data = new Object();
                data.keyword = $scope.keyword;
                $http.post('api/CampaignService/GetCampaignListByBusinessId', data)
                    .success(function (response) {
                        $scope.listcampaigns = response.NewFeedsItemsList;
                    })
                    .error(function (errors, status) {
                    });
            }


            $scope.view = {
                limitIndividuals: loadSize,
                limitBusinesses: loadSize,
                limitInteractions: loadSizeInteractions,
            }

            $scope.viewprofileuser = function (user) {
                window.location.href = "/User/Profile?id=" + user.id;
            }
            $scope.viewprofilebus = function (user) {
                window.location.href = "/BusinessAccount/Profile?id=" + user.id;
            }
            $scope.goToBusinessProfile = function (campaign) {
                if (campaign.BusinessUserobjectId !== null) {
                    window.location.href = "/BusinessAccount/Profile?id=" + campaign.BusinessUserobjectId;
                }
            }
            $scope.searchResultsInteractions = [];

            $scope.sortIndividuals = function (user) {
                if (user.network === 'trusted')
                    return 1;
                if (user.network === 'normal')
                    return 2;
                if (user.network === 'pending')
                    return 3;
                return 4;
            };
            $scope.sortBusinesses = function (business) {
                if (business.following)
                    return 1;
                return 2;
            };
            $scope.sortInteractions = function (interaction) {
                if (interaction.participated)
                    return 1;
                return 2;
            };
            $scope.loadMore = function (section) {
                switch (section) {
                    case 'individuals':
                        loadSize = loadSize + loadSize;
                        $scope.initIndividuals();
                        break;
                    case 'businesses':
                        loadSizebus = loadSizebus + loadSizebus;
                        $scope.initBusinesses();

                        break;
                    case 'interactions':
                        $scope.view.limitInteractions += loadSizeInteractions;
                        break;
                }
            }
            $scope.inviteToNetwork = function (user) {
                if (user.id) {
                    _networkService.InviteFriend({ReceiverId: user.id}).then(function () {
                        var message = $rootScope
                                .translate('A_Request_Has_Been_Sent_To_Message') + ' ' +
                            user.firstName + ' ' + user.lastName;
                        user.network = "pending";
                        __common.swal(_sweetAlert, $rootScope.translate('Ok_Title'), message, 'success');
                        $scope.invitation = null;
                    }, function (errors) {
                        __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                    })
                } else {
                    __common.swal(_sweetAlert, $rootScope.translate('Warning_Title'),
                        $rootScope.translate('Please_Choose_Friend_Message'),
                        'warning');
                }
            };

            $scope.followBusiness = function (business) {
                _baService.FollowBusiness({Id: business.id}).then(function (followSummary) {
                    business.following = true;
                }, function (errors) {
                    __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                })

            };
            $scope.unfollowBusiness = function (business) {
                _baService.UnfollowBusiness({Id: business.id}).then(function (followSummary) {
                    business.following = false;
                }, function (errors) {
                    __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                })

            };

            //Campaign

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
                                {"id": "10", "name": "undefined"}
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

                        var modalInstance = $uibModal.open({
                            animation: $scope.animationsEnabled,
                            templateUrl: 'modal-feed-open-reg.html',
                            controller: 'RegistrationOnNewFeedController',
                            size: "",

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
                                    };
                                }
                            }
                        });

                        modalInstance.result.then(function (campaign) {
                        }, function () {
                        });
                    })
                    .error(function (errors, status) {
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
                    })
                    .error(function (errors, status) {
                    });
            };


            $scope.initIndividuals();
            $scope.initBusinesses();
            $scope.initCampaign();
        }]);


myApp.getController('PublicSearchResultsCtrl',
    ['$scope', 'CommonService', '$rootScope', '$http', 'NetworkService', 'SweetAlert', '$interval', 'BusinessAccountService', '$uibModal',
        function ($scope, CommonService, $rootScope, $http, _networkService, _sweetAlert, $interval, _baService, $uibModal) {
            $scope.keyword = CommonService.GetQuerystring("query");
            $scope.avatarPath = '/Areas/Beta/img/avatars';
            var loadSize = 5;
            var loadSizebus = 5;
            var loadSizeInteractions = 5;

            $scope.initIndividuals = function () {
                var data = new Object();
                data.keyword = $scope.keyword;
                data.CurrentPageNumber = 1;
                data.PageSize = loadSize;
                data.isbus = false;
                $scope.searchResultsIndividuals = [];
                var array = [];
                $http.post('/api/SearchService/SearchMainPublic', data)
                    .success(function (response) {
                        $(response.results).each(function (index, userresult) {
                            array.push({
                                id: userresult.Userid,
                                firstName: userresult.FirstName,
                                middleName: '',
                                lastName: userresult.LastName,
                                email: userresult.Email,
                                desc: userresult.Description,
                                avatar: userresult.PhotoUrl == "" ? "/Areas/User/Content/Images/no-pic.png" : userresult.PhotoUrl,
                                network: userresult.StatusFriend,
                                displayName: ((userresult.DisplayName == "" || userresult.DisplayName == null) ? (userresult.FirstName + " " + userresult.LastName) : userresult.DisplayName),
                            })
                        });
                        $scope.searchResultsIndividuals = array;
                    })
                    .error(function (errors, status) {
                    });
            }
            $scope.initBusinesses = function () {
                var data = new Object();
                data.keyword = $scope.keyword;
                data.CurrentPageNumber = 1;
                data.PageSize = loadSize;
                data.isbus = true;
                $scope.searchResultsBusinesses = [];
                var array = [];
                $http.post('/api/SearchService/SearchMainPublic', data)
                    .success(function (response) {
                        $(response.results).each(function (index, userresult) {
                            array.push({
                                id: userresult.Userid,
                                displayName: ((userresult.DisplayName == "" || userresult.DisplayName == null) ? (userresult.FirstName + " " + userresult.LastName) : userresult.DisplayName),
                                email: userresult.Email,
                                desc: userresult.Description,
                                avatar: userresult.PhotoUrl == "" ? "/Areas/User/Content/Images/no-pic.png" : userresult.PhotoUrl,
                                following: userresult.StatusFriend == "Followed"
                            })
                        });
                        $scope.searchResultsBusinesses = array;
                    })
                    .error(function (errors, status) {
                    });
            }

            $scope.initCampaign = function () {
                $scope.listcampaigns = [];

                var data = new Object();
                data.keyword = $scope.keyword;
                $http.post('/api/SearchService/GetCampaignListByBusinessId', data)
                    .success(function (response) {
                        $scope.listcampaigns = response.NewFeedsItemsList;
                    })
                    .error(function (errors, status) {
                    });
            }
            $scope.view = {
                limitIndividuals: loadSize,
                limitBusinesses: loadSize,
                limitInteractions: loadSizeInteractions,
            }

            $scope.viewprofileuser = function (user) {
                window.location.href = "/User/Profile?id=" + user.id;
            }
            $scope.viewprofilebus = function (user) {
                window.location.href = "/BusinessAccount/Profile?id=" + user.id;
            }
            $scope.goToBusinessProfile = function (campaign) {
                if (campaign.BusinessUserobjectId !== null) {
                    window.location.href = "/BusinessAccount/Profile?id=" + campaign.BusinessUserobjectId;
                }
            }
            $scope.searchResultsInteractions = [];

            $scope.sortIndividuals = function (user) {
                if (user.network === 'trusted')
                    return 1;
                if (user.network === 'normal')
                    return 2;
                if (user.network === 'pending')
                    return 3;
                return 4;
            };
            $scope.sortBusinesses = function (business) {
                if (business.following)
                    return 1;
                return 2;
            };
            $scope.sortInteractions = function (interaction) {
                if (interaction.participated)
                    return 1;
                return 2;
            };
            $scope.loadMore = function (section) {
                switch (section) {
                    case 'individuals':
                        loadSize = loadSize + loadSize;
                        $scope.initIndividuals();
                        break;
                    case 'businesses':
                        loadSizebus = loadSizebus + loadSizebus;
                        $scope.initBusinesses();

                        break;
                    case 'interactions':
                        $scope.view.limitInteractions += loadSizeInteractions;
                        break;
                }
            }
            $scope.inviteToNetwork = function (user) {
                if (user.id) {
                    _networkService.InviteFriend({ReceiverId: user.id}).then(function () {
                        var message = $rootScope
                                .translate('A_Request_Has_Been_Sent_To_Message') + ' ' +
                            user.firstName + ' ' + user.lastName;
                        user.network = "pending";
                        __common.swal(_sweetAlert, $rootScope.translate('Ok_Title'), message, 'success');
                        $scope.invitation = null;
                    }, function (errors) {
                        __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                    })
                } else {
                    __common.swal(_sweetAlert, $rootScope.translate('Warning_Title'),
                        $rootScope.translate('Please_Choose_Friend_Message'),
                        'warning');
                }
            };

            $scope.followBusiness = function (business) {
                _baService.FollowBusiness({Id: business.id}).then(function (followSummary) {
                    business.following = true;
                }, function (errors) {
                    __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                })

            };
            $scope.unfollowBusiness = function (business) {
                _baService.UnfollowBusiness({Id: business.id}).then(function (followSummary) {
                    business.following = false;
                }, function (errors) {
                    __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
                })

            };

            //Campaign

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
                                {"id": "10", "name": "undefined"}
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

            // $scope.initIndividuals();
            $scope.initBusinesses();
            $scope.initCampaign();
        }]);
