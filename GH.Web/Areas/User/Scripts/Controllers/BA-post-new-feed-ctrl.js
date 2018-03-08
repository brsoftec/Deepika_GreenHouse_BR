var myApp = getApp("myApp");

myApp.run(['UserManagementService', 'AuthorizationService', '$rootScope', function (_userManager, _authService, $rootScope) {

}])

myApp.getController("BAPostNewFeedController", [
    '$scope', '$rootScope', '$http', 'BusinessAccountService', 'SweetAlert', function ($scope, $rootScope, $http, dashboardService, SweetAlert) {
        $scope.NewPost = { Photos: [], Privacy: { Public: true, Friends: false, Private: false } };
        $scope.postingImages = {};
        $scope.IsPostAdvance = false;
        $scope.NewPost.IsPostGreenHouse = true;
        $scope.setPrivacy = function (type) {
            if (type == 'public')
                $scope.NewPost.Privacy = { Public: true, Friends: false, Private: false };
            else if (type == 'private')
                $scope.NewPost.Privacy = { Public: false, Friends: false, Private: true };
        }

        $scope.PostANewPost = function () {
            if (!$scope.NewPost.IsPostGreenHouse && !$scope.NewPost.IsPostFacebook && !$scope.NewPost.IsPostTwitter) {
                SweetAlert.warning('You must choose at least one social network.');
                return false;
            }
            else if (!$scope.NewPost.Message || $scope.NewPost.Message.isEmpty()) {
                SweetAlert.warning('You must enter in message before posting.');
                return false;
            }

            //validate number of letters if post to Twitter
            if ($scope.NewPost.IsPostTwitter) {
                var message = $scope.NewPost.Message;
                if (message.length === 0 || message.length > 140) {
                    SweetAlert.warning("Twitter message length not valid!");
                    return;
                }
            }

            if ($scope.NewPost.Photos && $scope.NewPost.Photos.length > 1) {
                SweetAlert.warning("Cannot upload more than 1 picture!");
                return;
            }
            //get photo
            //call post API
            dashboardService.postNewFeedTobusiness("/API/Social/BAPostANewFeed", $scope.NewPost).then(function (response) {
                if (response != null || response != []) {
                    if ($rootScope.addExternalFeedToArray) {
                        for (var j = 0; j < response.data.length; j++) {
                            $rootScope.addExternalFeedToArray(response.data[j], true);
                        }
                    }
                    $scope.NewPost.Message = "";                    
                    $scope.NewPost.IsPostFacebook = false;
                    $scope.NewPost.IsPostTwitter = false;
                    $scope.IsPostAdvance = false;
                    SweetAlert.swal({ title: "Post Detail", text: 'Post successfull, please wait for approve!', closeOnConfirm: false });
                }
            }, function (error) {
                var e = __errorHandler.ProcessErrors(error);
                __errorHandler.Swal(e, SweetAlert);
            });
        }

        $scope.BAPostAdvance = function () {
            $scope.IsPostAdvance = true;
        }

        postMessageDone = function (MStatus) {

        };

        $scope.picturesSelected = function () {

            angular.forEach($scope.postingImages.__files, function (value, key) {
                var file = value;
                if (file && /^image\/.+$/.test(file.type)) {
                    var fileReader = new FileReader();
                    fileReader.readAsDataURL(file);
                    fileReader.onload = function (e) {
                        $scope.$apply(function () {
                            $scope.NewPost.Photos.push({thumb:e.target.result,file:file});
                        })
                    };
                }
            });
            

           
        };

        $scope.removepicture = function (index) {
            $scope.NewPost.Photos.splice(index, 1);
        }

        $scope.AdvancePostClose = function () {
            $scope.IsPostAdvance = false;
            $scope.NewPost = { Photos: [], Privacy: { Public: true, Friends: false, Private: false } };
            $scope.postingImages = {};
        }

    }
]);