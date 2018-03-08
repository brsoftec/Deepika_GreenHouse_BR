angular.module('msg', ['angular-momentjs', 'windows', 'regit.ui', 'signalR.eventAggregator'])
    .run(function() {

        if (typeof(wdtEmojiBundle) !== 'undefined') {
            wdtEmojiBundle.defaults.emojiSheets.apple = '/Content/img/emojis/sheets/sheet_apple_64_indexed_128.png';
            wdtEmojiBundle.defaults.emojiSheets.google = '/Content/img/emojis/sheets/sheet_google_64_indexed_128.png';
            wdtEmojiBundle.defaults.emojiSheets.twitter = '/Content/img/emojis/sheets/sheet_twitter_64_indexed_128.png';
            wdtEmojiBundle.defaults.emojiSheets.emojione = '/Content/img/emojis/sheets/sheet_emojione_64_indexed_128.png';
        }
    })
    .filter('rawHtml', ['$sce', function($sce){
        return function(text) {
            return $sce.trustAsHtml(text);
        };
    }])
    .factory('msgService', ['$rootScope', '$moment', 'windowService', '$http', '$q', function ($rootScope, $moment, windowService, $http, $q) {
        var currentUserName = '';
        var currentUserId = '';
        var msg = {};
        var conversations = [];

        msg.getConversations = function () {
            var deferred = $q.defer();

            $http({
                method: 'GET',
                url: '/Api/PersonalMessages/conversations'
            })
                .success(function (data, status) {

                    currentUserName = data.userName;
                    currentUserId = data.userId;
                    conversations = data.conversations;
                    
                    // Get latest message date per contact 
                    angular.forEach(conversations, function (conversation) {
                        var messages = conversation.messages;
                        if (messages.length > 0) {
                            conversation.latestMessage = messages[conversation.messages.length - 1];
                            var tmpDate = $moment(messages[conversation.messages.length - 1].created);
                            conversation.dateLatest = tmpDate;                            
                        } else {
                            conversation.dateLatest = $moment(new Date(2000,0,1)); // January 1, 2000
                        }
                    });

                    updateConversations(true);
                    
                    deferred.resolve(data.conversations);
                })
                .error(function (data, status) {
                    __promiseHandler.Error(errors, status, deferred);
                });

            return deferred.promise;
            //return conversations;
        };

        msg.getSettingsMenu = function (windowType) {
            if (windowType === 'MsgList') {
                return {
                    items: [
                        //{
                        //     name: 'msg.newGroupChat',
                        //     label: 'New group chat'
                        // },
                        {
                            name: 'msg.disableMessaging',
                            label: 'Turn off messaging'
                        }
                    ]
                };

            }
            if (windowType === 'MsgBox') {
                return {
                    items: [
                        // {
                        //     name: 'msg.addPeopleToConversation',
                        //
                        //     label: 'Add/Remove people'
                        // },
                        // {
                        //     name: 'msg.deleteConversation',
                        //     label: 'Delete conversation'
                        // },
                        // {
                        //     name: 'msg.reportAbuseInConversation',
                        //     label: 'Report abuse'
                        // }
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

        msg.Updatestatusopenwindow = function (conversationId, status) {
            angular.forEach(conversations, function (conversation) {
                if (conversation.id === conversationId) {
                    conversation.isopen = status;
                }
            });

        };

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
        msg.updatestatustusunread = function (conversationId, conversationmessageId) {
            $http({
                method: 'POST',
                url: '/Api/PersonalMessages/updateunread',
                data: {
                    conversationid: conversationId,
                    conversationmessageid: conversationmessageId
                }
            })
                .success(function (data, status) {
                    // console.log('Add Message: ' + data, 'background: #222; color: #bada55');
                })
                .error(function (data, status) {
                    console.log(status);
                });
        };
        msg.addMessage = function (conversationId, message, callback) {
            var conversation = msg.getConversationById(conversationId);
            if (conversation) {
                if (!message.created) {
                    message.created = new Date();
                }
               // message.text += String.fromCodePoint(128512);
               //  message.text = message.text.replace(//,String.fromCodePoint('$1'))
               //  add message to PersonalMessage collection, call api /API/PersonalMessages/addMessages
                $http({
                    method: 'POST',
                    url: '/Api/PersonalMessages/addmessages',
                    data: message
                })
                    .success(function (data, status) {
                        message.messageid = data;
                        conversation.messages.push(message);
                        windowService.scroolbarend(conversationId);
                        updateConversations();
                    })
                    .error(function (error) {
                        console.log(error);
                    });

                //Push notification to other user (use SignalR)
                //$scope.eventAggregator().publish(new GH.Core.SignalR.Events.MessageSingleConstrainedEvent(message));

            }
            if (callback && angular.isFunction(callback)) {
                var result = { success: true };
                callback.call(null, result);
            }
        };

        msg.fetchUsers = function (network) {
            // return userService.getUsers(network);
            var users = [];
            if (network === 'friends') {
                angular.forEach(conversations, function (conversation) {
                    var user = conversation.from;
                    if ($.inArray(user, users < 0)) {
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
                    name += ' and ' + (users.length - 1) + ' more';
                }
            }

            var conversation = {
                id: Date.now().toString(),
                created: new Date(),
                isGroupChat: isGroupChat,
                name: name,
                owner: currentUserName,
                participators: users,
                messages: [],
                userIds: []
            };

            var userIds = [];
            //  Generate sample message from each participator
            angular.forEach(users, function (user) {
                var message = {
                    from: user,
                    created: new Date(),
                    text: 'Hi, I\'m ' + user.name + '. I\'m in!'
                };
                conversation.messages.push(message);
                userIds.push(user.id);
            });
            conversation.userIds = userIds;

            // console.log(users);

            $http({
                method: 'POST',
                url: '/Api/PersonalMessages/addgroupchat',
                data: conversation
            })
                .success(function (data, status) {
                    console.log('Add Message: ' + data, 'background: #222; color: #bada55');

                    angular.forEach(conversations, function (con) {
                        if (con.id == conversation.id) {
                            con.id = data;
                            console.log('new id ' + con.id);
                        }
                    });
                })
                .error(function (data, status) {
                    console.log(status);
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

        function updateConversations(IsInitialLoad) {
            IsInitialLoad = typeof IsInitialLoad !== 'undefined' ? IsInitialLoad : false;
            angular.forEach(conversations, function (conversation) {
                var messages = conversation.messages;
                if (messages.length > 0) {
                    conversation.latestMessage = messages[conversation.messages.length - 1];
                    var tmpDate = $moment(messages[conversation.messages.length - 1].created);                     
                    conversation.dateLatest = tmpDate;
                    conversation.latestMessage.when = $moment(conversation.latestMessage.created).calendar(null, {
                        sameDay: 'LT'
                    }
                    );
                } else {
                    conversation.dateLatest = $moment(new Date(2000, 0, 1)); // January 1, 2000
                }
                if (!IsInitialLoad) {
                    delete conversation.unreadCount;
                }
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
                //if (unreadCount) {
                //  conversation.unreadCount = unreadCount;
                //}
            });

        }

        function sortResults(prop, asc) {
            arr = arr.sort(function (a, b) {
                if (asc) return (a[prop] > b[prop]);
                else return (b[prop] > a[prop]);
            });
            showResults();
        }

        function updatedeletemessage(conversationId, messageid, datedelete) {
            angular.forEach(conversations, function (conversation) {
                if (conversation.id == conversationId) {
                    $(conversation.messages).each(function (index, object) {
                        if (object.messageid == messageid) {
                            object.Datedeleted = datedelete;
                            object.Isdeleted = true;
                            object.text = "Message deleted";
                            conversation.latestMessage = object;
                            conversation.latestMessage.when = $moment(object.Datedeleted).calendar(null, {
                                sameDay: 'LT'
                            }
                            );
                        }
                    });

                }
            });

        }

        function receiveConversation(conversationId, message, messageid, messagetype, jsonFieldsdrfi) {
            angular.forEach(conversations, function (conversation) {
                if (conversation.id == conversationId) {
                    var newmessage = {
                        created: new Date(),
                        text: message,
                        messageid: messageid,
                        type: messagetype,
                        jsonFieldsdrfi: jsonFieldsdrfi
                    };
                    conversation.messages.push(newmessage);
                    conversation.latestMessage = newmessage;
                    conversation.latestMessage.when =
                        conversation.latestMessage.when = $moment(conversation.latestMessage.created).calendar(null, {
                            sameDay: 'LT'
                        }
                        );
                    if (isNaN(conversation.unreadCount))
                        conversation.unreadCount = 0;
                    if (!conversation.isopen) {
                        conversation.unreadCount = conversation.unreadCount + 1;
                    }
                    else {
                        msg.updatestatustusunread(conversation.id, newmessage.conversationId);
                    }
                    windowService.scroolbarend(conversation.id);
                }
            });
        }

        function refreshConversations() {
            $http({
                method: 'GET',
                url: '/Api/PersonalMessages/conversations'
            })
                .success(function (data, status) {
                    // console.log(data, 'background: #222; color: #bada55');
                    currentUserName = data.userName;
                    currentUserId = data.userId;
                    conversations = data.conversations;
                    updateConversations();
                })
                .error(function (data, status) {
                    console.log("Error: " + data);
                });
        }

        function subscribeConversations() {
            $http({
                method: 'GET',
                url: '/Api/PersonalMessages/userid'
            })
                .success(function (data, status) {
                    $rootScope.eventAggregator().subscribe(GH.Core.SignalR.Events.MessageSingleConstrainedEvent, function onEvent(e) {
                        switch (e.Type) {
                            case "addmessage":
                                var conversationId = e.ConversationId;
                                var message = e.Message;
                                receiveConversation(conversationId, message, e.Messageid, e.MessageType, e.jsonFieldsdrfi);
                                // console.log("addmessage");
                                break;
                            case "deletemessage":
                                updatedeletemessage(e.ConversationId, e.Messageid, e.DateDelete);
                                // console.log("deletemessage");
                                break;
                        }

                    }, { AccountId: data });

                })
                .error(function (data, status) {
                    console.log("Error: " + data);
                });

        };

        subscribeConversations();
        // refreshConversations();
        updateConversations();

        return msg;
    }])
    .directive('msgList', function ($document, $sce, msgService, windowService, rguModal) {
        return {
            restrict: 'E',
            templateUrl: '/Areas/regitUI/templates/msg-list.html?v=1',
            require: '^^window',
            scope: {
                type: '@'
            },
            link: function (scope, element, attrs, windowCtrl) {
                windowCtrl.addSettingsCommand('msg.newGroupChat', function () { /* Not implemented */
                });
                scope.me = {
                    id: '00',
                    name: '',
                    avatar: '',
                    online: false
                };
                scope.$on('window.selectSettingsMenuItem', function (event, cmd) {

                    event.preventDefault();
                    if (scope.type === 'friends') return;
                    if (cmd.name === 'msg.newGroupChat') {
                        scope.participators = [];
                        scope.conversation = msgService.newConversation(scope.participators, 'New group chat', false);
                        rguModal.openModal('msg.manageConversation', scope, { newGroupChat: true });
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
                    msgService.Updatestatusopenwindow(conversation.id, true);
                    if (conversation.unreadCount != 0)
                        msgService.updatestatustusunread(conversation.id, "");
                    var user = conversation.from;
                    var win = element; //$document.find('.msg-list');
                    var x = win.position().left - 310, y = win.position().top;
                    var row = $document.find('.msg-list-row').eq(index);
                    y += row.position().top;
                    var windowHeight = $(window).height();
                    if (y + 780 > windowHeight) {
                        y = windowHeight - 780;
                    }

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
                    windowService.setWindowHeadingIconClass(conversation.id, iconClass);
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
                if ($scope.type == "friends") {
                    msgService.getConversations().then(function (res) {
                        $scope.conversations = res;
                    });
                }
            }
        };
    })
    .directive('msgConversation', function ($rootScope, $timeout, $moment, rguModal, windowService, msgService, rguView, $http, $uibModal, interactionFormService) {
        return {
            restrict: 'E',
            templateUrl: '/Areas/regitUI/templates/msg-conversation.html?v=3',
            scope: {
                conversationId: '@'
            },
            link: function (scope, element, attrs) {
                scope.isOpenHeadpane = false;
                scope.rguView = rguView;
                scope.openMessageActions = function (message) {
                    if (message.fromMe) {
                        rguView.openingContextMenu = message.messageid;
                    }
                    windowService.scroolbarend(attrs.conversationId);
                };
                scope.deleteMessage = function (message) {
                    message.Datedeleted = new Date();
                    message.Isdeleted = true;
                    message.text = "Message deleted";
                    $http({
                        method: 'POST',
                        url: '/Api/PersonalMessages/deletemessages',
                        data: message
                    })
                        .success(function (data, status) {
                            //updateConversations();

                            var messages = scope.conversation.messages;
                            if (messages.length > 0) {
                                scope.conversation.latestMessage = messages[scope.conversation.messages.length - 1];
                                scope.conversation.latestMessage.when = $moment(scope.conversation.latestMessage.Datedeleted).calendar(null, {
                                    sameDay: 'LT'
                                }
                                );
                            }

                            // console.log('deleted Message: ' + data, 'background: #222; color: #bada55');
                        })
                        .error(function (data, status) {
                            console.log(status);
                        });

                    // addMessage
                };

                scope.me = {
                    id: '00',
                    name: '',
                    avatar: '',
                    online: false
                };
                var conversationId = attrs.conversationId;
                msgService.markAllRead(conversationId);
                scope.loadConversations = function () {
                    scope.conversation = msgService.getConversationById(conversationId);
                    $timeout(function () {
                        windowService.scroolbarend(attrs.conversationId);
                    }, 100);

                };
                scope.loadConversations();
                scope.participators = scope.conversation.isGroupChat ? scope.conversation.participators : [scope.conversation.from];
                scope.messages = scope.conversation.messages;
                // scope.$on('window.selectSettingsMenuItem', function (event, cmd) {
                //
                //     if (cmd.name === 'msg.addPeopleToConversation') {
                //         scope.isOpenHeadpane = true;
                //     }
                //     else if (cmd.name === 'msg.manageConversation') {
                //         scope.isOpenHeadpane = false;
                //         rguModal.openModal(cmd.name, scope);
                //     }
                //     return false;
                // });
                scope.$on('window.windowclose', function (event) {
                    msgService.Updatestatusopenwindow(scope.conversation.id, false);
                    scope.conversation.isopen = false;
                    return false;
                });
                scope.renderSeenInfo = function (message) {
                    return $moment(message.created).calendar(null, { sameDay: 'LT' });
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
                };
                scope.viewformdrfi = function (msg) {
                    var fields = JSON.parse(msg.jsonFieldsdrfi);
                    if (fields != null && fields != undefined) {
                        $(fields).each(function (index) {
                            fields[index].membership = fields[index].membership == "true" ? true : false;
                            fields[index].selected = true;
                            switch (fields[index].type) {
                                case "date":
                                case "datecombo":
                                    fields[index].model = new Date(fields[index].model);
                                    break;
                                case "location":
                                    var mode = {
                                        country: fields[index].model,
                                        city: fields[index].unitModel
                                    }
                                    fields[index].model = mode;
                                    break;
                                case "address":
/*                                    var mode = {
                                        address: fields[index].model,
                                        address2: fields[index].unitModel
                                    }
                                    fields[index].model = mode;*/
                                    break;
                                case "doc":
                                    var m = [];
                                    $(fields[index].modelarrays).each(function (index, fname) {
                                        m.push({
                                            fname: fname
                                        });
                                    })
                                    fields[index].model = m;
                                    break;
                            }
                        });
                        var drfiMessage = {
                            fromMe: true,
                            created: new Date(),
                            text: '[DRFI Response]',
                            type: 'drfiresponse',
                            jsonFieldsdrfi: "",
                            conversationId: scope.conversation.id,
                            ToReceiverId: scope.conversation.from.id
                        };
                        var modalInstance = $uibModal.open({
                            animation: true,
                            templateUrl: 'modal-feed-open-drfi.html',
                            controller: 'RequestFormDRFIController',
                            size: "",
                            backdrop: 'static',
                            resolve: {
                                registerPopup: function () {
                                    return {
                                        ListOfFields: fields,
                                        userid: msg.toReceiverId,
                                        drfiMessage: drfiMessage
                                    };
                                }
                            }
                        });

                        modalInstance.result.then(function (data) {
                        }, function () {
                        });
                    }
                };
                scope.isvaliddrfi = function (field) {
                    var isvalid = true;
                    if (field.model == "" || field.model == undefined || field.model == null)
                        ismiss = false;
                    switch (field.type) {
                        case "doc":
                            isvalid = (angular.isArray(field.model) && field.model.length)
                                || (field.hasOwnProperty('modeldocvault') && field.modeldocvault.length);
                            break;
                        case "address":
                            if (field.model == null || field.model.address == "")
                                isvalid = false;
                            break;
                        case "location":
                            if (field.model == null || field.model.country == "" || (field.model.city == null && field.options != 'nocity'))
                                isvalid = false;
                            break;
                        case "date":
                            if (field.model + "" == "Invalid Date")
                                isvalid = false;
                            break;
                    }
                    return isvalid;
                };
                scope.acceptDrfi = function (msg) {
                    var fields = JSON.parse(msg.jsonFieldsdrfi);
                    if (fields != null && fields != undefined) {
                        $(fields).each(function (index) {
                            fields[index].membership = fields[index].membership == "true" ? true : false;
                            fields[index].selected = true;
                            switch (fields[index].type) {
                                case "date":
                                case "datecombo":
                                    fields[index].model = new Date(fields[index].model);
                                    break;
                                case "location":
                                    var mode = {
                                        country: fields[index].model,
                                        city: fields[index].unitModel
                                    }
                                    fields[index].model = mode;
                                    break;
                                case "address":
                                    var mode = {
                                        address: fields[index].model,
                                        address2: fields[index].unitModel
                                    }
                                    fields[index].model = mode;
                                    break;

                                case "doc":
                                    var m = [];
                                    $(fields[index].modelarrays).each(function (index, fname) {
                                        m.push({
                                            fname: fname
                                        });
                                    })
                                    fields[index].model = m;
                                    break;
                            }
                        });
                    }
                    var data = new Object();
                    data.UserId = "";
                    data.Listvaults = [];
                    var isvalid = true;
                    $(fields).each(function (index) {
                        if (!fields.optional || fields[index].selected) {
                            var field = {
                                id: fields[index].id,
                                jsPath: fields[index].jsPath,
                                displayName: fields[index].displayName,
                                optional: fields[index].optional,
                                type: fields[index].type,
                                options: fields[index].options,
                                model: fields[index].model,
                                unitModel: fields[index].unitModel,
                                value: fields[index].value,
                                membership: fields[index].membership + "",
                                modelarrays: fields[index].modelarrays,
                                choices: fields[index].choices + "",
                                qa: fields[index].qa + ""
                            }
                            if (isvalid)
                                isvalid = scope.isvaliddrfi(fields[index]);
                            if (!isvalid)
                                return;

                            var allowadd = true;
                            switch (field.type) {
                                case "location":
                                    var model = fields[index].model;
                                    field.model = model.country;
                                    field.unitModel = model.city;
                                    break
                                case "address":
                                    var model = fields[index].model;
                                    field.model = model.address;
                                    field.unitModel = model.address2;
                                    break;
                                case "range":
                                    var model = fields[index].model;
                                    if (model == -1 || model == "-1")
                                        allowadd = false;
                                    var arrays = [];
                                    $(field.modelarrays).each(function (index, object) {
                                        var array = [];
                                        array.push(object[0]);
                                        array.push(object[1]);
                                        arrays.push(array);
                                    });
                                    field.modelarrays = arrays;
                                    break;
                                case "qa":
                                    if (field.choices) {
                                        var arrays = [];
                                        $(fields[index].modelarrays).each(function (index, object) {
                                            arrays.push({ value: object.value });
                                        });
                                        field.modelarrays = arrays;
                                        field.model = fields[index].model.value;
                                    }
                                    else
                                        field.model = fields[index].model;
                                    break;
                                case "doc":
                                    var arrays = [];
                                     $(field.model).each(function (index, object) {
                                            arrays.push(object.fname);
                                        });
                                     field.modelarrays = arrays;
                                    field.pathfile=fields[index].pathfile;
                                    field.model = "";
                                    break;
                                case "history":
                                    allowadd = false;
                                    break;
                                case "datecombo":
                                case "date":
                                    allowadd = true;
                                    field.model = $moment(field.model).format('YYYY-MM-DD');
                                    break;
                            }
                            if (allowadd == true)
                                data.Listvaults.push(field);
                        }
                    });
                    if (!isvalid)
                        return;
                    var drfiMessage = {
                        fromMe: true,
                        created: new Date(),
                        text: '[DRFI Response]',
                        type: 'drfiresponseaccepted',
                        drfiRequestId: msg.messageid,
                        jsonFieldsdrfi: JSON.stringify(data.Listvaults),
                        conversationId: scope.conversation.id,
                        ToReceiverId: scope.conversation.from.id
                    };
                    msgService.addMessage(drfiMessage.conversationId, drfiMessage, function () {
                    });
                }
                scope.denyDrfi = function (msg) {
                  var drfiMessage = {
                        fromMe: true,
                        created: new Date(),
                        text: '[DRFI Response]',
                        type: 'drfiresponsedenied',
                        drfiRequestId: msg.messageid,
                        jsonFieldsdrfi: "",
                        conversationId: scope.conversation.id,
                        ToReceiverId: scope.conversation.from.id
                    };
                    msgService.addMessage(drfiMessage.conversationId, drfiMessage, function () {
                    });
                }

                scope.renderDrfiData = function (msg, asText) {
                    function deCamelize(text) {
                        if (!text) return '';
                        var result = text.replace(/([a-z])([A-Z])/g, "$1 $2");
                        return result.charAt(0).toUpperCase() + result.slice(1);
                    }
                    var json = '{}';
                    if (msg.jsonFieldsdrfi) {
                        json = '{ "fields": ' + msg.jsonFieldsdrfi + ' }';
                    }
                    var fields = JSON.parse(json).fields;
                    if (fields) {
                        $(fields).each(function (index) {
                            
                            switch (fields[index].type) {
                                case "doc":
                                    var m = [];
                                    $(fields[index].modelarrays).each(function (index, fname) {
                                        m.push({
                                            fname: fname
                                        });

                                    })
                                    fields[index].model = m;
                                    break;
                            }
                        });
                    }
                    var html = '', text = '';
                    interactionFormService.initFieldGroups(fields);
                    angular.forEach(fields, function (field) {
                        if (field.firstInGroup) {
                            html += '<h5>' + deCamelize(field.group) + '</h5>';
                            text += '----' + deCamelize(field.group) + '----\n';
                        }
                        var value = field.model;
                        if (!value) return;
                        if (field.type === 'location') {
                            value = field.unitModel + ', ' + field.model;
                            html += '<p>' + field.displayName + ': ' + value + '</p>';
                        }
                        else if (field.type === 'doc')
                        {
                            html += '<p>' + field.displayName + '</p>';
                            $(field.modelarrays).each(function (index1, objectdoc) {
                                html += '<p style="margin-left:5px" ><a target="_blank" href=/' + field.pathfile + encodeURI(objectdoc) + '>' + objectdoc + '</a></p>';
                            });
                        }
                        else {
                            value = value.toString();
                            html += '<p>' + field.displayName + ': ' + value + '</p>';
                        }
                       
                        text += field.displayName + ': ' + value + '\n';
                    });
                    return asText ? text : html;
                };
                scope.missingDrfiData = function (msg) {
                    var ismiss = false;
                    var json = '{}';
                    if (msg.jsonFieldsdrfi) {
                        json = '{ "fields": ' + msg.jsonFieldsdrfi + ' }';
                    }

                    var fields = '';
                    if (fields) {
                        $(fields).each(function (index) {
                            fields[index].membership = fields[index].membership == "true" ? true : false;
                            fields[index].selected = true;
                            switch (fields[index].type) {
                                case "date":
                                case "datecombo":
                                    fields[index].model = new Date(fields[index].model);
                                    break;
                                case "location":
                                    var mode = {
                                        country: fields[index].model,
                                        city: fields[index].unitModel
                                    }
                                    fields[index].model = mode;
                                    break;
                                case "address":
                                    var mode = {
                                        address: fields[index].model,
                                        address2: fields[index].unitModel
                                    }
                                    fields[index].model = mode;
                                    break;

                                case "doc":
                                    var m = [];
                                    $(fields[index].modelarrays).each(function (index, fname) {
                                        m.push({
                                            fname: fname
                                        });

                                    })
                                    fields[index].model = m;
                                    break;
                            }
                        });
                    };
                    angular.forEach(fields, function (field) {
                        if (!scope.isvaliddrfi(field))
                            ismiss = true;
                    });
                    return ismiss;
                };
                scope.renderDrfiDataPreview = function (msg) {
                    function deCamelize(text) {
                        if (!text) return '';
                        var result = text.replace(/([a-z])([A-Z])/g, "$1 $2");
                        return result.charAt(0).toUpperCase() + result.slice(1);
                    }
                    var json = '{}';
                    if (msg.jsonFieldsdrfi) {
                        json = '{ "fields": ' + msg.jsonFieldsdrfi + ' }';
                    }
                    try {
                        // var fields = JSON.parse(json).fields;
                        var fields = $.parseJSON(json).fields;
                    } catch(error) {
                        console.log('Error parsing DRFI');
                    }


                    if (fields) {
                        if (!angular.isArray(fields)) {
                            // fields = $.map(fields, function (field) {
                            //     return [Number(e), field];
                            // });
                            var result = Object.keys(fields).map(function(e) {
                                return [Number(e), fields[e]];
                            });
                            // console.log(result);
                            fields = result;
                        }
                        // console.log(fields);
                        fields.forEach(function (field, index) {
                            fields[index].membership = fields[index].membership == "true" ? true : false;
                            fields[index].selected = true;
                            switch (fields[index].type) {
                                case "date":
                                case "datecombo":
                                    fields[index].model = new Date(fields[index].model);
                                    break;
                                case "location":
                                    var mode = {
                                        country: fields[index].model,
                                        city: fields[index].unitModel
                                    }
                                    fields[index].model = mode;
                                    break;
                                case "address":
                                    var mode = {
                                        address: fields[index].model,
                                        address2: fields[index].unitModel
                                    }
                                    fields[index].model = mode;
                                    break;

                                case "doc":
                                    var m = [];
                                    $(fields[index].modelarrays).each(function (index, fname) {
                                        m.push({
                                            fname: fname
                                        });

                                    })
                                    fields[index].model = m;
                                    break;
                            }
                        });
                    };
                    // console.log(fields);
                    var html = '';
                    interactionFormService.initFieldGroups(fields);
                    angular.forEach(fields, function (field) {
                        if (field.firstInGroup) {
                            html += '<h5>' + deCamelize(field.group) + '</h5>';
                        }
                        var value = field.model;
                        var missing = !scope.isvaliddrfi(field);
                        if (!missing) {
                            value = '***';
                        } else {
                            value = '<span class="drfi-missing-value">(no value)</span>'
                        }
                        // if (field.type==='location') {
                        //     value = field.unitModel + ', '+ field.model;
                        // } else {
                        //     value = value.toString();
                        // }
                        html += '<p';
                        if (missing) {
                            html += ' class="drfi-missing-field"';
                        }
                        html += '>' + field.displayName + ': ' + value + '</p>';
                    });
                    return html;
                };
                scope.downloadinformationdrfi = function (msg) {
                    var fieldsvault = JSON.parse(msg.jsonFieldsdrfi);
                    var items = [];
                    var item = new Object();
                    $(fieldsvault).each(function (index, object1) {

                        switch (object1.type) {
                            case 'doc':
                                var strdoc="";
                                 var fullhostname = $(location).attr('host');
                                $(object1.modelarrays).each(function(index1,object2){
                                strdoc+=fullhostname+"//"+object1.pathfile+object2+", ";
                                })

                                item[object1.displayName] = strdoc;

                                break;
                            default:
                                item[object1.displayName] = object1.model;
                                break;

                        }

                    })
                    items.push(item);
                    if (items.length > 0) {
                        var opts = {
                            headers: true,
                            column: { style: { Font: { Bold: "1" } } },

                            //cells: {
                            //    1: {
                            //        1: {
                            //            style: { Font: { Color: "#00FFFF" } }
                            //        }
                            //    }
                            //}
                        };
                        var fileName = "DRFIResponse.xlsx";
                        // alasql('SELECT * INTO xlsx("ExportAllHandshakes.xlsx",?) FROM ?', [opts, items]);
                        alasql('SELECT * INTO xlsx("' + fileName + '",?) FROM ?', [opts, items]);
                    }

                };

            }
        };
    })

    .directive('msgInput', function (msgService, $timeout) {

        var resId = 0;
        return {
            restrict: 'E',
            templateUrl: '/Areas/regitUI/templates/msg-input-box.html?v=5',
            scope: {
                conversationId: '@'
            },
            link: function (scope, element, attr) {
                resId++;
                element.addClass('c' + resId + scope.conversationId);
                function encodeMessage(html) {
                    // html = html.replace(/<img src="img\/emojis\/(\d\d)\.png" class="msg-emoji">/g, '$&');
                    html = html.replace(/<div><br><\/div>/g, '');
                    html = html.replace(/(<br\s*\/?>)+\s*$/g, ''); // remove trailing line breaks;
                    return html; //.replace(/<(?:.|\n)*?>/gm, '');
                }
                scope.openEmojiPopup = function($event) {
                    var input = element.find('.msg-input-box');
                    var id = 'msginput' + resId;
                    input.addClass(id);
                    if (wdtEmojiBundle) {
                        wdtEmojiBundle.close();
                        $timeout(function () {
                            wdtEmojiBundle.init('.' + id, '.c' + resId + scope.conversationId);
                            wdtEmojiBundle.openPicker({target: element.find('.wdt-emoji-picker').get(0)});
                            element.find('.wdt-emoji-popup').addClass('open');
                        }, 100);
                    }
                };

                scope.closeEmojiPopup = function() {
                    wdtEmojiBundle.close();
                };

                scope.conversation = msgService.getConversationById(attr.conversationId);
                scope.messages = scope.conversation.messages;
                var input = element.find('.msg-input-box');
                input.focus();
                input.on('keyup', function (e) {
                    e = e || event;
                    // html = html.replace(/<br>/g,'');
                    if (e.keyCode === 13 && !e.shiftKey) {
                        var text = input.text();
                        var html = input.html();
                        if (!text.length && html.indexOf('emoji') === -1) {
                            input.html('');
                            return false;
                        }
                        text = encodeMessage(html);

                        // submit message
                        var message = {
                            fromMe: true,
                            created: new Date(),
                            text: text,
                            conversationId: scope.conversationId,
                            ToReceiverId: scope.conversation.from.id
                        };
                        input.html('');
                        scope.$apply(function () {
                            msgService.addMessage(scope.conversationId, message, function () {
                            });
                        });
                        return false;
                    }
                });

                scope.emojis = new Array(40);

                scope.insertEmoji = function (id) {
                    var input = element.find('.msg-input-box');
                    var img = document.createElement('img');
                    if (id < 9) {
                        id = '0' + id;
                    }
                    img.src = '/Areas/regitUI/img/emojis/' + id + '.png';
                    img.className = 'msg-emoji';
                    input.get(0).appendChild(img);
                    input.focus();
                    //  Set cursor to end of text field
                    var el = input[0];
                    if (typeof window.getSelection !== "undefined"
                        && typeof document.createRange !== "undefined") {
                        var range = document.createRange();
                        range.selectNodeContents(el);
                        range.collapse(false);
                        var sel = window.getSelection();
                        sel.removeAllRanges();
                        sel.addRange(range);
                    } else if (typeof document.body.createTextRange !== "undefined") {
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
    })
    .controller('DrfiController', function ($scope, $http, msgService) {

        $scope.drfi = {
            agreed: false
        };
        $scope.matchedField = null;
        $scope.pickedField = $scope.model;

        $scope.vaultEntries = [];
        $scope.formEntries = [];

        //  Prepare Vault tree for search
        $http.post('/api/CampaignService/GetVaultTreeForRegistration', {})
        // $http.get('/Content/sources/interaction-vault-tree.json')
            .success(function (response) {
                $scope.vaultTree = response.TreeVault;

                var entries = [];

                function toTitleCase(str) {
                    return str.replace(/\w\S*/g, function (txt) {
                        return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
                    });
                }

                function traverseVault(node, level, path, fullName, jsPath) {
                    if (!angular.isObject(node) || node.nosearch || node.nodrfi) return;
                    angular.forEach(node, function (entry, name) {
                        if (!angular.isObject(entry) || entry.nosearch || !angular.isDefined(entry.hiddenForm) || entry.hiddenForm === true)
                            return;

                        var displayName = toTitleCase(entry.label);
                        var thisJsPath = jsPath + '.' + name;
                        switch (thisJsPath) {
                            case '.contact.mobile':
                                displayName = 'Mobile Number';
                                break;
                            case '.contact.office':
                                displayName = 'Office Number';
                                break;
                            case '.contact.officeEmail':
                                displayName = 'Work Email';
                                break;
                            case '.governmentID.passportID':
                                displayName = 'Passport Form';
                                break;
                        }
                        if (!angular.isObject(entry.value) || angular.isArray(entry.value)) {
                            //  Leaf entry (field)

                            var field = {
                                id: entry._id,
                                label: entry.label,
                                displayName: displayName,
                                options: entry.value,
                                leaf: true,
                                type: entry.controlType,
                                undraggable: entry.undraggable,
                                rules: entry.rules,
                                level: level,
                                path: path,
                                fullName: fullName + ' / ' + entry.label,
                                jsPath: thisJsPath,
                                selected: false
                            };
                            if (jsPath.substring(0, 11) === '.membership') {
                                field.membership = true;
                            }
                            if (entry.controlType === 'history') {
                                field.model = 4;
                            }
                            entries.push(field);
                        } else {
                            //  Non-leaf entry (folder)
                            entries.push({
                                id: entry._id,
                                label: entry.label,
                                displayName: displayName,
                                leaf: false,
                                undraggable: entry.undraggable,
                                level: level,
                                path: path,
                                fullName: fullName + ' / ' + entry.label,
                                jsPath: thisJsPath,
                                selected: false
                            });
                            traverseVault(entry.value, level + 1, path + '.' + entry.label, fullName + ' / ' + entry.label, thisJsPath);
                        }
                    });
                }

                traverseVault($scope.vaultTree, 1, '', '', '');
                $scope.fields = $scope.vaultEntries = entries;
/*                var fields = $scope.fields.map(function(f) {
                    var field = {
                        path: f.jsPath,
                        title: f.displayName
                    };
                    if (f.leaf) {
                        field.type = f.type;
                        if (f.options) field.options = angular.isArray(f.options) ? f.options : [ f.options ];
                        if (f.rules) field.rules = f.rules;
                    } else {
                        field.branch = true;
                    }

                    return field;
                });
               console.log(JSON.stringify(fields))*/


                //  Add presets
                $scope.addFieldByJsPath('.address.currentAddress');
                $scope.addFieldByJsPath('.address.mailingAddress');
                $scope.addFieldByJsPath('.governmentID.passportID');
                $scope.addFieldByJsPath('.contact.mobile');
                $scope.addFieldByJsPath('.contact.office');
                $scope.addFieldByJsPath('.contact.email');
                $scope.addFieldByJsPath('.contact.officeEmail');


            })
            .error(function (errors, status) {
            });

        $scope.pickMatchedField = function ($item, $model, $label, $event) {
            $scope.addField($scope.matchedField, true);
            $scope.matchedField = null;
        };

        $scope.addField = function (field, selected) {
            var newField = angular.copy(field);
            if (selected) {
                newField.selected = true;
            }
            $scope.formEntries.push(newField);
        };
        $scope.addFieldByJsPath = function (jsPath) {
            angular.forEach($scope.vaultEntries, function (field) {
                //remove photo
                if (field.jsPath === jsPath) {
                    $scope.addField(field);
                }
            });

        };

        $scope.deleteField = function (field) {
            var index = $.inArray(field, $scope.formEntries);
            if (index >= 0) {
                $scope.formEntries.splice(index, 1);
            }
        };

        $scope.addedToForm = function (entry) {
            var found = false;
            angular.forEach($scope.formEntries, function (field) {
                if (entry.id === field.id) {
                    found = true;
                } else if (new RegExp('^' + field.jsPath, 'i').test(entry.jsPath)) {
                    found = true;
                }
            });
            return found;
        };

        $scope.filterEntry = function (field) {
            return (field.leaf || !field.undraggable) && !field.membership && !$scope.addedToForm(field);
        };
        $scope.filterMatches = function (field) {
            return !($scope.model && field.id === $scope.model.id);
        };

        $scope.close = function () {
            $scope.view.openingDRFIPopover = false;
        };

        $scope.canSend = function () {
            var selected = false;
            angular.forEach($scope.formEntries, function (field) {
                if (field.selected) {
                    selected = true;
                }
            });
            return selected;
        };
        $scope.sendDrfi = function () {
            var fields = [];
            angular.forEach($scope.formEntries, function (entry) {
                if (!entry.selected) return;
                if (entry.leaf) {
                    fields.push(entry);
                } else {
                    var jsPath = entry.jsPath;
                    angular.forEach($scope.vaultEntries, function (field) {
                        if (new RegExp('^' + jsPath + '\.', 'i').test(field.jsPath)) {
                            fields.push(field);
                        }
                    });

                }
            });
            // console.log(fields);
            var fieldsnew = [];
            $(fields).each(function (index, object) {

                if (object.jsPath.search(".photo") < 0)
                {

                    fieldsnew.push(fields[index]);
                }
            })
            var vaultfields = [];
            $(fieldsnew).each(function (index, field) {
                switch (field.jsPath) {
                    default:
                        vaultfields.push({
                            displayName: field.label,
                            id: field.id,
                            jsPath: field.jsPath + "",
                            path: field.path,
                            label: field.label,
                            optional: false,
                            type: field.type,
                            options: field.options,
                            value: "",
                            model: "",
                            unitModel: "",
                            membership: false

                        })
                }
            })
            //  Add DRFI message
            var drfiMessage = {
                fromMe: true,
                created: new Date(),
                text: '[DRFI Request]',
                type: 'drfi',
                jsonFieldsdrfi: JSON.stringify(vaultfields),
                conversationId: $scope.conversation.id,
                ToReceiverId: $scope.conversation.from.id
            };
            msgService.addMessage($scope.conversation.id, drfiMessage, function () {
            });

            $scope.close();
        };

    });



