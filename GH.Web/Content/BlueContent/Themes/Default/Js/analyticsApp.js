var appController = function ($scope) {
    $scope.demoDate = new Date();
    $scope.dateFormat = 'mm-dd-yyyy';
    $scope.dateOptions = {showWeeks: false};
    $scope.datePicker = {opened: false};
    $scope.openDatePicker = function () {
        $scope.datePicker.opened = true;
    };
    $scope.genderModel = 'All';
    $scope.popoverNotifications = {
        content: 'Hello, World!',
        templateUrl: 'popover.notifications.html',
        title: 'Notifications'
    };
    $scope.openNotifications = function ($event) {
        $event.preventDefault();

    };

};
var analyticsApp = angular.module("analyticsApp", ['ui.bootstrap', 'highcharts-ng']);

analyticsApp.controller("AppCtrl", appController);
analyticsApp.controller("AnalyticsCtrl", function ($scope) {

    // var colors = Highcharts.getOptions().colors;
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
            title: {text: 'Number of Followers'}
        },
        series: [
            {
                type: 'areaspline',
                name: 'Women',
                data: [65, 59, 80, 81, 56, 55, 40, 60],
                pointStart: Date.UTC(2016, 5, 11),
                pointIntervalUnit: 'day'
            }, {
                type: 'areaspline',
                name: 'Men',
                data: [28, 48, 40, 19, 86, 27, 90, 20],
                pointStart: Date.UTC(2016, 5, 11),
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

        series: [{
            name: 'Women',
            data: [65, 59, 80, 81, 56, 55, 40, 60]
        }, {
            name: 'Men',
            data: [28, 48, 40, 19, 86, 27, 90, 20],
            color: '#0084b6'
        }],

        xAxis: {
            title: {text: 'Age'},
            categories: ['13-17', '18-24', '25-34', '35-44', '45-54', '55-64', '65+']
        },
        yAxis: {
            title: {text: 'Followers'},
            labels: {
                formatter: function() {
                    return this.value+"%";
                }
            }
        },


        //size (optional) if left out the chart will default to size of the div or something sensible.
                size: {
         // width: 400,
         height: 300
         },
        //function (optional)
        func: function (chart) {
            //setup some logic for the chart
        }
    };

});

