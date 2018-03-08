myApp.getController('NotificationSettingsController', ['$scope', 'SweetAlert', 'UserManagementService', function ($scope, SweetAlert, UserManagementService) {

    $scope.Model = {
        Interactions: false,
        EventAndReminders: false,
        NetworkRequest: false,
        Workflow:false
    };
    UserManagementService.GetCurrentUserProfile().then(function (profile) {
        if (profile.NotificationSettings) {
            $scope.Model = profile.NotificationSettings;
        }
        
    });
    $scope.Save = function () {
        UserManagementService.UpdateAccountNotificationSetting($scope.Model);
    }
}])