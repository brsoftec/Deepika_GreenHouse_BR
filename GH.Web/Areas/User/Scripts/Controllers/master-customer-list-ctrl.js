var myApp = getApp("myApp", ['TranslationModule', 'UserModule', 'SocialModule', 'CommonDirectives', 'oitozero.ngSweetAlert', 'ui.select', 'ui.bootstrap', 'BusinessAccountModule', 'NotificationModule', 'chart.js'], true);

myApp.getController('MasterCustomerListController', ['$scope', '$rootScope', '$http', '$filter', 'BusinessAccountService', function ($scope, $rootScope, $http, $filter, _baService) {
    var statTo = new Date();
    var statFrom = new Date();
    statFrom.setDate(statFrom.getDate() - 6);

    $scope.followersByTime = {
        Raw: {},
        Series: ['Following Area'],
        Data: [],
        Labels: [],
        Options: {
            scales: {
                yAxes: [{
                    ticks: {
                        min: 0,
                        suggestedMax: 100,
                        maxTicksLimit: 5
                    }
                }]
            }
        },
        Colors: ['#B6E8FC'],
        DatasetOverrided: [
            {
                lineTension: 0
            }
        ]
    };
    _baService.SummarizeNumberOfFollowersByTime(statFrom, statTo).then(function (response) {
        $scope.followersByTime.Raw = response;
        var data = [];
        angular.forEach(response.FollowersByDate, function (stat) {
            data.push(stat.Followers);
            $scope.followersByTime.Labels.push($filter('date')(stat.From, 'dd/MM/yyyy'))
        })
        $scope.followersByTime.Data.push(data);
    })

    $scope.followersByGenders = {
        Raw: {},
        Series: [],
        Data: [],
        Labels: [],
        Options: {
            legend: {
                display: true,
                position: 'bottom'
            },
            scales: {
                yAxes: [{
                    ticks: {
                        callback: function (label, index, labels) {
                            return label + '%';
                        },
                        min: 0,
                        suggestedMax: 10,
                        maxTicksLimit: 5
                    }
                }]
            },
            tooltips: {
                callbacks: {
                    label: function (tooltipItem, data) {
                        var legend = data.datasets[tooltipItem.datasetIndex].label;
                        return legend + ': ' + tooltipItem.yLabel + '%';
                    }
                }
            }
        },
        Colors: ['#00C0F3', '#0084B6', '#D6D6D6']
    };
    _baService.SummarizeNumberOfFollowersByGenders().then(function (response) {
        $scope.followersByGenders.Raw = response;

        var totalFollowers = 0;

        for (var i = 0; i < response.length; i++) {
            totalFollowers += response[i].Followers;
        }

        angular.forEach(response, function (stat) {
            if (stat.Gender) {
                $scope.followersByGenders.Series.push(stat.Gender);
            } else {
                $scope.followersByGenders.Series.push('Unknown');
            }
            var data = [];

            angular.forEach(stat.FollowersByAge, function (byAge, index) {
                var label = 'Unknown';
                if (byAge.FromAge != null || byAge.ToAge != null) {
                    if (byAge.FromAge != null && byAge.ToAge != null) {
                        label = byAge.FromAge + '-' + byAge.ToAge;
                    } else if (byAge.FromAge == null) {
                        label = '0-' + byAge.ToAge;
                    } else {
                        label = byAge.FromAge + '+';
                    }
                }
                $scope.followersByGenders.Labels[index] = label;
                data.push(parseFloat((byAge.Followers / totalFollowers * 100).toFixed(2)));
            })
            $scope.followersByGenders.Data.push(data);
        })
    })

    $scope.followersByCountries = [];
    _baService.SummarizeNumberOfFollowersByCountries().then(function (response) {
        $scope.followersByCountries = response;
    })

    $scope.followersByCities = [];
    _baService.SummarizeNumberOfFollowersByCities().then(function (response) {
        $scope.followersByCities = response;
    })
}])
