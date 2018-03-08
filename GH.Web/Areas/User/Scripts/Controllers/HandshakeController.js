var myApp = getApp("myApp", true);

myApp.getController("HandshakeController", ['$scope', '$rootScope', '$timeout', '$document', '$http', 'rguView', 'rguModal', 'rguNotify', function ($scope, $rootScope, $timeout, $document, $http, rguView, rguModal, rguNotify) {

    $scope.view = $scope.view || {};

    $scope.view.showingUserSelect = true;

    $scope.handshakeConfig = {
        notifyFormats:
            [{
                label: 'Notification only',
                value: 'notif'

            }, {
                label: 'Show changed values',
                value: 'values'
            }
            ]
    };

    $scope.openHandshakeNewModal = function () {
        $scope.handshake = {
            description: '',
            toName: '',
            toEmail: '',
            toType: '',
            expiry: {
                indefinite: true,
                date: new Date()
            },
            notifyFormat: 'notif'
        };

        $scope.handshake.fields = [
            {label: 'Current Address', jsPath: '.address.currentAddress', selected: true},
            {label: 'Mailing Address', jsPath: '.address.mailingAddress', selected: false},
            {label: 'Mobile Number', jsPath: 'contact.mobile', selected: true},
            {label: 'Office Number', jsPath: 'contact.office', selected: false},
            {label: 'Personal Email', jsPath: 'contact.email', selected: true},
            {label: 'Work Email', jsPath: 'contact.officeEmail', selected: false}
        ];

        rguModal.openModal('handshake.new', $scope);
    };

    $scope.closeModal = function (hideFunc) {
        if (angular.isFunction(hideFunc)) {
            hideFunc();
        }
    };

    $scope.canAdd = function () {
        return $scope.handshake.toEmail.length && $scope.handshake.fields.some(function (field) {
            return field.selected;
        });
    };

    $scope.addHandshake = function (hideFunc) {
        var handShake = new Object();
        handShake = angular.copy($scope.handshake);
        $http.post('/api/manualhandshake/invite', handShake)
            .success(function (response) {
                rguNotify.add('Created handshake with ' + (handShake.toType === 'user' ? handShake.toName : handShake.toEmail));

            }, function (errors) {
                console.log('Error creating handshake: ', errors)
            });

        $rootScope.$broadcast('LoadManualHandshake');
        $scope.closeModal(hideFunc);
    };

    $scope.actions = {
        selectUser: function (user) {
            $timeout(function () {
                $scope.handshake.toEmail = user.email;
                $scope.handshake.toName = user.displayName;
                $scope.handshake.toAvatar = user.avatar;
                $scope.handshake.toType = user.type || 'user';
            });
        }
    };


}]);

