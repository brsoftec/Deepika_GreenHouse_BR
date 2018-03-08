var regitAbout = angular.module(regitGlobal.ngApp);

regitAbout.run(['$rootScope', '$cookies', 'AuthorizationService', function ($rootScope, _$cookies, _authService) {
    $rootScope.authorized = false;
    $rootScope.authorized = _authService.IsAuthorized();
}]);

regitAbout.controller("ContactController", ['$scope', function ($scope) {
    $scope.view = {openingContact: false};
    $scope.openContact = function (event) {
        $scope.view.openingContact = true;

    };
    $scope.toggleContact = function () {
        $('.contact-modal').addClass('open');
        $scope.view.openingContact = !$scope.view.openingContact;
    };
    $scope.closeContact = function (event) {
        $scope.view.openingContact = false;
    };
}]);

regitAbout.controller("PublicSigninController", ['$scope', '$rootScope', '$timeout', '$http', 'AuthorizationService', '$cookies', function ($scope, $rootScope, $timeout, $http, _authService, $cookies) {

    $scope.providers = [];
    $scope.provider = {};

    $scope.signModel = {Email: null, Password: null};

    $scope.view = {
        openingSignupPicker: false
    };

    $scope.openSignupPicker = function () {
        $scope.view.openingSignupPicker = true;
    };
    $scope.closeSignupPicker = function () {
        $scope.view.openingSignupPicker = false;
    };
    $scope.toggleSignupPicker = function () {
        $scope.view.openingSignupPicker = !$scope.view.openingSignupPicker;
    };

    $scope.selectTier = function (tier) {
        if (tier === 'business') {
            location.href = '/BusinessAccount/Signup';
        } else {
            location.href = '/User/Signup';
        }
    };

    $scope.$on('document::click', function (ev, args) {
        $timeout(function () {
            $scope.closeSignupPicker();
        });

    });

    $scope.showMessage = function (msg) {
        $('.signin-message-text').html(msg);
    };

    $scope.signIn = function () {

        if (!$scope.signModel.Email) {
            $scope.showMessage('Please enter email and password');
            return;
        }
        var re = /^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})$/i;

        if (!re.test($scope.signModel.Email)) {
            $scope.showMessage('Invalid email');
            return;
        }
        if (!$scope.signModel.Password || $scope.signModel.Password.trim().length < 6) {
            $scope.showMessage('Invalid email or password');
            return;
        }
        $scope.showMessage('Signing in...');
        _authService.SignInLocal($scope.signModel.Email, $scope.signModel.Password)
            .then(function () {

                    var hideIntro = window.localStorage.getItem('hideIntro');
                    if (hideIntro && hideIntro !== 'permanent') {
                        window.localStorage.removeItem('hideIntro');
                    }

                    $rootScope.authorized = _authService.IsAuthorized();
                    window.location.href = "/";
                },
                function (errors) {
                    if (errors.error_description == 'EMAIL_NOT_VERIFIED') {
                        window.location.href = '/User/VerifyingEmail?email=' + $scope.signModel.Email;
                    } else if (errors.error_description == 'DISABLE_USER_BY_MAIL') {
                        window.location.href = '/User/DisableUserByEmail?email=' + $scope.signModel.Email;
                    } else {
                        $scope.showMessage(errors.error_description);
                    }
                });

        return true;
    };


}]);

regitAbout.directive('signupFormEmail', function ($timeout) {
    return {
        restrict: 'C',
        //templateUrl: 'signup-picker.html',
        link: function (scope, element, attrs) {
            scope.$watch('view.openingSignupPicker', function (value) {
                if (value === true) {
                    $timeout(function () {
                        element[0].focus();
                    });
                }
            });
        }
    };
});

