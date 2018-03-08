var myApp = getApp('myApp');

myApp.getController("SocialPostsFilterController", ['$scope', '$rootScope', 'DashboardService', 'SweetAlert', '$q', '$location', '$httpParamSerializer', function ($scope, $rootScope, _dashboardService, _sweetAlert, $q, $location, $httpParamSerializer) {

    $scope.privacies = [{ Value: 'Global', Display: $rootScope.translate('Public') }, { Value: 'Personal', Display: $rootScope.translate('Private') }];

    $scope.dateRanges = [
        {
            Display: $rootScope.translate('All'),
            Value: null
        },
        {
            Display: $rootScope.translate('Last_10_days'),
            Value: 'Last-10-days'
        },
        {
            Display: $rootScope.translate('This_month'),
            Value: 'This-month'
        }
    ]

    $scope.filters = {
        Privacy: $scope.privacies[0],
        DateRange: $scope.dateRanges[0],
        Regit: true,
        Facebook: true,
        Twitter: true,
        Keyword: null
    }

    $scope.selectPrivacy = function (privacy) {
        $scope.filters.Privacy = privacy;
    }

    $scope.selectDateRange = function (date) {
        $scope.filters.DateRange = date;
    }

    $rootScope.refillFilter = function (criteria) {
        $scope.filters.Keyword = criteria.Keyword;
        if (criteria.Privacy && criteria.Privacy.toLowerCase() == 'personal') {
            $scope.filters.Privacy = $scope.privacies[1];
        }
        $scope.filters.Regit = criteria.Regit;
        $scope.filters.Facebook = criteria.Facebook;
        $scope.filters.Twitter = criteria.Twitter;
        if (criteria.For) {
            for (var i = 0; i < $scope.dateRanges.length; i++) {
                if ($scope.dateRanges[i].Value && criteria.For.toLowerCase() == $scope.dateRanges[i].Value.toLowerCase()) {
                    $scope.filters.DateRange = $scope.dateRanges[i];
                    break;
                }
            }
        } else {
            $scope.filters.DateRange = $scope.dateRanges[0];
        }
    }

    $scope.search = function () {
        if (!$scope.filters.Twitter && !$scope.filters.Facebook && !$scope.filters.Regit) {
            __common.swal(_sweetAlert, $rootScope.translate('Invalid_filters'), $rootScope.translate('Please_choose_at_least_one_social_network'), 'warning');
            searching = false;
            return;
        }

        if (!$scope.filters.Keyword) {
            __common.swal(_sweetAlert, $rootScope.translate('Invalid_filters'), $rootScope.translate('Please_type_keyword_to_search'), 'warning');
            searching = false;
            return;
        }

        var criteria = {
            Keyword: $scope.filters.Keyword,
            Regit: $scope.filters.Regit,
            Facebook: $scope.filters.Facebook,
            Twitter: $scope.filters.Twitter,
            Privacy: $scope.filters.Privacy.Value,
            For: $scope.filters.DateRange.Value
        }

        var q = $httpParamSerializer(criteria);

        if (window.location.pathname.toLowerCase().startsWith('/businessaccount')) {
            window.location.href = window.location.pathname.substr(0, 16) + '#/search?' + q;
        } else {
            window.location.href = '/#/search?' + q;
        }
    }
}])