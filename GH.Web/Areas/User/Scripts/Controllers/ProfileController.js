var myApp = getApp("myApp", true);
// var myApp = angular.module('myApp');

myApp.getController("MainProfileController", [ '$scope', '$http', function($scope, $http) {
    $scope.pullProfile = function() {
        $scope.profile = {};
        $http.get('/Api/Users/CurrentProfile')
            .success(function (response) {
                $scope.profile = response.data;
            }).error(function (error) {
            console.log(error);
        });
    };
    $scope.pullProfile();
    $scope.$on('profile:update',function () {
        $scope.pullProfile();
    });
}]);

myApp.getController("BusinessProfileController", ['$scope', '$http', function($scope, $http) {
    $scope.pullProfile = function() {
        $scope.businessProfile = {};
        $http.get('/Api/Users/BusinessProfile')
            .success(function (response) {
                $scope.businessProfile = response;
            }).error(function (error) {
            console.log(error);
        });
    };
    $scope.pullProfile();
    $scope.$on('profile:update',function () {
        $scope.pullProfile();
    });
}]);


myApp.getController("ViewedProfileController", [ '$scope', '$http', function($scope, $http) {
    $scope.pullProfile = function() {
        $scope.viewedProfile = {};
        $http.get('/Api/Users/Profile/' + regitGlobal.viewedProfileId)
            .success(function (response) {
                var profile = response.data;
                $scope.viewedProfile = profile;
                if (angular.isObject($scope.profiles)) {
                    $scope.profiles.viewedProfile = profile;
                }
            }).error(function (error) {
            console.log(error);
        });
    };
    $scope.pullProfile();
    $scope.$on('profile:update',function () {
        $scope.pullProfile();
    });
}]);