regitAbout.controller("PublicSignupController", ['$scope', '$rootScope', '$http', 'AuthorizationService', '$cookies', function ($scope, $rootScope, $http, _authService, $cookies) {
    $scope.signup = {
        email: '',
        firstName: '',
        lastName: '',
        password: '',
        repass: ''
    };
    $scope.signupError = {
        email: false,
        firstName: false,
        lastName: false,
        password: false,
        repass: false
    };
    $scope.signupView = {
        dirty: false,
        signingUp: false,
        signupError: false,
        signingIn: false
    };


    $scope.signupBusiness = function () {
        $scope.view.exiting = true;
        //location.href = '/BusinessAccount/Signup';
    };

    $scope.onSignupInput = function (field) {
        console.log($scope.view.exiting);
        if ($scope.view.exiting) return;
        $scope.signupView.dirty = true;
        if (field === 'email') {
            var email = $scope.signup.email;
            if (!email.length) {
                $scope.signupError.email = 'null';
            } else if (!email.match(/[a-z0-9_\-.]+@[a-z0-9_\-.]+\.([a-z]{2,})/gi)) {
                $scope.signupError.email = 'invalid';
            } else {
                $http.get('/Api/Account/Exists', {
                    params: {
                        email: email
                    }
                }).success(function (response) {
                    if (response.success)
                        $scope.signupError.email = 'taken';
                    else
                        $scope.signupError.email = false;
                }).error(function (response) {
                    console.log(response);
                });
            }
        } else if (field === 'firstName')
            $scope.signupError.firstName = !$scope.signup.firstName.length;
        else if (field === 'password')
            $scope.signupError.password = !$scope.signup.password.length ? 'null' : $scope.signup.password.length < 8 ? 'length' : false;
        else if (field === 'repass')
            $scope.signupError.repass = !$scope.signup.repass.length ? 'null' : $scope.signup.repass !== $scope.signup.password  ? 'mismatch' : false;
    };
    $scope.canSignup = function () {
        return $scope.signupView.dirty && !$scope.signupError.email && !$scope.signupError.firstName && !$scope.signupError.password && !$scope.signupError.repass;
    };
    $scope.signupSubmit = function () {
        if ($scope.canSignup()) {
            $scope.signupView.signingUp = true;
            $http.post('/Api/Account/Signup', {
                email: $scope.signup.email,
                firstName: $scope.signup.firstName,
                lastName: $scope.signup.lastName,
                password: $scope.signup.password
            }).success(function (response) {
                $scope.signupView.signingUp = false;
                if (response.success) {
                    $scope.signupView.signingIn = true;
                    _authService.SignInLocal($scope.signup.email, $scope.signup.password)
                        .then(function () {
                                $rootScope.authorized = _authService.IsAuthorized();
                                window.location.href = "/";
                            },
                            function (response) {
                                $scope.signupView.signingIn = false;
                                $scope.signupView.signupError = response.error_description;
                                console.log(response);
                            });
                }
                else {
                    $scope.signupView.signupError = response.message;
                }
            }).error(function (response) {
                $scope.signupView.signupError = response ? response.message : '';
                console.log(response);
            });
        }
    };
}]);

regitAbout.directive('rguSpinner', function () {
        return {
            restrict: 'E',
            // template: '<span class="rgu-spinner"></span>',
            template: '',
            link: function (scope, element, attrs) {
                /*                var opts = {
                                    lines: 7 // The number of lines to draw
                                    , length: 3 // The length of each line
                                    , width: 3 // The line thickness
                                    , radius: 4 // The radius of the inner circle
                                    , scale: 1 // Scales overall size of the spinner
                                    , corners: 1 // Corner roundness (0..1)
                                    , color: '#000' // #rgb or #rrggbb or array of colors
                                    , opacity: 0.25 // Opacity of the lines
                                    , rotate: 0 // The rotation offset
                                    , direction: 1 // 1: clockwise, -1: counterclockwise
                                    , speed: 1 // Rounds per second
                                    , trail: 60 // Afterglow percentage
                                    , fps: 20 // Frames per second when using setTimeout() as a fallback for CSS
                                    , zIndex: 2e9 // The z-index (defaults to 2000000000)
                                    , className: 'spinner' // The CSS class to assign to the spinner
                                    , top: '50%' // Top position relative to parent
                                    , left: '50%' // Left position relative to parent
                                    , shadow: false // Whether to render a shadow
                                    , hwaccel: false // Whether to use hardware acceleration
                                    , position: 'relative' // Element positioning
                                };  */
                var opts = {
                    lines: 20 // The number of lines to draw
                    , length: 4 // The length of each line
                    , width: 12 // The line thickness
                    , radius: 25 // The radius of the inner circle
                    , scale: 0.2 // Scales overall size of the spinner
                    , corners: 1 // Corner roundness (0..1)
                    , color: '#00adef' // #rgb or #rrggbb or array of colors
                    , opacity: 0 // Opacity of the lines
                    , rotate: 0 // The rotation offset
                    , direction: 1 // 1: clockwise, -1: counterclockwise
                    , speed: 1 // Rounds per second
                    , trail: 60 // Afterglow percentage
                    , className: 'spinner' // The CSS class to assign to the spinner

                };
                var spinner = new Spinner(opts).spin(element.get(0));
            }
        };
    })
;


regitAbout.controller("ConfirmSMSController", ['$scope', '$rootScope', '$http', 'AuthorizationService', '$cookies', function ($scope, $rootScope, $http, _authService, $cookies) {

    //sms login new
    $scope.confirmSMS =
        {
            StaticPIN: '',
            RequestId: '',
            PIN: ''
        };

    $scope.errors = {PIN: null};

    $scope.GetConfirmSMS = function () {
        _authService.SendSMS().then(function (rs) {
            $scope.confirmSMS.RequestId = rs;
        });
    };
    $scope.checkPinSMS = function (value) {
        if ($scope.confirmSMS.PIN == '') {
            // $scope.errors.PIN = $rootScope.translate('PIN_Error_Required');
            $scope.errors.PIN = 'Please enter your PIN';
            return;
        }

        var model = {
            StaticPIN: '',
            RequestId: value,
            PIN: $scope.confirmSMS.PIN
        };
        _authService.CheckSetPIN(model).then(function () {
            window.location.href = "/";
        }, function (errors) {

            $scope.errors.PIN = 'Incorrect PIN. Please re-enter';
        });
    };


}]);
