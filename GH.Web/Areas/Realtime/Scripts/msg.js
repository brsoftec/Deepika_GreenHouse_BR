var msgModule = getApp('msg', ['angular-momentjs', 'windows', 'UserModule']);

msgModule.factory('msgService', ['$moment', 'NetworkService', function ($moment, networkService) {

    var users = null;
    var msg = {};
    msg.msgList = [];


    // get user from Database
    networkService.GetFriends().then(function (data) {
        if (data) {
            angular.forEach(data, function (friend) {
                var user = {
                    from: {
                        id: friend.Id,
                        name: friend.DisplayName,
                        avatar: friend.Avatar
                    },
                    messages: [
                        {
                            created: new Date(),
                            text: ''
                        }
                    ]
                };
                msg.msgList.unshift(user);
            });
        }
    },
    function (error) {
        var info = error;
    });


    $.connection.hub.start().done(function () {
    });
    var chat = $.connection.chatHub;
    chat.client.broadcastMessage = function (userId, message) {
        var comingMessage = {
            created: new Date(),
            fromMe: false,
            text: message
        };
        msg.addMessage(userId, comingMessage,null);
        $('#' + userId).click();
        //$('#0b786d782a9542ad8b74a8de0f1956d9').click();
    };
    

    function updateConversation(user) {
        angular.forEach(msg.msgList, function (user) {
            var messages = user.messages;
            user.latestMessage = messages[user.messages.length - 1];
            user.latestMessage.when = $moment(user.latestMessage.created).calendar(null, {
                sameDay: 'LT'
            }
            );
            delete user.unreadCount;
            var unreadCount = 0;
            var lastFromMe = 'new';
            angular.forEach(messages, function (msg) {
                if (msg.unread) {
                    unreadCount++;
                }
                if (lastFromMe !== 'new' && msg.fromMe === lastFromMe) {
                    msg.fromSame = true;
                }
                lastFromMe = msg.fromMe;
            });
            if (unreadCount) {
                user.unreadCount = unreadCount;
            }
        });
    }

    msg.markAllRead = function (id) {
        var user = msg.getConversationByUserId(id);
        if (user) {
            angular.forEach(user.messages, function (message) {
                delete message.unread;
            });
            updateConversation(user);
        }
    };

    //updateConversation(user);

    msg.getConversationByUserId = function (id) {
        var conversation;
        angular.forEach(msg.msgList, function (user) {
            if (user.from.id === id) {
                conversation = user;
            }
        });
        return conversation;
    };

    msg.addMessage = function (userId, message, callback) {
        var user = msg.getConversationByUserId(userId);
        if (user) {
            if (!message.fromMe) {
                message.unread = true;
            }
            user.messages.push(message);
            updateConversation(user);
        }
        if (callback && angular.isFunction(callback)) {
            callback.apply(null);
        }
    };

    return msg;
}])
    .directive('msgList', function () {
        return {
            template: '<div class="msg-list">'
            + '<div class="msg-list-row"  ng-repeat="user in msgList track by user.from.id" ng-init="" ng-class="{\'msg-list-row-unread\' : user.unreadCount}">'
                + '<div class="msg-list-avatar"><img src="{{user.from.avatar}}"></div>'
                    + '<div id="{{user.from.id}}" class="msg-list-summary" ng-click="openMsgBox($event,user,$index)">'
                        + '<div class="msg-list-when">{{user.latestMessage.when}}</div>'
                        + '<div class="msg-list-name">{{user.from.name}} '
                            + '<span class="msg-list-unread badge" ng-if="user.unreadCount">{{user.unreadCount}}</span>'
                        + '</div>'
                        + '<div class="msg-list-text">{{user.latestMessage.text}}</div>'
                        + '<div class="msg-list-row-read-indicator" ng-if="!user.unreadCount"></div>'
                    + '</div>'
                + '</div>'
            + '</div>',
            restrict: 'E',
            link: function (scope, element, attrs, windowCtrl) {

            },
            controller: function msgListController($scope, $element, $document, $compile, windowService) {
                $scope.openMsgBox = function (e, user, index) {
                    e.preventDefault();
                    var win = $document.find('.msg-list');
                    var x = win.position().left - 280, y = win.position().top;
                    var row = $element.find('.msg-list-row').eq(index);
                    y += row.position().top;
                    windowService.createWindow(user.from.id, { heading: user.from.name }, 'msg-box',
                        { left: x, top: y }, '.msg-container',
                        '<msg-conversation userid="' + user.from.id + '"></msg-conversation>');
                };
            }
        };
    })
    .directive('msgConversation', function (msgService) {
        return {
            // require: '^^msgList',
            restrict: 'E',
            scope: {
                userid: '@'

            },
            link: function (scope, element, attr, msgListCtrl) {
                var userId = attr.userid;
                msgService.markAllRead(userId);
                scope.conversation = msgService.getConversationByUserId(userId);
                scope.messages = scope.conversation.messages;
            },
            template: '<div class="msg-conversation">'
            + '<div class="msg-conversation-entry" ng-repeat="message in messages track by $index" ng-class="{\'msg-from-me\': message.fromMe}">'
            + '<div class="msg-conversation-avatar unselectable" ng-if="!message.fromMe"><img src="{{conversation.from.avatar}}" ng-if="!message.fromSame"></div>'
            + '<div class="msg-conversation-text" ng-bind="message.text" ng-show="message.text"></div>'
            + '</div></div>'
            + '<msg-input userid="{{userid}}"></msg-input>'
        };
    })
    .directive('msgInput', function (msgService) {
        return {
            restrict: 'E',
            scope: {
                userid: '@'
            },
            link: function (scope, element, attr) {
                scope.conversation = msgService.getConversationByUserId(attr.userid);
                scope.messages = scope.conversation.messages;
                var input = element.find('textarea');
                input.focus();
                input.on('keyup', function (e) {
                    if (!scope.inputMsgText.length) {
                        e.preventDefault();
                        scope.inputMsgText = '';
                        return false;
                    }
                    e = e || event;
                    if (e.keyCode === 13 && !e.shiftKey) {
                        // submit message
                        var message = {
                            fromMe: true,
                            created: new Date(),
                            text: scope.inputMsgText
                        };
                        scope.inputMsgText = '';
                        scope.$apply(function () {
                            msgService.addMessage(scope.userid, message, function () {
                                $.connection.chatHub.server.sendTo('' + scope.userid + '', '' + message.text + '');
                            });
                        });
                        e.preventDefault();
                        return true;
                    }
                    return false;
                });
            },
            template: '<div class="msg-conversation-input"><form class="form-msg-input">'
            + '<textarea class="input-msg" placeholder="Type a message..." ng-model="inputMsgText"></textarea>'
            + '</form></div>'
        };
    });



