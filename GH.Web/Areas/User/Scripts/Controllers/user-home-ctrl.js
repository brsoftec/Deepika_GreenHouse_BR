var myApp = getApp("myApp", ['TranslationModule', 'SocialModule', 'angularMoment', 'CommonDirectives', 'UserModule', 'oitozero.ngSweetAlert', 'NotificationModule', 'ngRoute', 'ngTouch', 'bootstrapLightbox', 'ui.bootstrap'], true);

myApp.filter("trustUrl", ['$sce', function ($sce) {
    return function (recordingUrl) {
        return $sce.trustAsResourceUrl(recordingUrl);
    };
}]);

myApp.config(['$routeProvider',
  function ($routeProvider) {
      $routeProvider.when('/home', {
          templateUrl: "home.html",
          controller: "homeController"
      }).when('/search', {
          templateUrl: "search.html",
          controller: "SearchResultController",
          reloadOnSearch: true
      }).otherwise({
          redirectTo: '/home'
      });
  }]);

myApp.config(['LightboxProvider', function (LightboxProvider) {
    LightboxProvider.fullScreenMode = true;
    LightboxProvider.getImageUrl = function (image) {
        return image.Url;
    };
    LightboxProvider.calculateModalDimensions = function (dimensions) {
        var width = Math.max(400, dimensions.imageDisplayWidth + 32);

        if (width >= dimensions.windowWidth - 20 || dimensions.windowWidth < 768) {
            width = 'auto';
        }

        return {
            'width': width,
            'height': 'auto'
        };
    };
}]);

myApp.run(['$templateCache', '$rootScope', 'Lightbox', function ($templateCache, $rootScope, Lightbox) {
    $templateCache.put('lightbox.html', '<div class="modal-body lightbox-modal" ng-swipe-left="Lightbox.nextImage()" ng-swipe-right="Lightbox.prevImage()"><div class="lightbox-image-container"><img lightbox-src="{{Lightbox.imageUrl}}"></div><div class="lightbox-buttons" ng-if="Lightbox.images.length > 1"><a ng-click="Lightbox.prevImage()" class="prev-button">Previous</a> image {{Lightbox.index + 1}}/{{Lightbox.images.length}} <a ng-click="Lightbox.nextImage()" class="next-button">Next</a></div></div>');

    $rootScope.openLightbox = function (images, index) {
        Lightbox.openModal(images, index);
    }
}]);

