myApp.getController('RelationshipSettingsController',
[
    '$scope', '$http','SweetAlert', 'UserManagementService', 'BusinessAccountService', '$uibModal',
    function ($scope, $http, SweetAlert, _userManager, businessAccountService, $uibModal) {
        var self = this;
        self.Interaction = false;
        self.Event = false;
        self.Network = false;
        self.Workflow = false;
        self.currentPage = 0;
        self.Followees = [];
        self.followParameter = {
            UserId: "",
            Start: 0,
            Length: 10
        }

        $scope.view = {
            start: 0,
            total: 0
        };

        self.pageChanged = function () {
            self.followParameter.Start = self.currentPage - 1;
            $scope.view.start = (self.currentPage-1)*10;
            //self.GetFollowee();
        }

        self.GetFollowee = function () {
/*            _userManager.GetFolloeeOfCurrentUser(self.followParameter)
            .then(function (res) {
                self.Followees = res;
            });*/

            $http.get('/api/Network/Business/List', {
                params: {
                }
            })
                .success(function (response) {
                    $scope.businesses = response.data;
                });
        };

        self.open = function (business) {
            var modalInstance = $uibModal.open({
                templateUrl: 'transactionDetail.html',
                controller: function ($scope, $uibModalInstance, follow) {
                    $scope.follow = business;
                    $scope.transactions = [];
                    $scope.Total = 0;
                    $scope.isLoadding = false;
                    $scope.isAll = false;
                    $scope.parameter = {
                        FromUser: "",
                        ToUser: business.id,
                        TransactionType: 0,
                        Length: 7
                    };
                    _userManager.GetFollowTransactions($scope.parameter)
                        .then(function (response) {
                            $scope.transactions = response.Transactions;
                            $scope.Total = response.Total;
                            if (response.Total === response.Transactions.length) {
                                $scope.isAll = true;
                            }
                        });

                    $scope.cancel = function () {
                        $uibModalInstance.dismiss('cancel');
                    };
                },
                resolve: {
                    follow: function () {
                        return business;
                    }
                }
            });
        };
        self.unfollow = function (business) {
            var followModel = {
                Id: business.id
            };
            /*businessAccountService.UnfollowBusiness(followModel)
                .then(function (data) {
                    self.GetFollowee();
                });*/
            $http.post('/api/Interaction/Unfollow/' + business.id)
                .success(function (response) {
                    self.GetFollowee();
                })                .error(function (response) {
                    console.log(response)
                });
        }
        self.GetFollowee();
    }
]);

