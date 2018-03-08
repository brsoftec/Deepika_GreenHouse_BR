//var myApp = getApp("myApp", ['SocialModule', 'angularMoment', 'CommonDirectives', 'UserModule', 'oitozero.ngSweetAlert', 'NotificationModule'], true);
var myApp = getApp("myApp", true);
myApp.filter("trustUrl", ['$sce', function ($sce) {
    return function (recordingUrl) {
        return $sce.trustAsResourceUrl(recordingUrl);
    };
}]);

myApp.getController("ManagerActivityLogController", [
    '$scope', '$rootScope', '$http', 'SweetAlert', 'ActivityLogServices', '$sce', '$q', function ($scope, $rootScope, $http, _sweetAlert, activityLogServices, $sce, $q) {

        // Son
        $scope.view = {
            currentFilter: 'all'
        };
          $scope.ActivityLogs = [];
          $scope.selectFilter = function (filter) {
              $scope.ActivityLogs.length = 0;
              $scope.ActivityLogs = [];
            $scope.view.currentFilter = filter;

            $scope.pullActivityLog($scope.view.currentFilter);
        };
        //
     
        $scope.init = function () {
         
            $scope.constant = {
                //1. url api
                pullFeedUrl: "/api/ActivityLogService/GetActivityLogs",
                start: 0,
                take: 20
            };
            //2. call service ISODate("2012-07-14T01:00:00+01:00").toLocaleTimeString()
            $scope.pullActivityLog($scope.view.currentFilter);
        };

        //$scope._listForm.push(
        $scope.testTime = [];
        $scope.pullActivityLog = function (filter) {
            $scope.constant = {
                //1. url api
                pullFeedUrl: "/api/ActivityLogService/GetActivityLogs",
                start: 0,
                take: 20
            };
            var start = $scope.constant.start;
            var take = $scope.constant.take;
            activityLogServices.PullActivityLog($scope.constant.pullFeedUrl, filter, start, take).then(function (response) {
                if (response && response.length > 0) {
                  
                    $scope.ActivityLogs = response;
                    //for(var i=0; i< response.length; i++)
                    //{
                    //    var dt = new Date.UTC(response[i].DateTime);
                    //    $scope.testTime.push(dt);
                    //}
                    //$scope.constant.start += $scope.constant.take;
                }
            });
        }

//more var localDate = new Date(utcDate);

        $scope.pullMoreActivityLog = function (filter) {
            var start = $scope.constant.start;
            var take = $scope.constant.take;
            activityLogServices.PullMoreActivityLog($scope.constant.pullFeedUrl, filter, start, take).then(function (response) {
                if (response.length > 0) {
                    for (var i = 0; i < response.length; i++) {
                        $scope.ActivityLogs.push(response[i]);
                    }
                    $scope.constant.start += $scope.constant.take;
                }

            }, function (error) {
                swal('Pull more activity log', 'Error', 'error');
            });

        }

        $scope.deleteActivityLog = function (_value) {
            var index = $scope.ActivityLogs.indexOf(_value);
            var act = new Object();
            act.ActivityId = _value.ActivityId;
            act.FromUserId = _value.FromUserId;
            swal({
                title: "Are you sure to remove this this activity log?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"
            }).then(function () {
                    _value.removed = true;
                    activityLogServices.DeleteActivityLog(act).then(function () {
                        $scope.ActivityLogs.splice(index, 1);
                    });

                }
                , function (dismiss) {

                });
        };

        $scope.deleteActivityLogByUser = function () {

            swal({
                title: "Are you sure to clear all your activity log?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No"
            }).then(function () {

                    activityLogServices.DeleteActivityLogByUser().then(function () {
                        $scope.ActivityLogs = [];
                    });

                }
                , function (dismiss) {

                });
        }

        $scope.init();
    }
])
;



