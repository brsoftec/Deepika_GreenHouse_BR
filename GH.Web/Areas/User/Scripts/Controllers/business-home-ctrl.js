var myApp = getApp("myApp", ['ngRoute', 'TranslationModule', 'UserModule', 'SocialModule', 'CommonDirectives', 'oitozero.ngSweetAlert', 'ui.select', 'ui.bootstrap', 'BusinessAccountModule', 'NotificationModule', 'ngTouch', 'bootstrapLightbox'], true);

myApp.filter("trustUrl", ['$sce', function ($sce) {
    return function (recordingUrl) {
        return $sce.trustAsResourceUrl(recordingUrl);
    };
}]);

myApp.config(['$routeProvider',
  function ($routeProvider) {
      $routeProvider.when('/home', {
          templateUrl: "home.html",
          controller: "BAHomeController"
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


myApp.getController("BAHomeController", [
    '$scope', '$rootScope', '$http', 'DashboardService', '$sce', '$q', 'SweetAlert', function ($scope, $rootScope, $http, dashboardService, $sce, $q, _sweetAlert) {
        $scope.businessAccountId;
        $scope.init = function () {
            $scope.posts = [];
            $scope.shareSocialPost = {};
            $scope.socialPost = {};
            $scope.comments = [];
            $scope.newComment = {};
            $scope.configs = {
                ShowAddCommentModal: false,
                ShowShareSocialPostModal: false
            }
            $scope.constant = {
                pullFeedUrl: "/API/Social/GetBusinessFeed",
                likePost: "/API/Social/LikePost",
                addCommentUrl: "/API/Social/AddComment",
                sharePost: "API/Social/SharePost",
                start: 0,
                take: 20
            };
        }


        $scope.businessAccountId = regitGlobal.businessAccount.id;

        var isSearchPost = false;
        $scope.CampaignTypeIds = [];
        $scope.CampaignTypeIds["Advertising"]  = "ad";
        $scope.CampaignTypeIds["Registration"] = "reg"; 
        $scope.initializeController = function () {
            var deferer = $q.defer();
            $scope.listcampaigns = [];
            $scope.existUserInBusiness = [];

        };

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
        };

        $rootScope.onSearchDone = function (posts, loadMore) {
            isSearchPost = true;
            if (!loadMore) {
                $scope.posts = [];
            }

            posts.forEach(function(post) {
                post.Message = $sce.trustAsHtml(post.Message.replace(/(?:\r\n|\r|\n)/g, '<br />'));
                $scope.posts.push(post);
            });
        }

        $scope.pullDashboardFeed = function () {
            var start = $scope.constant.start;
            var take = $scope.constant.take;
            dashboardService.PullDashboardFeed($scope.constant.pullFeedUrl, start, take, $scope.businessAccountId).then(function (response) {
                if (response && response.length > 0) {
                    for (var i = 0; i < response.length; i++) {
                        response[i].Message = $sce.trustAsHtml(response[i].Message.replace(/(?:\r\n|\r|\n)/g, '<br />'));
                    }
                    $scope.posts = response;
                    $scope.constant.start += $scope.constant.take;
                }
            }, function (error) {
                var e = __errorHandler.ProcessErrors(error);
                __errorHandler.Swal(e, _sweetAlert);
            });
        }

        $rootScope.addExternalFeedToArray = function (feed, isPostNew) {
            feed.Message = $sce.trustAsHtml(feed.Message.replace(/(?:\r\n|\r|\n)/g, '<br />'));
            if (isPostNew || !$scope.posts.findItem('SocialId', feed.SocialId)) {
                $scope.posts.unshift(feed);
            }
        }

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
        $scope.getInteractionAbbr = function (campaign) {
            switch (campaign.CampaignType) {
                case "Registration":
                    return "reg";
                case "Advertising":
                    return "ad";
                case "Event":
                    return "evt";
                case "Handshake":
                    return "hs";

            }

            return "";

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
                _sweetAlert.warning($rootScope.translate('Please_choose_at_least_one_social_network'));
                return false;
            }
            dashboardService.SharePost($scope.constant.sharePost, $scope.shareSocialPost.Id, typeOfShares, $scope.shareSocialPost.Description).then(function (response) {
                var alertType = $rootScope.translate('success');

                var fbMess = '';
                if (response.ShareToFacebook === true) {
                    fbMess = '<div class="post-status-item true" aria-hidden="true"><i class="fa fa-check-circle fa-2x"></i><span class="post-status-mesage"><span class="share-to">Facebook</span>: share successfully</span></div>';
                    $scope.posts.unshift(response.FacebookPost);
                } else if (response.ShareToFacebook === false) {
                    fbMess = '<div class="post-status-item false" aria-hidden="true"><i class="fa fa-ban fa-2x"></i><span class="post-status-mesage"><span class="share-to">Facebook</span>: ' + response.FacebookError + '</span></div>';
                }

                var twbMess = '';
                if (response.ShareToTwitter === true) {
                    twbMess = '<div class="post-status-item true" aria-hidden="true"><i class="fa fa-check-circle fa-2x"></i><span class="post-status-mesage"><span class="share-to">Twitter</span>: share successfully</span></div>';
                    $scope.posts.unshift(response.TwitterPost);
                } else if (response.ShareToTwitter === false) {
                    twbMess = '<div class="post-status-item false" aria-hidden="true"><i class="fa fa-ban fa-2x"></i><span class="post-status-mesage"><span class="share-to">Twitter</span>: ' + response.TwitterError + '</span></div>';
                }

                var ghMess = '';
                if (response.ShareToGreenHouse === true) {
                    ghMess = '<div class="post-status-item true" aria-hidden="true"><i class="fa fa-check-circle fa-2x"></i><span class="post-status-mesage"><span class="share-to">Regit</span>: share successfully</span></div>';
                    $scope.posts.unshift(response.GreenHousePost);
                } else if (response.ShareToGreenHouse === false) {
                    ghMess = '<div class="post-status-item false" aria-hidden="true"><i class="fa fa-ban fa-2x"></i><span class="post-status-mesage"><span class="share-to">Regit</span>: ' + response.GreenHouseError + '</span></div>';
                }

                _sweetAlert.swal({
                    title: 'Share Detail',
                    html: true,
                    text: '<div class="post-status-container">' + ghMess + fbMess + twbMess + '</div>'
                });

                if (response.ShareToFacebook || response.ShareToTwitter || response.ShareToGreenHouse) {
                    var currentSocial = $scope.posts.findItem('Id', $scope.shareSocialPost.Id);
                    currentSocial.TotalShares = response.TotalShares;
                    $scope.configs.ShowShareSocialPostModal = false;
                }
            }, function (error) {
                var e = __errorHandler.ProcessErrors(error);
                __errorHandler.Swal(e, _sweetAlert);
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
                    })
                }
                $scope.socialPost = socialPost;
                $scope.configs.ShowAddCommentModal = true;
            })
        }

        $scope.onAddCommentToSocialPostModalHidden = function () {
            $scope.socialPost = {};
            $scope.comments = [];
            $scope.newComment = {};
        }

        $scope.saveAddCommentToSocialPost = function () {
            if (!$scope.newComment.Message || $scope.newComment.Message.isEmpty()) {
                _sweetAlert.warning($rootScope.translate('This_message_is_not_empty'));
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
                    __errorHandler.Swal(e, _sweetAlert);
                });
        }

        $scope.pullMoreFeed = function () {
            if (isSearchPost) {
                $rootScope.loadMoreSearchPosts();
            } else {
                var start = $scope.constant.start;
                var take = $scope.constant.take;
                dashboardService.PullDashboardFeed($scope.constant.pullFeedUrl, start, take, $rootScope.profile.Id).then(function (response) {
                    if (response.length > 0) {
                        for (var i = 0; i < response.length; i++) {
                            response[i].Message = $sce.trustAsHtml(response[i].Message.replace(/(?:\r\n|\r|\n)/g, '<br />'));
                            $scope.posts.push(response[i]);
                        }
                        $scope.constant.start += $scope.constant.take;
                    }

                }, function (error) {
                    var e = __errorHandler.ProcessErrors(error);
                    __errorHandler.Swal(e, _sweetAlert);
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
            _sweetAlert.swal({
                title: 'Warning',
                text: 'Are you sure remove this post?',
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#00aeef",
                confirmButtonText: "OK",
                closeOnConfirm: false
            },
                function (isConfirm) {
                    if (isConfirm) {
                        dashboardService.DeletePost(id, false)
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
                                        __common.swal(_sweetAlert, 'OK', '', 'success');
                                    }
                                },
                                function (error) {
                                    var e = __errorHandler.ProcessErrors(error);
                                    __errorHandler.Swal(e, _sweetAlert);
                                });
                    }
                });
        }

        $scope.init();

    }
]);

