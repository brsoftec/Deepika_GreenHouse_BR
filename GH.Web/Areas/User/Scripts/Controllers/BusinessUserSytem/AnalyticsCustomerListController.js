


var myApp = getApp("myApp", ['ngSanitize', 'ngCookies', 'UserModule', 'oitozero.ngSweetAlert','SocialModule', 'CommonDirectives', 'ui.select', 'NotificationModule'], true);
myApp.getController('AnalyticsCustomerListController',
['$scope', '$rootScope', '$http', 'UserManagementService', 'SweetAlert', 'AuthorizationService', 'alertService',  'NotificationService','$moment','analyticsService','$timeout',
function ($scope, $rootScope, $http, userManager, sweetAlert, authService, alertService, notificationService, $moment,analyticsService,$timeout ) {

    $scope.business = {
        id: 'f09b2089-7928-4156-a5c0-d62aa473cef3',
        name: 'Sample Business'
    };
 
    $scope.master = true;

    $scope.view = {
        selectedView: 'Current Week',
        showKeywords: false,
        showUnknownGender: false
    };

    Highcharts.setOptions({
        chart: {
            style: {
                fontFamily: 'Dosis, "Helvetica Neue", Helvetica, Arial, Geneva, sans-serif'
            }
        }
    });

    var start = $moment().subtract(1, 'month').toDate();
    analyticsService.getUsers($scope.master ? $scope.business.id : $scope.interaction.id, start, $scope.master)
        .then(function (users) {
            $scope.users = users;
            $scope.dataLoaded = true;
            $scope.participatorsWeek = [];
            var startWeek = $moment().subtract(1, 'week');
            $.each($scope.users, function (index, user) {
                user.moment = $moment(user.datefollow);
                user.followed = user.moment.toDate();
                user.days = Math.floor(user.moment.unix() / (3600 * 24));

                if (!user.age && (user.dob_vault || user.dob)) {
                    var dob = $moment(user.dob_vault || user.dob, 'DD-MM-YYYY');
                    user.age = $moment().diff(dob, 'years');
                    user.dob = dob.toDate();
                }
                // user.gender = chance.pickone(['Male', 'Female', '']);
                user.firstname = user.firstname_vault || user.firstname;
                user.lastname = user.lastname_vault || user.lastname;
                user.country = user.country_vault || user.country;
                user.city = user.city_vault || user.city;

                var fields = [];
                angular.forEach(user.ListFieldsRegis, function (field) {
                    fields.push({
                        label: field.displayName,
                        value: field.model
                    });
                });
                user.fields = fields;

                if (user.moment.isAfter(startWeek)) {
                    $scope.participatorsWeek.push(user);
                }
            });
            $scope.updateChart();
        }

        , function (reason) {
            $scope.users = null;
            $scope.dataLoaded = true;
        });

    $scope.updateChart = function () {

        if (!$scope.users) return;
        var range = $scope.view.selectedView === 'Current Week' ? 'week' : 'month';

        $scope.participators = range === 'week' ? $scope.participatorsWeek : $scope.users;
        //  Populate data on date
        var moments = [];
        $.each($scope.participators, function (index, user) {
            moments.push(user.moment);
        });
        var from = $moment.min(moments), to = $moment.max(moments);
        var fromDay = Math.floor(from.unix() / (3600 * 24)), toDay = Math.floor(to.unix() / (3600 * 24));
        var numDays = toDay - fromDay + 1;
        var participatorsByDay = [];
        for (var day = 0; day < numDays; day++) {
            participatorsByDay[day] = [];
        }
        $.each($scope.participators, function (index, follower) {
            var day = follower.days - fromDay;
            participatorsByDay[day].push(follower);
        });

        var menByDay = [], womenByDay = [], unknownByDay = [];
        for (day = 0; day < numDays; day++) {
            menByDay[day] = 0;
            womenByDay[day] = 0;
            unknownByDay[day] = 0;
        }
        $.each(participatorsByDay, function (day, participators) {
            $.each(participators, function (index, follower) {
                if (follower.gender === 'Male') {
                    menByDay[day]++;
                } else if (follower.gender === 'Female') {
                    womenByDay[day]++;
                } else {
                    unknownByDay[day]++;
                }
            });
        });

        ;
        // Populate data on age ranges
        var ageRanges = [17, 24, 34, 44, 54, 64, 200];
        var menByAge = [], womenByAge = [], unknownByAge = [];
        for (i = 0; i < ageRanges.length; i++) {
            menByAge[i] = 0;
            womenByAge[i] = 0;
            unknownByAge[i] = 0;
        }
        $.each($scope.participators, function (index, follower) {
            var age = follower.age;
            $.each(ageRanges, function (index, range) {
                if (age <= range) {
                    if (follower.gender === 'Male') {
                        menByAge[index]++;
                    } else if (follower.gender === 'Female') {
                        womenByAge[index]++;
                    } else {
                        unknownByAge[index]++;
                    }
                    return false;
                }
            });
        });
        //  Extract data on location
        var countries = [];
        $.each($scope.participators, function (index, follower) {
            var country = follower.country;
            var city = follower.city;
            if (!countries.hasOwnProperty(country)) {
                countries[country] = {
                    count: 1,
                    cities: []
                }
            } else {
                countries[country].count++;
            }
            if (!countries[country].cities.hasOwnProperty(city)) {
                countries[country].cities[city] = 1;
            } else {
                countries[country].cities[city]++;
            }
        });
        countries.sort(function (a, b) {
            return b - a;
        });
        $scope.countries = $.extend({}, countries);
        $scope.currentCities = [];
        $scope.showCitiesFromCountry = function (country) {
            $scope.currentCities = $.extend({}, $scope.countries[country].cities);
        };

        var series = [
            {
                name: 'Women',
                data: womenByDay,
                pointStart: from.valueOf(),
                pointIntervalUnit: 'day'
            }, {
                name: 'Men',
                data: menByDay,
                pointStart: from.valueOf(),
                pointIntervalUnit: 'day',
                color: '#0084b6'
            }
        ];
        if ($scope.view.showUnknownGender) {
            series.push({
                name: 'Unknown',
                data: unknownByDay,
                pointStart: from.valueOf(),
                pointIntervalUnit: 'day'
            });
        }

        $scope.chartConfig = {
            title: {
                text: 'Interaction Participators'
            },
            subtitle: {
                text: $scope.view.selectedView
            },
            xAxis: {
                type: 'datetime'
            },
            yAxis: {
                title: { text: 'Number of Participators' },
                allowDecimals: false
            },
            options: {
                chart: {
                    type: 'areaspline'
                }
            },
            series: series,
            exporting: {
                buttons: {
                    contextButton: {
                        menuItems: [{
                            text: 'Export to PNG',
                            onclick: function () {
                                this.exportChartLocal({
                                    filename: 'chart-participators.pdf'
                                });
                            }
                        }, {
                            text: 'Export to PDF',
                            onclick: function () {
                                this.exportChart({
                                    type: 'application/pdf',
                                    filename: 'chart-participators.pdf'
                                });
                            },
                            separator: false
                        }]
                    }
                }
            }
        };
        var ageSeries = [{
            name: 'Women',
            data: womenByAge
        }, {
            name: 'Men',
            data: menByAge
            // color: '#0084b6'
        }];
        if ($scope.view.showUnknownGender) {
            ageSeries.push(
                {
                    name: 'Unknown',
                    data: unknownByAge
                });
        }
        $scope.agesChartConfig = {
            title: {
                text: 'Participators by Age'
            },
            subtitle: {
                text: $scope.view.selectedView
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
                        formatter: function () {
                            var pcnt = (this.y / $scope.participators.length) * 100;
                            return pcnt + '%23';
                        }
                    }
                }
            },

            series: ageSeries,

            xAxis: {
                title: { text: 'Age' },
                categories: ['1-17', '18-24', '25-34', '35-44', '45-54', '55-64', '65+']

            },
            yAxis: {
                title: { text: 'Partipators' },
                labels: {
                    formatter: function () {
                        var pcnt = Math.round(this.value / $scope.participators.length * 100);
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
                func: function (chart) {
                    //setup some logic for the chart
                }
            }
        };

        $scope.tags = analyticsService.getTagCloud();

        $scope.$watchCollection('view.showUnknownGender', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                $timeout(function () {
                    $scope.updateChart();
                });
            }
        });
    };
    $scope.exportData = function () {

        html2canvas(document.getElementById('interaction-analytics-charts')).then(function (canvas) {
            var img = canvas.toDataURL('image/png');
            var doc = new jsPDF('p', 'px', 'a4');
            doc.addImage(img, 'PNG', 20, 20);
            html2canvas(document.getElementById('interaction-analytics-tags')).then(function (canvas) {
                var img = canvas.toDataURL('image/png');
                doc.addPage();
                doc.addImage(img, 'PNG', 20, 20);
                doc.save('interaction-analytics.pdf');
            });

        });

    };


    


    }
]);

