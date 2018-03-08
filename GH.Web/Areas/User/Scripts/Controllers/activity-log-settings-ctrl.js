myApp.getController('ActivityLogSettingsController', ['$scope', '$rootScope', '$http', 'UserManagementService','$timeout', function ($scope, $rootScope, $http, _userManager, $timeout) {
    
    $scope.baseActivityLogSettings = {};
    $scope.activityLogSettings = {};
    
    _userManager.GetCurrentUserActivityLogSettings().then(function (actLogSettings) {
        if (actLogSettings) {
            $scope.baseActivityLogSettings = actLogSettings;
            $scope.activityLogSettings = angular.copy($scope.baseActivityLogSettings);
        }
    })

    $scope.saveActivityLogSetting = function (actLogSetting) {
        var updateModel = angular.copy($scope.baseActivityLogSettings);
        updateModel[actLogSetting] = $scope.activityLogSettings[actLogSetting];

        _userManager.UpdateCurrentUserActivityLogSettings(updateModel).then(function (actLogSettings) {
            $scope.baseActivityLogSettings = actLogSettings;
        })
    }

}])
