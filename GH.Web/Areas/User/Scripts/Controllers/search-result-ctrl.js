var myApp = getApp("myApp");

myApp.getController("SearchResultController", ['$scope', '$rootScope', '$http', 'DashboardService', '$sce', '$location', 'SweetAlert', function ($scope, $rootScope, $http, _dashboardService, $sce, $location, _sweetAlert) {
    var query = $location.search();

    $scope.dateRanges = [
        {
            From: function () {
                return null;
            },
            To: function () {
                return null;
            },
            Display: 'All',
            Value: null
        },
        {
            From: function () {
                var date = new Date();
                date = new Date(date.getFullYear(), date.getMonth(), date.getDate());
                var lastTenDate = new Date(date.getTime());
                lastTenDate.setDate(date.getDate() - 10);
                return lastTenDate;
            },
            To: function () {
                return null;
            },
            Display: 'Last 10 days',
            Value: 'Last-10-days'
        },
        {
            From: function () {
                var date = new Date();
                return new Date(date.getFullYear(), date.getMonth(), 1);
            },
            To: function () {
                return null;
            },
            Display: 'This month',
            Value: 'This-month'
        }
    ]

    if (query.For) {
        for (var i = 0; i < $scope.dateRanges.length; i++) {
            if ($scope.dateRanges[i].Value && query.For.toLowerCase() == $scope.dateRanges[i].Value.toLowerCase()) {
                query.For = $scope.dateRanges[i].Value;
                query.From = $scope.dateRanges[i].From();
                query.To = $scope.dateRanges[i].To();
                break;
            }
        }
    }

    $rootScope.refillFilter(query);

    if (angular.equals(query, {})) {
        $location.path('/');
        return;
    }

    query.SearchBaseTime == new Date();

    var length = 2;
    var loadMoreLength = 10;

    $scope.globalPosts = [];
    $scope.searchGlobalPosts = function (loadMore) {
        var criteria = {
            Keyword: query.Keyword,
            Regit: query.Regit,
            Facebook: query.Facebook,
            Twitter: query.Twitter,
            From: query.From,
            To: query.To,
            Start: 0,
            Length: length,
            SearchBaseTime: query.SearchBaseTime
        }

        if (loadMore) {
            criteria.Start = $scope.globalPosts.length;
            criteria.Length = loadMoreLength;
        } else {
            $scope.globalPosts = [];
        }

        return _dashboardService.SearchGlobalPosts(criteria).then(function (posts) {
            angular.forEach(posts, function (post) {
                post.Message = post.Message == null ? '' : post.Message;
                post.Message = $sce.trustAsHtml(post.Message.replace(/(?:\r\n|\r|\n)/g, '<br />'));
            })
            $scope.globalPosts = $scope.globalPosts.concat(posts);
            return posts.length;
        })
    }

    $scope.personalPosts = [];
    $scope.searchPersonalPosts = function (loadMore) {
        var criteria = {
            Keyword: query.Keyword,
            Regit: query.Regit,
            Facebook: query.Facebook,
            Twitter: query.Twitter,
            From: query.From,
            To: query.To,
            Start: 0,
            Length: length,
            SearchBaseTime: query.SearchBaseTime
        }

        if (loadMore) {
            criteria.Start = $scope.personalPosts.length;
            criteria.Length = loadMoreLength;
        } else {
            $scope.personalPosts = [];
        }

        return _dashboardService.SearchPersonalPosts(criteria).then(function (posts) {
            angular.forEach(posts, function (post) {
                post.Message = post.Message == null ? '' : post.Message;
                post.Message = $sce.trustAsHtml(post.Message.replace(/(?:\r\n|\r|\n)/g, '<br />'));
            })
            $scope.personalPosts = $scope.personalPosts.concat(posts);
            return posts.length;
        })
    }

    $scope.globalUsers = [];
    $scope.searchGlobalUsers = function (loadMore) {
        var criteria = {
            Keyword: query.Keyword,
            Start: 0,
            Length: length,
            SearchBaseTime: query.SearchBaseTime
        }

        if (loadMore) {
            criteria.Start = $scope.globalUsers.length;
            criteria.Length = loadMoreLength;
        } else {
            $scope.globalUsers = [];
        }

        return _dashboardService.SearchGlobalUsers(criteria).then(function (users) {
            $scope.globalUsers = $scope.globalUsers.concat(users);
            return users.length;
        })
    }

    $scope.personalUsers = [];
    $scope.searchPersonalUsers = function (loadMore) {
        var criteria = {
            Keyword: query.Keyword,
            Start: 0,
            Length: length,
            SearchBaseTime: query.SearchBaseTime
        }

        if (loadMore) {
            criteria.Start = $scope.personalUsers.length;
            criteria.Length = loadMoreLength;
        } else {
            $scope.personalUsers = [];
        }

        return _dashboardService.SearchPersonalUsers(criteria).then(function (users) {
            $scope.personalUsers = $scope.personalUsers.concat(users);
            return users.length;
        })
    }

    if (query.Privacy == 'Personal') {
        $scope.searchPersonalPosts(false);
        $scope.searchPersonalUsers(false);
    } else {
        $scope.searchPersonalPosts(false);
        $scope.searchPersonalUsers(false);
        $scope.searchGlobalPosts(false);
        $scope.searchGlobalUsers(false);
    }

    $scope.showSocialIcon = function (code, post) {
        if (post.Types.indexOf(code) !== -1) {
            return true;
        }
        return false;
    }

    $scope.ShowProfile = function (account) {
        if (account.AccountType == 'Business') {
            window.location = '/BusinessAccount/Profile/' + account.Id;
        } else {
            window.location = '/user/profile/' + account.Id;
        }

    }

}]);
