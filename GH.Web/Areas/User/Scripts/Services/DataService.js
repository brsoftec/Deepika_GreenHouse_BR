var _dataApp = getApp('DataModule', [], false);

_dataApp.factory('DataService', ['$http', '$q', function ($http, $q) {

    var getAllCountry = function () {
        var deferer = $q.defer();

        $http.get('/Api/Data/Country', {})
            .success(function(res) {
                deferer.resolve(res.Countries);
            }).error(function(errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            });

        return deferer.promise;
    }

    var getCityByCountry = function (countryCode) {
        var deferer = $q.defer();

        $http.get('/Api/Data/Country/City', { params: { countryCode: countryCode, hideAjaxLoader: true } })
            .success(function(res) {
                deferer.resolve(res.Cities);
            }).error(function(errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            });

        return deferer.promise;
    }

    var getCityByRegion = function (regionCode) {
        var deferer = $q.defer();

        $http.get('/Api/Data/Region/City', { params: { regionCode: regionCode, hideAjaxLoader: true } })
            .success(function(res) {
                deferer.resolve(res.Cities);
            }).error(function(errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            });

        return deferer.promise;
    }

    var getRegionByCountry = function (countryCode) {
        var deferer = $q.defer();

        $http.get('/Api/Data/Country/Region', { params: { countryCode: countryCode, hideAjaxLoader: true } })
            .success(function(res) {
                deferer.resolve(res.Regions);
            }).error(function(errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            });

        return deferer.promise;
    }

    return {
        getAllCountry: getAllCountry,
        getCityByCountry: getCityByCountry,
        getCityByRegion: getCityByRegion,
        getRegionByCountry: getRegionByCountry,
    }
}])