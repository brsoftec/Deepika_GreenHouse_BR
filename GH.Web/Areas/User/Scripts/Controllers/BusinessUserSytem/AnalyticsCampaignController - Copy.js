

var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert', 'SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.getController('AnalyticsCampaignController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService', 'NotificationService', 'CommonService',
function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, notificationService, CommonService) {

    //var vm = this;

    $scope.initializeController = function () {

        var data = new Object();
        data.CampaignId = CommonService.GetQuerystring("CampaignIdValue");
        data.CampaignName = CommonService.GetQuerystring("CampaignNameValue");

        $http.post("/api/BusinessUserSystemService/GetUserListByCampaignForChart", data)
           .success(function (response) {
               var list = JSON.stringify(response.FollowerList);

                if (response.FollowerList.length < 0) {
                    $scope.DisplayChart = false;
                }
                else {
                    $scope.DisplayChart = true;
                    $scope.CampaignName = response.CampaignName;
                    var followers = [];
                    //  Generate sample data
                    var numFollowers = 1000;
                    var sampleCountries = [
                        {
                            name: 'Vietnam',
                            cities: ['Ho Chi Minh', 'Hanoi', 'Da Nang', 'Hue', 'Hai Phong', 'Can Tho']
                        },
                        {
                            name: 'Singapore',
                            cities: ['Singapore', 'Alexandra', 'Ama Keng', 'Banla Tengeh', 'Boon Lay', 'Bedoc']
                        },
                        {
                            name: 'Canada',
                            cities: ['Ottawa', 'Edmonton', 'Victoria', 'Toronto', 'Quebec City']
                        }
                    ];
                    var from1 = moment('2016-07-13');
                    for (var i = 0; i < numFollowers; i++) {
                        var days = Math.floor(Math.random() * 7);
                        var when = moment(from1);
                        when.add(days, 'days');
                        var country = sampleCountries[Math.floor(Math.random() * sampleCountries.length)];
                        var follower = {
                            Gender: Math.random() < .5 ? 'Male' : 'Female',
                            Age: Math.floor(Math.random() * 100),
                            CountryName: country.name,
                            CityName: country.cities[Math.floor(Math.random() * country.cities.length)],
                            FollowedDate: when.format()
                        };
                        followers.push(follower);
                    }

                    //followers = followers.concat(response.FollowerList);
                    followers = response.FollowerList;

                    $scope.followers = followers;

                    //  Populate data on date
                    var moments = [];

                    $.each(followers, function(index, follower) {
                        var dateStr = follower.FollowedDate.substr(0, 10);
                        follower.moment = moment(dateStr);
                        follower.days = Math.floor(follower.moment.unix() / (3600 * 24));
                        moments.push(follower.moment);
                    });
                    var from = moment.min(moments), to = moment.max(moments);
                    var fromDay = Math.floor(from.unix() / (3600 * 24)), toDay = Math.floor(to.unix() / (3600 * 24));
                    var numDays = toDay - fromDay + 1;
                    var followersByDay = [];
                    for (var day = 0; day < numDays; day++) {
                        followersByDay[day] = [];
                    }

                    $.each(followers, function(index, follower) {
                        var day = follower.days - fromDay;
                        followersByDay[day].push(follower);
                    });

                    var menByDay = [], womenByDay = [];
                    for (day = 0; day < numDays; day++) {
                        menByDay[day] = 0;
                        womenByDay[day] = 0;
                    }
                    $.each(followersByDay, function(day, followers) {
                        $.map(followers, function(follower, index) {
                            if (follower.Gender === 'Male') {
                                menByDay[day]++;
                            } else {
                                womenByDay[day]++;
                            }
                        });
                    });

                    // Populate data on age ranges
                    var ageRanges = [17, 24, 34, 44, 54, 64, 200];
                    var menByAge = [], womenByAge = [];
                    for (var i = 0; i < ageRanges.length; i++) {
                        menByAge[i] = 0;
                        womenByAge[i] = 0;
                    }
                    $.each(followers, function(index, follower) {
                        var age = follower.Age;
                        $.each(ageRanges, function(index, range) {
                            if (age <= range) {
                                if (follower.Gender === 'Male') {
                                    menByAge[index]++;
                                } else {
                                    womenByAge[index]++;
                                }
                                return false;
                            }
                        });
                    });
                    //  Calcucale percentage
                    /*
                     menByAge = $.map(menByAge, function (range, index) {
                     return Math.round(range / followers.length * 100);
                     });
                     womenByAge = $.map(menByAge, function (range, index) {
                     return Math.round(range / followers.length * 100);
                     });
                     */
                    //  Extract data on location
                    var countries = [];
                    $.each(followers, function(index, follower) {
                        var country = follower.CountryName;
                        var city = follower.CityName;
                        if ((!countries.hasOwnProperty(country))) {
                            countries[country] = {
                                count: 1,
                                cities: []
                            }
                        } else {
                            countries[country].count++;
                        }
                        if (!(countries[country].cities.hasOwnProperty(city))) {
                            countries[country].cities[city] = 1;
                        } else {
                            countries[country].cities[city]++;
                        }
                    });
                    countries.sort(function(a, b) {
                        return b - a;
                    });
                    $scope.countries = $.extend({}, countries);
                    delete $scope.countries.remove;
                    delete $scope.countries.findItem;
                    $scope.currentCities = [];
                    $scope.showCitiesFromCountry = function(country) {
                        $scope.currentCities = $.extend({}, $scope.countries[country].cities);
                        delete $scope.currentCities.remove;
                        delete $scope.currentCities.findItem;
                    };

                    Highcharts.setOptions({
                        chart: {
                            style: {
                                fontFamily: 'Dosis, "Helvetica Neue", Helvetica, Arial, Geneva, sans-serif'
                            }
                        }
                    });
                    $scope.chartConfig = {
                        title: {
                            text: 'Followers'
                        },
                        subtitle: {
                            text: 'Current week'
                        },
                        xAxis: {
                            type: 'datetime'
                        },
                        yAxis: {
                            title: { text: 'Number of Followers' },
                            allowDecimals: false
                        },
                        options: {
                            chart: {
                                type: 'areaspline'
                            }
                        },
                        series: [
                            {
                                name: 'Female',
                                data: womenByDay,
                                pointStart: from.valueOf(),
                                pointIntervalUnit: 'day'
                            }, {
                                name: 'Male',
                                data: menByDay,
                                pointStart: from.valueOf(),
                                pointIntervalUnit: 'day',
                                color: '#0084b6'
                            }
                        ]
                    };

                    $scope.agesChartConfig = {
                        title: {
                            text: 'Followers by Age'
                        },
                        subtitle: {
                            text: 'Current week'
                        },
                        options: {
                            chart: {
                                type: 'column'
                            }

                        },
                        plotOptions: {
                            column: {
                                dataLabels: {
                                    enabled: true
                                }
                            },
                            series: {
                                dataLabels: {
                                    enabled: true,
                                    formatter: function() {
                                        var pcnt = (this.y / $scope.followers.length) * 100;
                                        return pcnt + '%23';
                                    }
                                }
                            }
                        },

                        series: [
                            {
                                name: 'Female',
                                data: womenByAge
                            }, {
                                name: 'Male',
                                data: menByAge,
                                color: '#0084b6'
                            }
                        ],

                        xAxis: {
                            title: { text: 'Age' },
                            categories: ['1-17', '18-24', '25-34', '35-44', '45-54', '55-64', '65+']

                        },
                        yAxis: {
                            title: { text: 'Followers' },
                            labels: {
                                formatter: function() {
                                    var pcnt = Math.round(this.value / followers.length * 100);
                                    return pcnt + '%';
                                }
                            },
                            allowDecimals: false,

                            // tickInterval: 1,
                            //size (optional) if left out the chart will default to size of the div or something sensible.
                            size: {
                                // width: 400,
                                height: 300
                            },
                            //function (optional)
                            func: function(chart) {
                                //setup some logic for the chart
                            }
                        }
                    };
                }
            })
      .error(function (errors, status) {

          //__promiseHandler.Error(errors, status, deferer);
      });
    };

   
}
]);

