var myApp = getApp("myApp", true);

myApp.getController('SupportController', function ($scope, rguModal, rguView) {

    // Retrieve Firebase Messaging object.
    const messaging = firebase.messaging();

    messaging.requestPermission()
        .then(function() {
            //console.log('Notification permission granted.');
            // Get Instance ID token. Initially this makes a network call, once retrieved
            // subsequent calls to getToken will return from cache.
            messaging.getToken()
                .then(function(currentToken) {
                    if (currentToken) {
                        console.log(currentToken);
                        // sendTokenToServer(currentToken);
                        // updateUIForPushEnabled(currentToken);
                        messaging.onMessage(function(payload) {
                            console.log("Message received. ", payload);
                            // ...
                        });

                    } else {
                        // Show permission request.
                        console.log('No Instance ID token available. Request permission to generate one.');
                        // Show permission UI.
                        // updateUIForPushPermissionRequired();
                        // setTokenSentToServer(false);
                    }
                })
                .catch(function(err) {
                    console.log('An error occurred while retrieving token. ', err);
                    showToken('Error retrieving Instance ID token. ', err);
                    setTokenSentToServer(false);
                });
        })
        .catch(function(err) {
            console.log('Unable to get permission to notify.', err);
        });

    $scope.supportConfig = {
        caseTypes: ['Account and Billing Support', 'Technical Suport'],
        caseSeverities: ['Low', 'Medium', 'High'],
        caseLanguages: ['English', 'Vietnamese'],
        caseContactMethods: ['Web', 'Email', 'Phone'],
        caseCategories: [
            { name: 'general', label: 'General' },
            { name: 'user', label: 'User Management' },
            { name: 'vault', label: 'Information Vault' },
            { name: 'network', label: 'Network' }
        ]
    };

    $scope.support = {
        cases: [

        ],
        newCase: null

    };

    $scope.createCase = function() {
        $scope.support.newCase = {
            type: 'Account and Billing Support',
            category: 'General',
            subject: '',
            description: '',
            severity: '',
            lang: 'English',
            contactBy: 'Web',
            status: 'new'
        };
        rguModal.openModal('support.case',$scope)
    };
    $scope.editCase = function(c) {
        $scope.support.editCase = c;
        rguModal.openModal('support.case.edit',$scope)
    };

    $scope.canSaveCase = function() {
        var c = $scope.support.newCase;
        return c.subject && c.description;
    };

    $scope.saveNewCase = function(close) {
        var c = $scope.support.newCase;
        c.id = Date.now();
        c.created = new Date;
        c.status = 'created';
        $scope.support.cases.push(c);
        close();
    };
});
