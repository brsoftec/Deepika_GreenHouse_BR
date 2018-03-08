angular.module('analytics', ['angular-momentjs'])

    .factory('analyticsService', function ($http, $q, $moment) {
        var apiUrl = "/api/BusinessUserSystemService/GetChartDataByCampaign";
        var apiUrlBus = "/api/BusinessUserSystemService/GetChartDataByBus";
        return {
            getUsers: function (id, start, byBusiness) {
                var users = [];
                var deferred = $q.defer();
                $http({
                    method: 'POST',
                    url:  byBusiness ? apiUrlBus : apiUrl,
                    data: byBusiness ? {
                        BusId: id,
                        Startdate: $moment(start).format('YYYY-MM-DD')
                    } : {
                        CamapignId: id,
                        Startdate: $moment(start).format('YYYY-MM-DD')
                    }
                })
                    .then(function (response) {
                        users = response.data.Data;
                        deferred.resolve(users);
                    }, function (response) {
                        deferred.reject(response);
                    });
                return deferred.promise;
            },

            getTagCloud: function (interactionId) {
                //  Generate random user tags
                var count = chance.natural({
                    min: 40,
                    max: 70
                });
                var tags = [];
                for (var i = 0; i < count; i++) {
                    tags.push({
                        text: chance.word(),
                        weight: chance.integer({
                            min: 1,
                            max: 100
                        })
                    });
                }
                return tags;
            }
        }
    })
    .filter('shortenId', function () {
        return function (id) {
            return id.substring(0, 8) + '...';
        };
    })
    .filter('gridValue', function ($moment) {
        return function (value) {
            if (!angular.isDefined(value))
                return '';
            if (/^\d{1,2}-\d{1,2}-\d\d\d\d$/.test(value)) {
                value = $moment(value, 'D-M-YYYY').toDate();
            } else if (angular.isDate(value))
                return $moment(value).format('D-M-YYYY');
            if (angular.isArray(value))
                return value.join(', ');
            if (angular.isObject(value))
                return ''; //<span class="vault-value-meta">(Object)</span>'
            return value.toString();
        };
    });



