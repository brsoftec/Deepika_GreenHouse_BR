myApp.getController('PrivacySettingsController', ['$scope', '$rootScope', '$http', 'UserManagementService', function ($scope, $rootScope, $http, _userManager) {

    $scope.options = {
        SendMeMessage: [
            {
                Display: 'All',
                Value: 'All'
            },
            {
                Display: 'Network Only',
                Value: 'Network'
            },
            {
                Display: 'Business Only',
                Value: 'Business'
            }
        ]
    }

    $scope.basePrivacies = {
    };

    $scope.privacies = {
    };


    _userManager.GetCurrentUserPrivacies().then(function (privacies) {
        if (privacies) {
            privacies.SendMeMessage = $scope.options.SendMeMessage[privacies.SendMeMessage].Value;
            $scope.basePrivacies = privacies;
            $scope.privacies = angular.copy($scope.basePrivacies);
        }
    })


    $scope.savePrivacy = function (privacy) {
        var updateModel = angular.copy($scope.basePrivacies);
        updateModel[privacy] = $scope.privacies[privacy];

        _userManager.UpdateCurrentUserPrivacies(updateModel).then(function (privacies) {
            $scope.basePrivacies = privacies;
        })
    }

}])
