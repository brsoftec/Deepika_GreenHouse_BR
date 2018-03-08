angular.module("regitApp")
    .controller('DelegationCtrl', function ($scope, $timeout, $uibModal) {
        $scope.toTitleCase = function (str) {
            return str.replace(/\w\S*/g, function (txt) {
                return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
            });
        };
        $scope.delegatees = [
            {
                name: 'Jennifer Pham',
                role: 'super',
                expires: 'indefinite',
                status: 'active'
            },
            {
                name: 'Andy Roberto',
                role: 'secretary',
                expires: 'indefinite',
                status: 'active'
            },
            {
                name: 'Maria Ginza',
                role: 'normal',
                expires: '2 July 2016',
                status: 'inactive'
            },
            {
                name: 'Superman',
                role: 'custom',
                expires: '12 July 2016',
                status: 'pending'
            },
            {
                name: 'Hugo Lina',
                role: 'normal',
                expires: '30 Jan 2016',
                status: 'expired'
            }
        ];
        $scope.delegation = {
            role: 'Normal',
            fromDate: new Date(),
            expiry: 'Indefinite',
            permissions: [
                {
                    name: 'Basic Information',
                    read: false,
                    write: false
                }, {
                    name: 'Address',
                    read: false,
                    write: false
                }, {
                    name: 'Govenment ID',
                    read: false,
                    write: false
                }, {
                    name: 'Education',
                    read: false,
                    write: false
                }, {
                    name: 'Employment',
                    read: false,
                    write: false
                }, {
                    name: 'Family',
                    read: false,
                    write: false
                }, {
                    name: 'Membership',
                    read: false,
                    write: false
                }, {
                    name: 'Financial',
                    read: false,
                    write: false
                }, {
                    name: 'Other',
                    read: false,
                    write: false
                }
            ]
        };

        $scope.onPermissionChange = function(permission) {
            $timeout(function() {
                if (permission.write) {
                    permission.read = true;
                }
            });
        };

        $scope.openDelegate = function ($event, action, status) {
            $event.preventDefault();
            $scope.currentDelegationStatus = status;
            var modalInstance = $uibModal.open({
                templateUrl: 'modal-delegate-' + action + '.html',
                size: 'md',
                controller: 'ModalCtrl',
                scope: $scope
            });
        };
        $scope.openDelete = function ($event, type) {
            $event.preventDefault();
            var modalInstance = $uibModal.open({
                templateUrl: type === 'delegatee' ? 'modal-delegatee-delete.html' : 'modal-delegator-delete.html',
                size: 'sm',
                controller: 'ModalCtrl'
            });
        };
        $scope.submit = function () {
            $uibModalInstance.close(0);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };
    });