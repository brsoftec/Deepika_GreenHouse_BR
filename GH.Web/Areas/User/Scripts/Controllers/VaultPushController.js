var myApp = getApp("myApp", true);
// var myApp = angular.module("myApp");



    myApp.getController('VaultPushController', 
    ['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService', 'NetworkService',

function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, notificationService, _networkService ) {

    $scope.pushingVault = false;
    $scope.vaultPush = {
        label: '',
        fields: null,
        button: null,
        fieldPushed: [],
        showPushed: false,
        sendOptions: ['From network', 'User email'],
        sendToEmail: false
    };

    // Vu

     $scope.ListMember = [];
    
    $scope.GetAllFriends = function () {
        _networkService.GetFriends().then(function (result) {
                $scope.ListMember = result;
            })
    }

    // End Vu
    $scope.newPushForm = function (event, label) {
        $scope.pushingVault = true;

        $scope.vaultPush.fields = [];
        $scope.vaultPush.label = label;

        $scope.vaultPush.button = angular.element(event.target);
        $scope.vaultPush.button.addClass('active')
            .closest('.vault-group').addClass('vault-pushing');
    };
    $scope.addField = function (event, jsPath, value) {
        if (!jsPath in vaultMap) return;
        var field = vaultMap[jsPath];
        $scope.vaultPush.fieldPushed[jsPath] = true;
        var type = field[1] || 'textbox';
        $scope.vaultPush.fields.push({
            jsPath: jsPath,
            label: field[0],
            controlType: type,
            options: field[2],
            value: value
        });
        angular.element(event.target).closest('.vault-row').addClass('field-pushed');
    };
    $scope.removeField = function (event, jsPath) {
        $scope.vaultPush.fieldPushed[jsPath] = false;
        angular.forEach($scope.vaultPush.fields, function (field, index) {
            if (field.jsPath === jsPath) {
                $scope.vaultPush.fields.splice(index, 1);
            }
        });

        angular.element(event.target).closest('.vault-row').removeClass('field-pushed');
    };
    $scope.savePushForm = function () {
    };
    $scope.cancelPushForm = function (event) {
        $scope.pushingVault = false;
        $scope.vaultPush.fields = null;
        $scope.vaultPush.label = '';

        $scope.vaultPush.button = angular.element(event.target);
        $scope.vaultPush.button.removeClass('active')
            .closest('.vault-group').removeClass('vault-pushing');
    };

    // Call
    $scope.GetAllFriends();
}]);

