var inviteApp = angular.module("inviteApp");
inviteApp.controller("InviteController", function ($scope, $window, $http, $timeout, fbapi, gapi, rguNotify) {
    $scope.nav = {
        section: 'google'
    };
    $scope.navTo = function (section) {
        $scope.nav.section = section;
        if (section === 'facebook') {
            $scope.inviteFacebook();
        }
    };
    $scope.view = {
        showingAll: false
    };
    $scope.invite = {
        message: 'Hi, I want to connect with you on the Regit platform so we can keep in touch.'
    };
    $scope.sent = $scope.fbSent = false;

    $scope.filterContacts = function (person) {
        if ($scope.view.showingAll) return !!person.name || person.email;
        return !!person.email;
    };
    $scope.sortByEmail = function (person) {
        return person.email ? '0' + person.name : person.name;
    };
    $scope.sortByName = function (person) {
        return person.name;
    };
    $scope.toggleSelect = function (person) {
        if (!person.email || $scope.sent) return;
        person.selected = !person.selected;
    };
    $scope.gapi = {
        loaded: false,
        signedIn: false
    };
    $scope.fbapi = {
        loaded: false,
        signedIn: undefined
    };
    $scope.profileLoaded = false;
    $scope.ggContacts = [];
    $scope.ggContactsLoaded = false;
    $scope.fbprofileLoaded = false;
    $scope.fbFriends = [];
    $scope.fbFriendsLoaded = false;

    gapi.onLoad(function () {
        $scope.$apply(function () {
            $scope.gapi.loaded = true;
        });
    });
    gapi.onSign(function (isSignedIn) {
        $scope.$apply(function () {
            $scope.gapi.signedIn = isSignedIn;
            if (isSignedIn) {
                $scope.inviteGoogle();
            }
        });
    });

    $scope.signInGoogle = function () {
        gapi.signIn();
    };
    $scope.signOutGoogle = function () {
        gapi.signOut(function() {
            $scope.profileLoaded = false;
            $scope.clearContacts();
        });
    };

    $scope.clearContacts = function () {
        $scope.ggContacts.splice(0);
        $scope.ggContactsLoaded = false;
    };


    $scope.noEmails = function () {
        return $scope.ggContacts.every(function (person) {
            return !person.email;
        });
    };
    $scope.selectedContacts = function () {
        return $scope.ggContacts.filter(function (person) {
            return person.selected;
        });
    };

    $scope.getGoogleProfile = function () {
        if (!$scope.gapi.signedIn) return;
        gapi.getProfile(function(profile) {
            if (!profile) return;
            $scope.profile = profile;
            $scope.profileLoaded = true;
            $scope.$apply();
        });

    };
    $scope.getGoogleContacts = function () {
        if (!$scope.gapi.signedIn) return;

        gapi.getContacts(function(contacts) {
            if (!contacts) return;
            $scope.ggContactsLoaded = true;
            $scope.ggContacts = contacts;
            $scope.$apply();
        });
    };

    $scope.inviteGoogle = function () {
        $scope.getGoogleProfile();
        $scope.getGoogleContacts();
    };


    $scope.inviteFacebook = function () {
        fbapi.onLoad(function (isSignedIn) {
            // $scope.$apply(function () {
                $scope.fbapi.loaded = true;
                $scope.fbapi.signedIn = isSignedIn;
            // });
            if (isSignedIn === 'unknown') {
                $scope.fbapi.signedIn = false;
                $scope.signInFacebook();
            }
            else if (isSignedIn) {
                $scope.getFacebookProfile();
            }


        });
    };
    $scope.signInFacebook = function (inFrame) {
        fbapi.login(function () {
            $scope.$apply(function () {
                // $timeout(function() {
                $scope.fbapi.signedIn = true;

                // });

            });
            $scope.getFacebookProfile();
        });
    };
    $scope.signOutFacebook = function () {
        FB.logout(function (response) {
            $scope.fbapi.signedIn = false;
            $scope.fbProfileLoaded = false;
            $scope.fbSent = false;
            $scope.clearFriends();
            $scope.$apply();
        });
    };

    $scope.clearFriends = function () {
        $scope.fbFriends.splice(0);
        $scope.fbFriendsLoaded = false;
    };

    $scope.getFacebookProfile = function () {
        fbapi.getProfile(function (profile) {
            $scope.fbProfile = profile;
            $scope.fbProfileLoaded = true;
            $scope.$apply();
        });
    };

    $scope.getFacebookFriends = function () {
        $scope.clearFriends();

    };

    $scope.openSendDialog = function () {
        $scope.fbSent = false;
        fbapi.sendDialog(function (sent) {
            if (!sent) return;
            $scope.$apply(function () {
                $scope.fbSent = true;

            });
        });
    };


    $scope.sendInvitations = function () {
        var account = regitGlobal.userAccount;
        var contacts = $scope.selectedContacts();
        if (!contacts.length) return;
        var invites = [];
        contacts.forEach(function (person) {
            var invite = {
                category: 'network',
                toEmail: person.email,
                fromAccountId: account.id,
                fromDisplayName: account.displayName,
                toName: person.name,
                inviteId: '',
                message: $scope.invite.message,
                options: null
            };
            invites.push(invite);
        });

        $http.post('/Api/Invite/NewInvites', invites)
            .success(function (response) {
                rguNotify.add('Invitations sent');
                $scope.sent = true;
            }).error(function (errors) {
            console.log('Error sending invitation emails', errors)
        });

    };

    if (location.href.includes('facebook'))
        $scope.navTo('facebook');


});

