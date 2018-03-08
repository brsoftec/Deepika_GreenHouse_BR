angular.module('msg', ['angular-momentjs', 'users', 'windows', 'regit.ui'] )
    .factory('msgService', function ($rootScope, $moment, userService) {
        var msg = {};
        var conversations = [
            {
                id: '01',
                name: 'Tung Nguyen',
                from: {
                    id: '01',
                    name: 'Tung Nguyen',
                    avatar: '/Areas/regitUI/img/avatars/1.jpg',
                    online: true
                },
                messages: [
                    {
                        created: new Date(),
                        text: 'Hello, how is it going?',
                        read: new Date()
                    },
                    {
                        created: new Date(),
                        fromMe: true,
                        text: 'Everything OK',
                        read: new Date()
                    },
                    {
                        created: new Date(),
                        text: 'Awesome',
                        read: new Date()
                    },
                    {
                        created: new Date(),
                        text: 'How bout this weekend',
                        read: new Date()
                    },
                    {
                        created: new Date(),
                        text: 'Can\'t wait'
                    },
                    {
                        created: new Date(),
                        fromMe: true,
                        text: 'I know, maybe eat out before going to drink sthing'
                    },
                    {
                        created: new Date(),
                        text: 'Ok let\'s do it'
                    },
                    {
                        created: new Date(),
                        fromMe: true,
                        text: 'Cool!',
                        read: new Date()
                    }
                ]
            },
            {
                id: '02',
                name: 'Ngoc Tran',
                from: {
                    id: '02',
                    name: 'Ngoc Tran',
                    avatar: '/Areas/regitUI/img/avatars/2.jpg'
                },
                messages: [
                    {
                        created: new Date(),
                        text: 'Hello, how is it going?',
                        read: new Date()
                    },
                    {
                        created: new Date(),
                        fromMe: true,
                        text: 'Everything fine'
                    },
                    {
                        created: new Date(),
                        text: 'Awesome',
                        read: new Date()
                    },
                    {
                        created: new Date(),
                        text: 'How bout this weekend',
                        read: new Date()
                    },
                    {
                        created: new Date(),
                        text: 'Can\'t wait',
                        read: new Date()
                    },
                    {
                        created: new Date(),
                        fromMe: true,
                        text: 'I know, maybe eat out before going to drink sthing'
                    },
                    {
                        created: new Date(),
                        text: 'Great see ya'
                    }

                ]
            }
        ];
        //  Generate random sample conversations
        for (var i = 3; i < 10; i++) {
            var userName = 'Friend #' + i;
            var conversation = {
                id: '0' + i,
                name: userName,
                from: {
                    id: '0' + i,
                    name: userName,
                    avatar: '/Areas/regitUI/img/avatars/' + i + '.jpg'
                },
                messages: [
                    {
                        created: new Date(),
                        text: 'What\'s up?',
                        read: new Date()
                    }
                ]
            };
            conversations.push(conversation);
        }

        msg.getConversations = function () {
            return conversations;
        };

        msg.getSettingsMenu = function (windowType) {
            if (windowType === 'MsgList') {
                return {
                    items: [{
                        name: 'msg.newGroupChat',
                        label: 'New group chat'
                    }, {
                        name: 'msg.disableMessaging',
                        label: 'Turn off messaging'
                    }
                    ]
                };

            }
            if (windowType === 'MsgBox') {
                return {
                    items: [
                        {
                            name: 'msg.addPeopleToConversation',
                            label: 'Add/Remove people'
                        },
                        {
                            name: 'msg.deleteConversation',
                            label: 'Delete conversation'
                        },
                        {
                            name: 'msg.reportAbuseInConversation',
                            label: 'Report abuse'
                        }
                    ]
                };
            }
            if (windowType === 'MsgBoxGroup') {
                return {
                    items: [
                        {
                            name: 'msg.addPeopleToConversation',
                            label: 'Add/Remove people'
                        },
                        {
                            name: 'msg.manageConversation',
                            label: 'Manage conversation'
                        },
                        {
                            name: 'msg.deleteConversation',
                            label: 'Delete conversation'
                        },
                        {
                            name: 'msg.leaveConversation',
                            label: 'Leave conversation'
                        },
                        {
                            name: 'msg.reportAbuseInConversation',
                            label: 'Report abuse'
                        }
                    ]
                };
            }
        };

        msg.getUserByUserId = function (id) {
            var user;
            angular.forEach(conversations, function (conversation) {
                if (conversation.from.id === id) {
                    user = conversation.from;
                }
            });
            return user;
        };

        msg.getConversationById = function (id) {
            var found;
            angular.forEach(conversations, function (conversation) {
                if (conversation.id === id) {
                    found = conversation;
                }
            });
            return found;
        };

        function updateConversations() {
            angular.forEach(conversations, function (conversation) {
                var messages = conversation.messages;
                conversation.latestMessage = messages[conversation.messages.length - 1];
                conversation.latestMessage.when = $moment(conversation.latestMessage.created).calendar(null, {
                        sameDay: 'LT'
                    }
                );
                delete conversation.unreadCount;
                var unreadCount = 0;
                var lastFromMe = 'new';
                angular.forEach(messages, function (msg) {
                    if (msg.fromMe && !msg.read) {
                        unreadCount++;
                    }
                    if (lastFromMe !== 'new' && msg.fromMe === lastFromMe) {
                        msg.fromSame = true;
                    }
                    lastFromMe = msg.fromMe;
                });
                if (unreadCount) {
                    conversation.unreadCount = unreadCount;
                }
            });
        }

        updateConversations();

        msg.markAllRead = function (conversationId) {
            var conversation = msg.getConversationById(conversationId);
            if (conversation) {
                angular.forEach(conversation.messages, function (message) {
                    if (!message.read) {
                        message.read = new Date();
                    }
                });
                updateConversations();
            }
        };

        msg.addMessage = function (conversationId, message, callback) {
            var conversation = msg.getConversationById(conversationId);
            if (conversation) {
                if (!message.created) {
                    message.created = new Date();
                }
                conversation.messages.push(message);
                updateConversations();
            }
            if (callback && angular.isFunction(callback)) {
                var result = {success: true};
                callback.call(null, result);
            }
        };

        msg.fetchUsers = function (network) {
            // return userService.getUsers(network);
            var users = [];
            if (network === 'friends') {
                angular.forEach(conversations, function (conversation) {
                    var user = conversation.from;
                    if ($.inArray(user,users < 0)) {
                        users.push(user);
                    }
                });
            }
            return users;
        };

        msg.newConversation = function (users, name, isGroupChat, save) {
            isGroupChat = isGroupChat || !save || users.length > 1;
            if (!name) {
                name = users[0].name;
                if (isGroupChat) {
                    name += ' and ' + (users.length - 1) + ' more'
                }
            }
            var conversation = {
                id: Date.now().toString(),
                created: new Date(),
                isGroupChat: isGroupChat,
                name: name,
                owner: userService.getCurrentUser(),
                participators: users,
                messages: []
            };
            //  Generate sample message from each participator
            angular.forEach(users, function (user) {
                var message = {
                    from: user,
                    created: new Date(),
                    text: 'Hi, I\'m ' + user.name + '. I\'m in!'
                };
                conversation.messages.push(message);
            });
            if (save) {
                conversations.push(conversation);
            }

            return conversation;
        };
        msg.updateConversation = function (conversationId, users, name) {

            var conversation = msg.getConversationById(conversationId);
            if (!conversation) return null;
            var isGroupChat = conversation.isGroupChat || users.length > 1;

            if (!name) {
                name = users[0].name;
                if (isGroupChat) {
                    name += ' and ' + (users.length - 1) + ' more'
                }
            }

            conversation.isGroupChat = isGroupChat;
            conversation.name = name;
            conversation.participators = users;
            return conversation;
        };

        return msg;
    })
    .directive('msgList', function ($document, $sce, userService, msgService, windowService, rguModal) {
        return {
            restrict: 'E',
            templateUrl: RegitTemplatePath + '/msg-list.html',
            require: '^^window',
            scope: {
                type: '@'
            },
            link: function (scope, element, attrs, windowCtrl) {
                windowCtrl.addSettingsCommand('msg.newGroupChat', function () { /* Not implemented */
                });
                scope.me = userService.getCurrentUser();
                scope.$on('window.selectSettingsMenuItem', function (event, cmd) {

                    event.preventDefault();
                    if (scope.type === 'friends') return;
                    if (cmd.name === 'msg.newGroupChat') {
                        scope.participators = [];
                        scope.conversation = msgService.newConversation(scope.participators, 'New group chat', false);
                        rguModal.openModal('msg.manageConversation', scope, {newGroupChat: true});
                    }
                });
                scope.$on('msg.openConversation', function (event, conversationId) {
                    event.preventDefault();
                    if (scope.type === 'groups') return;
                    var conversation = msgService.getConversationById(conversationId);
                    scope.openMsgBox(conversation, 0);
                });
                scope.filterConversations = function (conversation) {
                    var type = attrs.type;
                    if (!type || type === 'friends') {
                        return !conversation.isGroupChat;
                    } else if (type === 'groups') {
                        return conversation.isGroupChat;
                    }
                    return false;
                };
                scope.openMsgBox = function (conversation, index) {
                    var user = conversation.from;
                    var win = element; //$document.find('.msg-list');
                    var x = win.position().left - 310, y = win.position().top;
                    var row = $document.find('.msg-list-row').eq(index);
                    y += row.position().top;

                    windowService.createWindow(conversation.isGroupChat ? 'MsgBoxGroup' : 'MsgBox', conversation.id,
                        {
                            heading: conversation.name
                        },
                        'msg-box', {
                            left: x,
                            top: y
                        }, '.msg-container',
                        '<msg-conversation conversation-id="' + conversation.id + '"></msg-conversation>');

                    var iconClass = '';
                    if (conversation.isGroupChat) {
                        iconClass = 'msg-heading-group-chat';
                    }
                    else if (user.online) {
                        iconClass = 'msg-user-online';
                    }
                    // windowService.setWindowHeading(conversation.id,headingHtml);
                    windowService.setWindowHeadingIconClass(conversation.id,iconClass);
                    return false;
                };
                scope.submitParticipators = function (setName) {
                    if (!scope.participators.length) return;
                    var conversationName = setName ? scope.conversation.name : null;
                    var conversation = msgService.newConversation(scope.participators.slice(), scope.conversation.name, true, true);
                    // windowService.setWindowHeading(conversation.id, conversation.name);
                    // scope.conversation = msgService.getConversationById(conversation.id);

                    scope.isOpenHeadpane = false;
                }
            },
            controller: function ($scope, rguModal) {
                this.openConversation = function (conversation) {
                    $scope.openMsgBox(conversation, 0);
                };
                $scope.conversations = msgService.getConversations();
            }
        };
    })
    .directive('msgConversation', function ($rootScope, $moment, rguModal, userService, windowService, msgService) {
        return {
            restrict: 'E',
            templateUrl: RegitTemplatePath + '/msg-conversation.html',
            scope: {
                conversationId: '@'
            },
            link: function (scope, element, attrs) {
                scope.isOpenHeadpane = false;
                scope.me = userService.getCurrentUser();
                var conversationId = attrs.conversationId;
                msgService.markAllRead(conversationId);
                scope.loadConversations = function () {
                    scope.conversation = msgService.getConversationById(conversationId);
                };
                scope.loadConversations();
                scope.participators = scope.conversation.isGroupChat ? scope.conversation.participators : [scope.conversation.from];
                scope.messages = scope.conversation.messages;
                scope.$on('window.selectSettingsMenuItem', function (event, cmd) {

                    if (cmd.name === 'msg.addPeopleToConversation') {
                        scope.isOpenHeadpane = true;
                    }
                    else if (cmd.name === 'msg.manageConversation') {
                        scope.isOpenHeadpane = false;
                        rguModal.openModal(cmd.name, scope);
                    }
                    return false;
                });
                scope.renderSeenInfo = function (message) {
                    return 'Seen ' + $moment(message.read).calendar(null, {sameDay: 'LT'});
                };
                scope.unpickUser = function (user) {
                    var picked = $.inArray(user, scope.participators);
                    if (picked >= 0) {
                        scope.participators.splice(picked, 1);
                    }
                };
                scope.submitParticipators = function (setName) {
                    if (!scope.participators.length) return;
                    var conversationName = setName ? scope.conversation.name : null;
                    var conversation = scope.conversation.isGroupChat ?
                        msgService.updateConversation(scope.conversation.id, scope.participators.slice(), conversationName) :
                        msgService.newConversation(scope.participators.slice(), null, true, true);
                    if (!scope.conversation.isGroupChat) {
                        scope.participators.splice(1);
                        windowService.removeWindow(scope.conversation.id);    // Remove original 1-1 conversation window
                        $rootScope.$broadcast('msg.openConversation', conversation.id);
                    } else {
                        windowService.setWindowHeading(conversation.id, conversation.name);
                        scope.loadConversations();
                    }

                    scope.isOpenHeadpane = false;
                }
            }
        };
    })

    .directive('msgInput', function (msgService) {
        return {
            restrict: 'E',
            templateUrl: 'templates/msg-input-box.html',
            scope: {
                conversationId: '@'
            },
            link: function (scope, element, attr) {
                function encodeMessage(html) {
                    // html = html.replace(/<img src="img\/emojis\/(\d\d)\.png" class="msg-emoji">/g, '$&');
                    
                    html = html.replace(/<div><br><\/div>/g, '');
                    return html; //.replace(/<(?:.|\n)*?>/gm, '');
                }

                scope.conversation = msgService.getConversationById(attr.conversationId);
                scope.messages = scope.conversation.messages;
                var input = element.find('.msg-input-box');
                input.focus();
                input.on('keyup', function (e) {
                    e = e || event;
                    if (e.keyCode == 13 && !e.shiftKey) {
                        var text = encodeMessage(input.html());
                        // submit message
                        var message = {
                            fromMe: true,
                            created: new Date(),
                            text: text
                        };
                        input.html('');
                        scope.$apply(function () {
                            msgService.addMessage(scope.conversationId, message, function () {
                            });
                        });
                        //e.preventDefault();
                        return false;
                    }
                });

                scope.emojis = new Array(40);

                scope.insertEmoji = function (id) {
                    var input = element.find('.msg-input-box');
                    var img = document.createElement('img');
                    img.src = 'img/emojis/' + id + '.png';
                    img.className = 'msg-emoji';
                    input.get(0).appendChild(img);
                    input.focus();
                    //  Set cursor to end of text field
                    var el = input[0];
                    if (typeof window.getSelection != "undefined"
                        && typeof document.createRange != "undefined") {
                        var range = document.createRange();
                        range.selectNodeContents(el);
                        range.collapse(false);
                        var sel = window.getSelection();
                        sel.removeAllRanges();
                        sel.addRange(range);
                    } else if (typeof document.body.createTextRange != "undefined") {
                        var textRange = document.body.createTextRange();
                        textRange.moveToElementText(el);
                        textRange.collapse(false);
                        textRange.select();
                    }

                    scope.openingPopover = false;
                };

            },

            // + '<textarea class="input-msg" placeholder="Type a message..." ng-model="inputMsgText" ng-minlength="1"></textarea>'
            controller: function ($scope, $element) {

            }
        };
    });



