

var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule', 'highcharts-ng'], true);
myApp.getController('IndividualBusinessController',
['$scope', '$rootScope', '$http', '$uibModal', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService',
function ($scope, $rootScope, $http, $uibModal, userManager, sweetAlert, authService, alertService, notificationService) {
    //var vm = this;
    $scope.futurify = false;
    $scope.initializeController = function () {
        var data = new Object();

        $http.post("/api/BusinessUserSystemService/GetBusinessListByUser", data)
          .success(function (response) {
              $scope.businesses = [];
              $scope.transactions = [];

              for (var i = 0; i < response.BusinessProfileList.length; i++) {
                  $scope.businesses.push({
                      BusinessId: response.BusinessProfileList[i].Id,
                      //UserId: response.UserId,
                      CampaignId: "",
                      Name: response.BusinessProfileList[i].DisplayName,
                      Avatar: response.BusinessProfileList[i].Avatar,
                      Desc: response.BusinessProfileList[i].Description
                  });
              }
              $scope.totalPages = 10;
              $scope.currentPage = 1;

          })
          .error(function (errors, status) {
              //__promiseHandler.Error(errors, status, deferer);
          });
    };

    $scope.openTransactions = function ($event, businessId) {
        $event.preventDefault();
        var data2 = new Object();
        data2.BusinessUserId = businessId;
        $scope.transactions = [];
        $http.post("/api/BusinessUserSystemService/GetTransactionByBusinessId", data2)
         .success(function (response) {


             for (var j = 0; j < response.ActivityLogList.length; j++) {
                 $scope.transactions.push({
                     desc: response.ActivityLogList[j].Title,
                     date: response.ActivityLogList[j].DateTime
                 });
             }

         })
         .error(function (errors, status) {

         });


        var modalInstance = $uibModal.open({
            templateUrl: 'modal-transactions.html',
            size: 'lg',
            controller: 'TransactionController',
            scope: $scope
        });
    };
    //Description: Remove follower from campain
    $scope.RemoveBusinessByUser = function (userId, businessId) {
        var data = new Object();
        //var deferer = $q.defer();
        //data.UserId = applicationConfiguration.usercurrent.Id;
        data.BusinessUserId = businessId;
        $http.post("/api/BusinessUserSystemService/RemoveBusinessFromUser", data)
          .success(function (response) {
              var dataresult = response;
              $scope.initializeController();
          })
          .error(function (errors, status) {

              //   __promiseHandler.Error(errors, status, deferer);
          });

        //ajaxService.ajaxPost(data, applicationConfiguration.urlwebapi + "api/BusinessUserSystemService/RemoveBusinessFromUser",
        //    function (sucessData) {
        //        dataresult = sucessData;
        //    },
        //    function (errorData) {
        //        dataresult = errorData;
        //    });
        //$location.path("BusinessUserSystem/IndividualBusiness");
    };


}
]);

