var myApp = getApp("myApp");

myApp.run(['UserManagementService', 'AuthorizationService', '$rootScope', function (_userManager, _authService, $rootScope) {

}])

myApp.getController("PostNewFeedController", [
    '$scope', '$rootScope', '$http', 'DashboardService', 'SweetAlert', 'AuthorizationService', 'NotificationService', function ($scope, $rootScope, $http, dashboardService, SweetAlert,
        authService, notificationService) {
        $scope.NewPost = { Photos: [], Privacy: { Public: true, Friends: false, Private: false } };
        $scope.IsPostAdvance = false;
        $scope.postingImages = {};
        $scope.NewPost.IsPostGreenHouse = true;
        $scope.setPrivacy = function (type) {
            if (type == 'public')
                $scope.NewPost.Privacy = { Public: true, Friends: false, Private: false };
            else if (type == 'friends')
                $scope.NewPost.Privacy = { Public: false, Friends: true, Private: false };
            else if (type == 'private')
                $scope.NewPost.Privacy = { Public: false, Friends: false, Private: true };
        }

        $scope.postAdvance = function () {
            $scope.IsPostAdvance = true;
        }
        
        $scope.AdvancePostClose = function () {
            $scope.IsPostAdvance = false;
            $scope.NewPost = { Photos: [], Privacy: { Public: true, Friends: false, Private: false } };
            $scope.postingImages = {};
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
            $scope.NewPost.Photos.splice(index, 1);
        }

        $scope.PostANewPost = function () {
            if (!$scope.NewPost.IsPostGreenHouse && !$scope.NewPost.IsPostFacebook && !$scope.NewPost.IsPostTwitter) {
                SweetAlert.warning($rootScope.translate('You_must_choose_at_least_one_social_network'));
                return false;
            }
            else if (!$scope.NewPost.Message || $scope.NewPost.Message.isEmpty()) {
                SweetAlert.warning($rootScope.translate('You_must_enter_in_message_before_posting'));
                return false;
            }

            //validate number of letters if post to Twitter
            if ($scope.NewPost.IsPostTwitter) {
                var message = $scope.NewPost.Message;
                if (message.length === 0 || message.length > 140) {
                    SweetAlert.warning($rootScope.translate('Twitter_message_lenght_is_not_greater_than_140'));
                    return;
                }
            }

            if ($scope.NewPost.Photos && $scope.NewPost.Photos.length > 1) {
                SweetAlert.warning($rootScope.translate('Cannot_upload_more_than_1_picture'));
                return;
            }

            //call post API
            dashboardService.PostNewFeedToSocial("/API/Social/PostANewFeed", $scope.NewPost).then(function (response) {
                if (response != null || response != []) {
                    if ($rootScope.addExternalFeedToArray && response.data) {
                        $rootScope.addExternalFeedToArray(response.data, true);
                    }
                    $scope.NewPost.Message = "";                    
                    $scope.NewPost.IsPostFacebook = false;
                    $scope.NewPost.IsPostTwitter = false;
                    $scope.IsPostAdvance = false;
                    var diaglogmessage = '<div class="post-status-container">';
                    angular.forEach(response.status, function (value, key) {
                        if (value.sucess) {
                            diaglogmessage = diaglogmessage + '<div class="post-status-item ' + value.sucess + '" aria-hidden="true"><i class="fa fa-check-circle fa-2x"></i><span class="post-status-mesage">' + value.message + '</span></div>';
                        } else {
                            diaglogmessage = diaglogmessage + '<div class="post-status-item ' + value.sucess + '" aria-hidden="true"><i class="fa fa-ban fa-2x"></i><span class="post-status-mesage">' + value.message + '</span></div>';
                        }
                    });
                    diaglogmessage = diaglogmessage + '</div>';

                    SweetAlert.swal({ title: "Post Detail", text: diaglogmessage, html: true, closeOnConfirm: false });
                }
            }, function (error) {
                var e = __errorHandler.ProcessErrors(error);
                __errorHandler.Swal(e, SweetAlert);
            });
        }

        postMessageDone = function (MStatus) {

        };

    }
]);


myApp.getController('NotifycationNewController', ['$scope', '$routeParams', '$location', 'ajaxService', 'dataGridService', 'alertService',
    'applicationConfiguration', '$uibModal', 'NotifycationService',
    function ($scope, $routeParams, $location, ajaxService, dataGridService, alertService, applicationConfiguration, $uibModal, NotifycationService) {
        $scope.ViewPopup = function (delegationId) {
            NotifycationService.closepopup();
            var modalInstance = $uibModal.open({
                animation: $scope.animationsEnabled,
                templateUrl: 'modal-delegate-view.html',
                controller: 'ViewDelegationMasterController',
                size: "",
                resolve: {
                    objectdelegationId: function () {
                        return {
                            Id: delegationId
                        }
                    }
                }
            });
            modalInstance.result.then(function () {
            }, function () {
            })
        }

        $scope.showmore = function () {
            NotifycationService.closepopup();
            $location.path("Notifycation/ManagerNotifycation");
        }

        $scope.ListNotifycation = [];

        $scope.getListNotifycation = function () {
            var NotifycationModelView = new Object();
            NotifycationModelView.UserId = applicationConfiguration.usercurrent.Id;
            ajaxService.ajaxPost(NotifycationModelView, applicationConfiguration.urlwebapi + "api/NotifycationService/GetLisNotificationNewView",
                this.getListNotifycationOnSuccess, this.getListNotifycationOnError);
        }

        $scope.getListNotifycationOnSuccess = function (response) {
            applicationConfiguration.usercurrent.CountNewNotifycation = "0";
            $scope.ListNotifycation = response.Listitems;
        }
        $scope.getListNotifycationOnError = function (response) {
        }

        $scope.getListNotifycation();

    }]);