
var myApp = getApp("myApp", ['TranslationModule','UserModule', 'SocialModule', 'CommonDirectives', 'oitozero.ngSweetAlert', 'ui.bootstrap', 'BusinessAccountModule', 'NotificationModule'], true);

myApp.filter("trustUrl", ['$sce', function ($sce) {
    return function (recordingUrl) {
        return $sce.trustAsResourceUrl(recordingUrl);
    };
}]);

myApp.getController('PendingPostController', ['$scope', '$rootScope', '$sce', 'BusinessAccountService', 'SweetAlert', function ($scope, $rootScope, $sce, _baService, _sweetAlert) {
    $scope.post = {};
    $scope.NewPost = {};
    $scope.IsPostAdvance = false;
    $scope.approval = {};
    $scope.postingImages = {};
    $scope.rejection = { Comment: '' };
    $scope.canEdit = function () {
        var _canEdit = false;
        if ($scope.post.Status == 'Draft' || $scope.post.Status == 'Repost' || $scope.post.Status == 'Rejected') {
            angular.forEach($rootScope.currentUserRoles, function (value, key) {
                if (value == 'Editor') {
                    _canEdit =  true;
                }
            }); 
        }
        return _canEdit;
    }

    $scope.CanApprove = function () {
        var _canreview = false;
        if ($scope.post.Status == 'Draft' || $scope.post.Status == 'Edited') {
            angular.forEach($rootScope.currentUserRoles, function (value, key) {
                if (value == 'Reviewer') {
                    _canreview = true;
                }
            });
        }
        return _canreview;
    };

    $scope.loadPendingPost = function (id) {
        _baService.GetBusinessPost(id).then(function (rpost) {
            $scope.post = rpost;
            $scope.post.Message = $sce.getTrustedHtml(rpost.Message.replace(/(?:\r\n|\r|\n)/g, '<br />'));
            $scope.approval.Id = rpost.Id;
            $scope.rejection.Id = rpost.Id;
            angular.forEach(rpost.SocialTypes, function (value, key) {
                if (value == "GreenHouse") {
                    $scope.post.IsPostGreenHouse = true;
                }

                if (value == "Twitter") {
                    $scope.post.IsPostTwitter = true;
                }

                if (value == "Facebook") {
                    $scope.post.IsPostFacebook = true;
                }
            });
        })
    }

    $scope.approve = function () {
        if (!$scope.approval.Id) {
            return;
        }

        if (!$scope.CanApprove()) {
            return;
        }

        _baService.ApproveBusinessPost($scope.approval).then(function () {
            __common.swal(_sweetAlert,$rootScope.translate('Approve_successfully'), '','success');
            $scope.post.Status = 'Approved';
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        })

    }

    $scope.reject = function () {
        if (!$scope.rejection.Id) {
            return;
        }

        if ($scope.rejection.Comment.trim().length == 0) {
            __common.swal(_sweetAlert,'warning', $rootScope.translate('Comment_is_required_for_rejection'), 'warning');
            return;
        }

        if (!$scope.CanApprove()) {
            return;
        }

        _baService.RejectBusinessPost($scope.rejection).then(function () {
            __common.swal(_sweetAlert,$rootScope.translate('Reject_successfully'), '','success');
            $scope.post.Status = 'Rejected';
        }, function (errors) {
            __errorHandler.Swal(__errorHandler.ProcessErrors(errors), _sweetAlert);
        });
    }

    $scope.cancel = function () {
        $scope.rejection.Comment = '';
    }
    $scope.EditPost = function () {
        $scope.IsPostAdvance = true;
        $scope.NewPost = angular.copy($scope.post);
        $scope.NewPost.DeletedPhoto = '';
    }
    $scope.picturesSelected = function () {
        angular.forEach($scope.postingImages.__files, function (value, key) {
            var file = value;
            if (file && /^image\/.+$/.test(file.type)) {
                var fileReader = new FileReader();
                fileReader.readAsDataURL(file);
                fileReader.onload = function (e) {
                    $scope.$apply(function () {
                        $scope.NewPost.Photos.push({ thumb: e.target.result, file: file });
                    })
                };
            }
        });
    };

    $scope.removepicture = function (index) {
        $scope.NewPost.DeletedPhoto = $scope.NewPost.DeletedPhoto + $scope.NewPost.Photos[index].Url+";";
        $scope.NewPost.Photos.splice(index, 1);
    }
    $scope.AdvancePostClose = function () {
        $scope.IsPostAdvance = false
    }
    $scope.PostANewPost = function () {
        if (!$scope.NewPost.IsPostGreenHouse && !$scope.NewPost.IsPostFacebook && !$scope.NewPost.IsPostTwitter) {
            _sweetAlert.warning($rootScope.translate('You_must_choose_at_least_one_social_network'));
            return false;
        }
        else if (!$scope.NewPost.Message || $scope.NewPost.Message.isEmpty()) {
            _sweetAlert.warning($rootScope.translate('You_must_enter_in_message_before_posting'));
            return false;
        }

        //validate number of letters if post to Twitter
        if ($scope.NewPost.IsPostTwitter) {
            var message = $scope.NewPost.Message;
            if (message.length === 0 || message.length > 140) {
                _sweetAlert.warning($rootScope.translate('Twitter_message_length_not_valid'));
                return;
            }

            if ($scope.NewPost.Photos.countlength > 1) {
                _sweetAlert.warning($rootScope.translate('Twitter_allow_1_photo_in_post'));
            }
        }
        //get photo
        //call post API
        _baService.EditFeedTobusiness($scope.NewPost).then(function (response) {
            if (response != null || response != []) {
                if (response.sucess) {
                    $scope.IsPostAdvance = false;
                    $scope.loadPendingPost(response.Id);
                    _sweetAlert.swal({ title: $rootScope.translate('Post_Detail'), text: response.message, closeOnConfirm: false });
                } else {
                    _sweetAlert.swal({ title: $rootScope.translate('Post_Detail'), text: response.message, closeOnConfirm: false });
                }
                
            }
        }, function (error) {
            var e = __errorHandler.ProcessErrors(error);
            __errorHandler.Swal(e, _sweetAlert);
        });
    }
}]);