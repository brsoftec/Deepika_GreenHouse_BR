var cloudApp = angular.module('cloudapps', [])

    .value('fbapiStatus', {load: ''})
    .value('fbapiEvents', {
        onLoad: function () {
        }
    })
    .run(function ($window, fbapiConfig, fbapiStatus, fbapiEvents, gapiConfig, gapi) {
        function fbapiInit() {
            fbapiStatus.load = 'loaded';
            FB.getLoginStatus(function (response) {

                if (response.status === 'connected') {
                    if (angular.isFunction(fbapiEvents.onLoad))
                        fbapiEvents.onLoad(true);
                } else if (response.status === 'unknown') {
                    // FB.login(function (response) {
                    //     if (angular.isFunction(fbapiEvents.onLoad))
                    //         fbapiEvents.onLoad(response.status === 'connected');
                    // }, {scope: 'public_profile,user_friends'});
                    if (angular.isFunction(fbapiEvents.onLoad))
                        fbapiEvents.onLoad('unknown');
                }
                else {
                    if (angular.isFunction(fbapiEvents.onLoad))
                        fbapiEvents.onLoad(false);
                }
            });
        }

        $window.fbAsyncInit = function () {
            FB.init({
                appId: fbapiConfig.appId,
                autoLogAppEvents: true,
                xfbml: true,
                version: 'v2.11'
            });
            fbapiInit();
        };

        function apiInit() {
            // Initialize the client with API key and People API, and initialize OAuth with an
            // OAuth 2.0 client ID and scopes (space delimited string) to request access.
            $window.gapi.client.init({
                apiKey: gapiConfig.apiKey,
                discoveryDocs: ["https://people.googleapis.com/$discovery/rest?version=v1"],
                clientId: gapiConfig.clientId,
                scope: 'profile https://www.googleapis.com/auth/contacts.readonly'
            }).then(function () {
                gapi.ready();
                // Listen for sign-in state changes.
                $window.gapi.auth2.getAuthInstance().isSignedIn.listen(gapi.updateSigninStatus);

                // Handle the initial sign-in state.
                gapi.updateSigninStatus($window.gapi.auth2.getAuthInstance().isSignedIn.get());

            });
        }

        $window.gapiInit = function () {
            $window.gapi.load('client:auth2', apiInit);
        };


    })
    .factory('gapi', function () {
        var loaded = false;
        var signedIn = false;
        var onLoad = function () {
        };
        var onSign = function (isSignedIn) {
        };

        return {
            ready: function () {
                loaded = true;
                onLoad();
            },
            updateSigninStatus: function (isSignedIn) {
                signedIn = isSignedIn;
                onSign(isSignedIn);
            },
            onLoad: function (callback) {
                onLoad = callback;
                if (loaded) callback();
            },
            onSign: function (callback) {
                onSign = callback;
                if (signedIn) callback(signedIn);
            },
            signIn: function (callback) {
                gapi.auth2.getAuthInstance().signIn();
                if (angular.isFunction(callback))
                    callback();
            },
            signOut: function (callback) {
                gapi.auth2.getAuthInstance().signOut();
                if (angular.isFunction(callback))
                    callback();
            },
            getProfile: function (callback) {

                gapi.client.people.people.get({
                    'resourceName': 'people/me',
                    'personFields': 'names,emailAddresses,photos'
                })
                    .then(function (response) {
                            var person = response.result;
                            var profile = {
                                name: angular.isArray(person.names) ? person.names[0].displayName : '',
                                email: angular.isArray(person.emailAddresses) ? person.emailAddresses[0].value : '',
                                avatar: ''
                            };
                            if (angular.isArray(person.photos)) {
                                var defaultPhotos = person.photos.filter(function (photo) {
                                    return !!photo.default;
                                });
                                if (defaultPhotos.length) {
                                    profile.avatar = defaultPhotos[0].url;
                                } else {
                                    profile.avatar = person.photos[0].url;
                                }
                            }
                            callback(profile);

                        },
                        function (reason) {
                            console.log('Error getting Google profile: ' + reason.result.error.message);
                            callback(null);
                        });
            },
            getContacts: function (callback) {
                gapi.client.people.people.connections.list({
                    'resourceName': 'people/me',
                    'personFields': 'names,emailAddresses,photos',
                    'pageSize': 2000
                })
                    .then(function (response) {
                            var contacts = [];
                            response.result.connections.forEach(function (person) {
                                var contact = {
                                    name: angular.isArray(person.names) ? person.names[0].displayName : '',
                                    email: angular.isArray(person.emailAddresses) ? person.emailAddresses[0].value : '',
                                    avatar: ''
                                };
                                if (angular.isArray(person.photos)) {
                                    /*
                                                                var defaultPhotos = person.photos.filter(function(photo) {
                                                                    return !!photo.default;
                                                                });
                                                                if (defaultPhotos.length) {
                                                                    contact.avatar = defaultPhotos[0].url;
                                                                } else {
                                                                    contact.avatar = person.photos[0].url;
                                                                }
                                    */
                                    contact.avatar = person.photos[0].url;

                                }
                                contact.selected = !!contact.email;
                                contacts.push(contact);
                            });
                            callback(contacts);

                        },
                        function (reason) {
                            console.log('Error getting Google contacts: ' + reason.result.error.message);
                            callback(null);
                        });
            }
        }
    })
    .factory('fbapi', function ($window, fbapiConfig, fbapiStatus, fbapiEvents) {
        var accessToken = '';
        return {
            ready: function () {
            },

            onLoad: function (callback) {
                fbapiEvents.onLoad = callback;
                if (fbapiStatus.load === 'loaded') {
                    FB.getLoginStatus(function (response) {
                        if (response.status === 'connected') {
                            callback(true);
                        } else if (response.status === 'unknown') {
                            callback('unknown');
                        }
                        else {
                            callback(false);
                        }
                    });
                }
            },
            login: function (callback) {
                FB.getLoginStatus(function (response) {
                    if (response.status === 'connected') {
                        //accessToken = response.authResponse.accessToken;
                        callback();
                    }
                    else {
                        FB.login(function (response) {
                            //var auth = FB.getAuthResponse();
                            //accessToken = auth.accessToken;
                            if (response.status === 'connected')
                                callback();
                        }, {scope: 'public_profile,user_friends'});
                    }
                });

            },
            getProfile: function (callback) {
                var profile = {
                    name: '', avatar: ''
                };
                FB.api('/me', function (response) {
                    profile.name = response.name;
                    FB.api('/me/picture',
                        function (response) {
                            profile.avatar = response.data.url;
                            callback(profile);

                        });
                });
            },
            sendDialog: function (callback) {
                FB.ui({
                        method: 'send',
                        link: 'https://regit.today'
                    },
                    function (response) {
                        callback(response && response.success)
                    });
            }
        };
    });

