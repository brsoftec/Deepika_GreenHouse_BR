var _notificationModule = getApp('NotificationModule', ['ui.bootstrap', 'angular-momentjs', 'UserModule']);

_notificationModule.factory('NotificationService', ['$q', '$http', '$interval', '$timeout', '$rootScope', 'AuthorizationService', 'rguNotify', function ($q, $http, $interval, $timeout, $rootScope, _authService, rguNotify) {
    var _numberOfUnreadNotifications = 0;

    var _countUnreadNotifications = function () {
        var deferer = $q.defer();

        $http.get('/Api/Notifications/CountUnread', {params: {hideAjaxLoader: true}}).success(function (count) {
            deferer.resolve(count)
        }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _triggerAutoCountUnread = false;
    var _intervalCountUnread = 5000;
    var _autoCountUnreadHandler = null;

    var _autoCountUnread = function (interval) {
        if (!_triggerAutoCountUnread) {
            _triggerAutoCountUnread = true;

            _countUnreadNotifications().then(function (count) {
                _numberOfUnreadNotifications = count;
            });

            var withMillisecond = typeof interval !== 'undefined' && interval != null && angular.isNumber(interval) ? interval : _intervalCountUnread;
            _autoCountUnreadHandler = $interval(function () {
                if (_authService.IsAuthorized()) {
                    _countUnreadNotifications().then(function (count) {
                        _numberOfUnreadNotifications = count;
                    });
                }
            }, withMillisecond);
        }
    }

    var _cancelAutoCountUnread = function () {
        if (_triggerAutoCountUnread) {
            _triggerAutoCountUnread = false;
            $interval.cancel(_autoCountUnreadHandler);
        }
    }

    var _getNotifications = function (start, length, createdBefore) {
        var deferer = $q.defer();
        $http.get('/Api/Notifications', {
            params: {
                start: start,
                length: length,
                createdBefore: createdBefore,
                hideAjaxLoader: true
            }
        })
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })
        return deferer.promise;
    }

    var _read = function (id) {
        var deferer = $q.defer();
        $http.post('/Api/Notifications/Read', {Id: id}, {params: {hideAjaxLoader: true}})
            .success(function () {
                _numberOfUnreadNotifications--;
                deferer.resolve();
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status);
        })
        return deferer.promise;
    }

    var _readAll = function () {
        var deferer = $q.defer();
        $http.post('/Api/Notifications/ReadAll', null, {params: {hideAjaxLoader: true}})
            .success(function () {
                _numberOfUnreadNotifications = 0;
                deferer.resolve();
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status);
        })
        return deferer.promise;
    }

    $rootScope.notifMessages = {};
    var _latestNotification = function () {
        var deferer = $q.defer();

        // $http.get('/Api/Notifications/CountUnreadByAccountId/' + regitGlobal.activeAccountId)
        //     .success(function (response) {
        //         console.log(response);
        //     }).error(function (errors, status) {
        // });
        //
        // var url = '/api/Notifications/GetNotifications/' + regitGlobal.activeAccountId;
        // $http.get(url, {params: {start: 0, take: 20, hideAjaxLoader: true}})
        //     .success(function (data) {
        //         console.log(data)
        //         if (angular.isArray(data)) {
        //             $rootScope.$broadcast('notifications:pulled', {
        //                 unreadCount: 0, //data.UnViewedNotifications,
        //                 notifications: data
        //             })
        //         } else return;
        //         if (true) {
        //             var count = data.UnViewedNotifications;
        //             if (count) {
        //                 var delay = 0;
        //                 for (var i = 0; i < count; i++) {
        //                     var notif = data.Notifications[i];
        //                     if (!notif) break;
        //                     var type = notif.Type;
        //                     if (/handshake/i.test(type)) {
        //                         // console.log(notif)
        //                         $rootScope.$broadcast('handshake:notification', {
        //                             notification: notif
        //                         })
        //                     }
        //                     var text = notif.Title;
        //                     if (!$rootScope.notifMessages.hasOwnProperty(text)) {
        //                         $timeout(function () {
        //                             rguNotify.add(text);
        //                         }, delay);
        //                         delay += 800;
        //                         count++;
        //                         $rootScope.notifMessages[text] = true;
        //                     }
        //                 }
        //                 $rootScope.$broadcast('notifications:new', {
        //                     count: count,
        //                     notifications: data.Notifications
        //                 })
        //             }
        //         }
        //         deferer.resolve(data);
        //     }).error(function (errors, status) {
        //     console.log('Error pulling notifications for account', regitGlobal.activeAccountId)
        //     //__promiseHandler.Error(errors, status);
        // });

        _notificationModule.run(['$templateCache', function ($templateCache) {
            $templateCache.put('notificationSummaryPopover.html', '<div class="notification-summary-container"><div class="container-header"><a ng-click="readAll()">Mark all as read</a><a>&middot;</a><a>Settings</a></div><div class="container-body"><div ng-class="{\'unread\': !notification.Read}" ng-repeat="notification in notifications" class="notification" ng-click="read(notification)"><div class="creator-avatar"><img ng-src="{{!notification.CreatorAvatar ? \'/Areas/User/Content/Images/no-pic.png\' : notification.CreatorAvatar}}" /></div><div class="notification-message"><span class="notification-creator">{{notification.Creator}}</span> {{notification.Message}}<br/><span class="notification-time" am-time-ago="notification.CreatedAt"></span></div></div></div><div class="container-footer"><a href="/User/Notifications">See more</a></div></div>');
        }]);
        return deferer.promise;
    }


    // Vu, Son fixed

    var _pullNotification = function (url, accountId, start, take) {
        var defer = $q.defer();

        $http.get(url + '/' + accountId, { params: { start: start, take: take, hideAjaxLoader2: true}}).success(function (response) {
            defer.resolve(response);
        }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, defer);
        });
        return defer.promise;
    };

    var _pullMoreNotification = _pullNotification;
    // End Vu


    return {
        GetNumberOfUnreadNotifications: function () {
            return _numberOfUnreadNotifications
        },
        TriggerAutoCountUnread: function (interval) {
            _autoCountUnread(interval);
        },
        StopAutoCountUnread: function () {
            _cancelAutoCountUnread();
        },
        GetNotifications: _getNotifications,
        Read: _read,
        ReadAll: _readAll,

        //Blue Code
        LatestNotification: _latestNotification,
        PullNotification: _pullNotification,
        PullMoreNotification: _pullMoreNotification
    }
}
]);

