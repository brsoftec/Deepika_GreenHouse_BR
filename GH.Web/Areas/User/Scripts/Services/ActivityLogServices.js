var socialModule = getApp("SocialModule", false);

socialModule.factory('ActivityLogServices', [
    '$http', '$q', function ($http, $q) {

        var pullActivityLog = function (url, filter, start, take) {
            var defer = $q.defer();
            $http.get(url + "?activityLogType=" + filter + "&start=" + start + "&take=" + take).success(function (response) {
                defer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, defer);
            });
            return defer.promise;
        }

        var pullMoreActivityLog = function (url, filter, start, take) {
            var defer = $q.defer();
            $http.get(url + "?activityLogType=" + filter + "&start=" + start + "&take=" + take).success(
                function (response) {
                    defer.resolve(response);
                }).error(function (errors, status) {
                    __promiseHandler.Error(errors, status, defer);
                });

            return defer.promise;
        }

       
        var _deleteActivityLog = function (model) {
            var deferer = $q.defer();

            $http.post('/api/ActivityLogService/DeleteActivityLog', model)
              .success(function () {
                  deferer.resolve();
              }).error(function (errors, status) {
                  __promiseHandler.Error(errors, status, deferer);
              })

            return deferer.promise;
        }
      
        var _deleteActivityLogByUser = function () {
            var deferer = $q.defer();

            $http.post('/api/ActivityLogService/DeleteActivityLogByUserId')
              .success(function () {
                  deferer.resolve();
              }).error(function (errors, status) {
                  __promiseHandler.Error(errors, status, deferer);
              })

            return deferer.promise;
        }
        return {
            PullActivityLog: pullActivityLog,
            PullMoreActivityLog: pullMoreActivityLog,
            DeleteActivityLog: _deleteActivityLog,
            DeleteActivityLogByUser: _deleteActivityLogByUser

        };
    }
]);