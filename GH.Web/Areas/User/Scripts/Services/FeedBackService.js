var socialModule = getApp("SocialModule");

socialModule.factory('FeedBackService', [
    '$http', '$q', function ($http, $q) {

        var _pullFeedBack = function (url) {
            var defer = $q.defer();
            $http.get(url).success(function (response) {
                defer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, defer);
            });
            return defer.promise;
        }
       
        var _deleteFeedBack = function (model) {
            var deferer = $q.defer();

            $http.post('/api/FeedBackService/DeleteFeedBack', model)
              .success(function () {
                  deferer.resolve();
              }).error(function (errors, status) {
                  __promiseHandler.Error(errors, status, deferer);
              })

            return deferer.promise;
        }

        var _insertFeedBack = function (model) {
            var deferer = $q.defer();
            $http.post('/Api/FeedBackService/InsertFeedBack', model)
                .success(function () {
                    deferer.resolve();
                }).error(function (errors, status) {
                    __promiseHandler.Error(errors, status, deferer);
                })

            return deferer.promise;
    }
       
        return {
            InsertFeedBack: _insertFeedBack,
            PullFeedBack: _pullFeedBack,
            DeletFeedBack: _deleteFeedBack
          

        };
    }
]);