myApp.getController("homeController", [
    '$scope', '$rootScope', '$uibModal', '$http', 'DashboardService', '$sce', '$q', 'SweetAlert', 'NotificationService',
    function ($scope, $rootScope, $uibModal, $http, dashboardService, $sce, $q, sweetAlert, notificationService, Lightbox) {
        $scope.init = function () {
            $scope.posts = [];
            $scope.shareSocialPost = {};
            $scope.totalDelegate = 0;
            $scope.socialPost = {};
            $scope.comments = [];
            $scope.ListDelegatedTo = [];
            $scope.newComment = {};
            $scope.ActivePost = null;
            $scope.configs = {
                ShowAddCommentModal: false,
                ShowShareSocialPostModal: false
            };
            $scope.constant = {
                pullFeedUrl: "/API/Social/PullMyFeed",
                likePost: "/API/Social/LikePost",
                addCommentUrl: "/API/Social/AddComment",
                sharePost: "/API/Social/SharePost",
                start: 0,
                take: 20
            };

            $scope.pullDashboardFeed();
            $scope.getListDelegatedToCurrentUser();
        };

        //Delegate
        $scope.getListDelegatedToCurrentUser = function () {
            var delegationModelView = new Object();
            var i = 0;
            delegationModelView.Direction = "DelegationIn";
            $http.post('/api/DelegationManager/GetListDelegation', delegationModelView)
                .success(function (response) {
                    $(response.Listitems).each(function (index, delegate) {
                        if (delegate.Status === "Accepted") {
                            $scope.ListDelegatedTo.push(delegate);
                            i++;
                        }
                    });
                    $scope.totalDelegate = i;
                })
                .error(function (errors, status) {
                });
        };
        var isSearchPost = false;

        //#region Regit - Blue
        $scope.CampaignTypeIds = [];
        $scope.CampaignTypeIds["Advertising"] = "ad";
        $scope.CampaignTypeIds["Registration"] = "reg";

        $scope.initializeController = function () {
            $scope.title = "Home Page";
            var deferer = $q.defer();
            $scope.listcampaigns = [];
            $scope.existUserInBusiness = [];

            var data = new Object();
            data.campaignpublicid = $("#campaignpublicid").val();
            data.CampaignType = $("#campaignpublictype").val();
            // $http.post('/api/CampaignService/GetActiveCampaignForCurrentUser', data)
            //     .success(function (response) {
            //         $scope.listcampaigns = response.NewFeedsItemsList;
            //
            //         $scope.BusinessIdList = response.BusinessIdList;
            //         $scope.BusinessCampaignIdList = response.BusinessCampaignIdList;
            //         $scope.CampaignIdInPostList = response.CampaignIdInPostList;
            //     })
            //     .error(function (errors, status) {
            //
            //     });

        };

        $scope.title = "Home Page";
        var deferer = $q.defer();
        $scope.listcampaigns = [];
        $scope.existUserInBusiness = [];

        // var data = new Object();
        // data.campaignpublicid = ''; //$("#campaignpublicid").val();
        // data.CampaignType = 'all';// $("#campaignpublictype").val();
        // data.keyword = '';
        // $http.post('/api/CampaignService/GetActiveCampaignForCurrentUser', data)
        //     .success(function (response) {
        //         $scope.listcampaigns = response.NewFeedsItemsList;
        //
        //         $scope.BusinessIdList = response.BusinessIdList;
        //         $scope.BusinessCampaignIdList = response.BusinessCampaignIdList;
        //         $scope.CampaignIdInPostList = response.CampaignIdInPostList;
        //     })
        //     .error(function (errors, status) {
        //
        //     });

        $scope.gotoProfile = function (campaign) {
            window.location.href = "/BusinessAccount/Profile?id=" + campaign.BusinessUserobjectId;
        }
        $scope.ActionToRegistrationCampaignDetail = function (campaign) {
            var campaignId = campaign.CampaignId;
            window.location.href = "/Campaign/RegistrationCampaignDetail?campaignid=" + campaignId;
        }
        $scope.getInteractionAbbr=function(campaign)
        {
            switch(campaign.CampaignType)
            {
                case "Registration":
                    return "reg";
                case "Advertising":
                    return "ad";
                case "Event":
                    return "evt";

            }

            return "";
               
        }

        $scope.onAdClick = function (e, campaign) {
            //e.preventDefault();
            var data = new Object();
            data.BusinessUserId = campaign.BusinessUserId;
            data.CampaignId = campaign.CampaignId;
            data.CampaignType = campaign.CampaignType;

            $http.post("api/BusinessUserSystemService/AddBusinessMemberAdsCampaign", data)
               .success(function (response) {
               })
               .error(function (errors, status) {

                   $scope.listcampaigns = response.NewFeedsItemsList;
                
               });
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
                                       if (!response.ListOfFields[index].qa)
                                       {
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

                        if (data.CampaignType != "Handshake") {
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
                                        };
                                    }
                                }
                            });
                          
                            modalInstance.result.then(function (campaign) {
                                $scope.initializeController();
                            }, function () {
                          
                            });
                            
                        }
                        else {
                            var modalInstance1 = $uibModal.open({
                                animation: $scope.animationsEnabled,
                                templateUrl: 'modal-feed-open-reg-handshake.html',
                                controller: 'RegistrationHandshakeOnNewFeedController',
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
                                        };
                                    }
                                }
                            });

                            modalInstance1.result.then(function (campaign) { }, function () { });
                        }
                    })
                    .error(function (errors, status) {
                        $scope.listcampaigns = response.NewFeedsItemsList;
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
        };

        $scope.OnFollow = function (e, userId, businessId) {
            this.AddBusinessMember(userId, businessId);
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
            data.UserId = userId;
            data.BusinessUserId = businessUserId;
            var dataresult;

            $http.post("/api/BusinessUserSystemService/AddBusinessMember", data)
                .success(function (response) {
                    $scope.BusinessIdList.push(businessUserId);
                })
                .error(function (errors, status) {

                    $scope.listcampaigns = response.NewFeedsItemsList;
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
                    $scope.listcampaigns = response.NewFeedsItemsList;
                });
        };

        $rootScope.onSearchDone = function (posts, loadMore) {
            isSearchPost = true;
            if (!loadMore) {
                $scope.posts = [];
            }

            posts.forEach(function (post) {
                post.Message = $sce.trustAsHtml(post.Message.replace(/(?:\r\n|\r|\n)/g, '<br />'));
                $scope.posts.push(post);
            })
        }

        $scope.pullDashboardFeed = function () {
            var start = $scope.constant.start;
            var take = $scope.constant.take;
            dashboardService.PullDashboardFeed($scope.constant.pullFeedUrl, start, take).then(function (response) {
                if (response && response.length > 0) {
                    for (var i = 0; i < response.length; i++) {
                        response[i].Message = $sce.trustAsHtml(response[i].Message.replace(/(?:\r\n|\r|\n)/g, '<br />'));
                    }
                    $scope.posts = response;
                    $scope.constant.start += $scope.constant.take;
                }
            }, function (error) {
                var e = __errorHandler.ProcessErrors(error);
                __errorHandler.Swal(e, sweetAlert);
            });
        }

        $rootScope.addExternalFeedToArray = function (feed, isPostNew) {
            feed.Message = $sce.trustAsHtml(feed.Message.replace(/(?:\r\n|\r|\n)/g, '<br />'));
            if (isPostNew || !$scope.posts.findItem('SocialId', feed.SocialId)) {
                $scope.posts.unshift(feed);
            }
        };
        $scope.fileSelected = function () {
            $scope.NewPost.Photos.push({});
            $scope.$apply();
        };
        $scope.sharePostToSocialNetwork = function (post) {
            $scope.shareSocialPost = post;
            $scope.configs.ShowShareSocialPostModal = true;
        }

        $scope.onShareSocialPostModalHidden = function () {
            $scope.shareSocialPost.GreenHouse = false;
            $scope.shareSocialPost.Facebook = false;
            $scope.shareSocialPost.Twitter = false;
            $scope.shareSocialPost = {};
        }

        $scope.saveShareSocialPost = function () {
            var typeOfShares = [];
            if ($scope.shareSocialPost.Facebook)
                typeOfShares.push('facebook');
            if ($scope.shareSocialPost.Twitter)
                typeOfShares.push('twitter');
            if ($scope.shareSocialPost.GreenHouse)
                typeOfShares.push('greenhouse');
            if (typeOfShares.length <= 0) {
                sweetAlert.warning($rootScope.translate('Please_choose_at_least_one_social_network'));
                return false;
            }
            dashboardService.SharePost($scope.constant.sharePost, $scope.shareSocialPost.Id, typeOfShares, $scope.shareSocialPost.Description).then(function (response) {
                if (response != null || response != []) {
                    if ($rootScope.addExternalFeedToArray && response.SharePost) {
                        $rootScope.addExternalFeedToArray(response.SharePost, true);
                    }

                    var diaglogmessage = '<div class="post-status-container">';
                    angular.forEach(response.postStatus, function (value, key) {
                        if (value.sucess) {
                            diaglogmessage = diaglogmessage + '<div class="post-status-item ' + value.sucess + '" aria-hidden="true"><i class="fa fa-check-circle fa-2x"></i><span class="post-status-mesage">' + value.message + '</span></div>';
                        } else {
                            diaglogmessage = diaglogmessage + '<div class="post-status-item ' + value.sucess + '" aria-hidden="true"><i class="fa fa-ban fa-2x"></i><span class="post-status-mesage">' + value.message + '</span></div>';
                        }
                    });
                    diaglogmessage = diaglogmessage + '</div>';

                    sweetAlert.swal({ title: "Post Detail", text: diaglogmessage, html: true, closeOnConfirm: false });
                }
                if (response.SharePost) {
                    var currentSocial = $scope.posts.findItem('Id', $scope.shareSocialPost.Id);
                    currentSocial.TotalShares = response.TotalShares;
                    $scope.configs.ShowShareSocialPostModal = false;
                }
            }, function (error) {
                var e = __errorHandler.ProcessErrors(error);
                __errorHandler.Swal(e, sweetAlert);
                $scope.configs.ShowShareSocialPostModal = false;
            });
        }

        $scope.addCommentToSocialPost = function (socialPost) {
            $scope.ActivePost = socialPost;
            dashboardService.GetCommentByPost(socialPost.Id).then(function (res) {
                $scope.comments = res;
                if ($scope.comments) {
                    $scope.comments.forEach(function (comment) {
                        if (typeof comment.MessageHtml == 'undefined') {
                            comment.MessageHtml = $sce.trustAsHtml(comment.Message.replace(/(?:\r\n|\r|\n)/g, '<br />'));
                        }
                    });
                }
                $scope.socialPost = socialPost;
                $scope.configs.ShowAddCommentModal = true;
            })
        }

        $scope.isAllowComment = function () {
            if (!$scope.ActivePost) {
                return false;
            }
            if (!$scope.ActivePost.Account.BusinessPrivacies) {
                return true;
            }

            if ($rootScope.BAProfile && ($rootScope.BAProfile.Id == $scope.ActivePost.Account.Id)) {
                return true;
            }

            if ($scope.ActivePost.Account.BusinessPrivacies.AllowComment) {
                return true;
            }
            return false;


        }

        $scope.onAddCommentToSocialPostModalHidden = function () {
            $scope.socialPost = {};
            $scope.comments = [];
            $scope.newComment = {};
        }

        $scope.saveAddCommentToSocialPost = function () {
            if (!$scope.newComment.Message || $scope.newComment.Message.isEmpty()) {
                sweetAlert.warning($rootScope.translate('This_message_is_not_empty'));
                return false;
            }
            dashboardService.AddComment($scope.constant.addCommentUrl, $scope.socialPost.Id, $scope.newComment.Message).then(function (comment) {
                comment['MessageHtml'] = $sce.trustAsHtml(comment.Message.replace(/(?:\r\n|\r|\n)/g, '<br />'));
                $scope.comments.push(comment);
                $scope.socialPost.TotalComment++;
            }, function (error) {
                var e = __errorHandler.ProcessErrors(error);
                __errorHandler.Swal(e, _sweetAlert);
            });
            $scope.newComment = {};
        }

        $scope.likePost = function (id) {
            dashboardService.LikePost($scope.constant.likePost, id).then(
                function (response) {
                    var currentSocial = $scope.posts.findItem('Id', id);
                    currentSocial.IsLiked = response.IsLiked;
                    if (response.IsLiked)
                        currentSocial.TotalLike++;
                    else
                        currentSocial.TotalLike--;
                }, function (error) {
                    var e = __errorHandler.ProcessErrors(error);
                    __errorHandler.Swal(e, sweetAlert);
                });
        }

        $scope.pullMoreFeed = function () {
            if (isSearchPost) {
                $rootScope.loadMoreSearchPosts();
            } else {
                var start = $scope.constant.start;
                var take = $scope.constant.take;
                dashboardService.PullDashboardFeed($scope.constant.pullFeedUrl, start, take).then(function (response) {
                    if (response.length > 0) {
                        for (var i = 0; i < response.length; i++) {
                            response[i].Message = $sce.trustAsHtml(response[i].Message.replace(/(?:\r\n|\r|\n)/g, '<br />'));
                            $scope.posts.push(response[i]);
                        }
                        $scope.constant.start += $scope.constant.take;
                    }

                }, function (error) {
                    var e = __errorHandler.ProcessErrors(error);
                    __errorHandler.Swal(e, sweetAlert);
                });
            }
        }

        $scope.ShowProfile = function (account) {
            if (account.AccountType == 'Business') {
                window.location = '/BusinessAccount/Profile/' + account.Id;
            } else {
                window.location = '/user/profile/' + account.Id;
            }

        }

        $scope.showSocialIcon = function (code, post) {
            if (post.Types.indexOf(code) !== -1) {
                return true;
            }
            return false;
        }

        $scope.delete = function (id) {
            sweetAlert.swal({
                title: 'Warning',
                text: $rootScope.translate('Are_you_sure_remove_this_post'),
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#00aeef",
                confirmButtonText: "OK",
                closeOnConfirm: false
            },
                function (isConfirm) {
                    if (isConfirm) {
                        dashboardService.DeletePost(id, true)
                            .then(
                                function (response) {
                                    var post = $scope.posts.findItem('Id', id);
                                    if (post) {
                                        $scope.posts.remove(post);
                                        for (var i = 0; i < response.length; i++) {
                                            var item = response[i];
                                            $scope.posts.findItem('Id', item.Id).Shares--;
                                            $scope.posts.findItem('Id', item.Id).TotalShares--;
                                        }
                                    }
                                    __common.swal(sweetAlert, 'OK', '', 'success');
                                },
                                function (error) {
                                    var e = __errorHandler.ProcessErrors(error);
                                    __errorHandler.Swal(e, sweetAlert);
                                });
                    }
                });
        }
        $scope.init();

     
    }
]);