_notificationModule.getController('NotificationSummaryController', ['$scope', '$rootScope', 'NotificationService', function ($scope, $rootScope, _service) {
    _service.TriggerAutoCountUnread();

    $scope.getNumberOfUnreadNotifications = _service.GetNumberOfUnreadNotifications;

    $scope.notifications = [];

    $scope.start = 0;
    $scope.length = 5;
    $scope.total = 0;

    $scope.getNotifications = function () {
        _service.GetNotifications($scope.start, $scope.length)
            .then(function (response) {
                $scope.total = response.Total;
                $scope.notifications = response.Notifications;
            })
    }

    $scope.readAll = function () {
        _service.ReadAll().then(function () {
            for (var i = 0; i < $scope.notifications.length; i++) {
                $scope.notifications[i].Read = true;
            }
        });
    }

    $scope.read = function (notification) {
        if (!notification.Read) {
            _service.Read(notification.Id).then(function () {
                notification.Read = true;
            }).then(function () {
                window.location.href = notification.Url;
            })
        } else {
            window.location.href = notification.Url;
        }
    }
}])

_notificationModule.getController('NotificationController', ['$scope', '$rootScope', '$timeout', 'NotificationService', function ($scope, $rootScope, $timeout, _service) {
    $scope.notifications = [];

    $scope.start = 0;
    $scope.length = 20;
    $scope.queryTime = null;
    $scope.total = 0;

    $scope.loadingMore = false;

    $scope.noMore = false;

    $scope.getNotifications = function () {
        if (!$scope.loadingMore && !$scope.noMore) {
            $scope.loadingMore = true;
            _service.GetNotifications($scope.start, $scope.length, $scope.queryTime)
                .then(function (response) {
                    $scope.total = response.Total;
                    $scope.notifications = $scope.notifications.concat(response.Notifications);
                    $scope.queryTime = response.QueryTime;
                    $scope.start = $scope.notifications.length;
                    if (response.Notifications.length == 0) {
                        $scope.noMore = true;
                    }

                    $timeout(function () {
                        $scope.loadingMore = false;
                    }, 1000);
                }, function () {
                    $timeout(function () {
                        $scope.loadingMore = false;
                    }, 1000);
                })
        }
    }

    $scope.getNotifications();

    $scope.read = function (notification) {
        if (!notification.Read) {
            _service.Read(notification.Id).then(function () {
                notification.Read = true;
            }).then(function () {
                window.location.href = notification.Url;
            })
        } else {
            window.location.href = notification.Url;
        }
    }
}])