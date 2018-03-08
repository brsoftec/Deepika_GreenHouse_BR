myApp.getController('BusinessPrivacySettingsController', ['$rootScope', '$scope', 'SweetAlert', 'UserManagementService', 'BusinessAccountService', function ($rootScope, $scope, SweetAlert, UserManagementService, BusinessAccountService) {
    $scope.privacyItems = ['Public', 'Private'];
    $scope.Model = null;
    $scope.BaId;
    BusinessAccountService.GetBusinessProfile().then(function (profile) {
        $scope.Model = profile.BusinessPrivacies;
        $scope.Model.BAId = profile.Id
    });
    $scope.setprivacy = function (index) {
        $scope.Model.Privacy = index;
    }
    $scope.Save = function () {
        UserManagementService.UpdateBusinessPrivacyAccount($scope.Model);
    }
}])