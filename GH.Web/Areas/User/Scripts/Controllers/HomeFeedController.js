var myApp = getApp("myApp", true);
// var myApp = angular.module('myApp');

myApp.controller("HomeFeedController", function ($scope, $http) {

    $scope.view = $scope.view || {};
    if (regitGlobal.hasOwnProperty('viewPreferences')) {
        $scope.view.preferences = {
            showIntroSlides: regitGlobal.viewPreferences.introSlides,
            showBusinessIntroSlides: regitGlobal.viewPreferences.businessIntroSlides,
            hideIntroVault: !regitGlobal.viewPreferences.introVault,
            hideIntroBusiness: !regitGlobal.viewPreferences.introBusiness
        };
    } else {
        $scope.view.preferences = {
            showIntroSlides: false,
            showBusinessIntroSlides: false,
            hideIntroVault: true,
            hideIntroBusiness: true
        };
    }


    $scope.updateViewPreferences = function (pref) {
        var prefName = undefined;
        var prefValue = undefined;
        switch (pref) {
            case 'introSlides':
                prefName = 'ShowIntroSlides';
                prefValue = !$scope.view.preferences.showIntroSlides;
                break;
            case 'businessIntroSlides':
                prefName = 'ShowBusinessIntroSlides';
                prefValue = !$scope.view.preferences.showBusinessIntroSlides;
                break;
            case 'introVault':
                prefName = 'ShowIntroVault';
                prefValue = !$scope.view.preferences.hideIntroVault;
                break;
            case 'introBusiness':
                prefName = 'ShowIntroBusiness';
                prefValue = !$scope.view.preferences.hideIntroBusiness;
                break;
        }
        if (prefName) {
            $http.post('/Api/AccountSettings/ViewPreference', {
                prefName: prefName, prefValue: prefValue
            }).success(function () {
            });
        }
    };

    $scope.$watch('view.preferences.hideIntroVault', $scope.updateViewPreferences.bind(null, 'introVault'));
    $scope.$watch('view.preferences.hideIntroBusiness', $scope.updateViewPreferences.bind(null, 'introBusiness'));

});

myApp.controller("HomeAlertController", function ($scope, $timeout, $http, rguNotify) {

    $scope.homeAlertView = {
        showingCountryEditor: false,
        confirmedCountry: false,
        errorUpdateCountry: false,
        errorResendVerifyEmail: false,
        validatePhone: {
            invalid: true
        },
        verifyPhone: {
            validNumber: false,
            seding: false,
            sent: false,
            error: false,
            pin: '',
            saving: false,
            complete: false
        }
    };
    var countryName = regitGlobal.homeAlert ? regitGlobal.homeAlert.countryName : '';
    if (countryName === 'Viet Nam')
        countryName = 'Vietnam (Việt Nam)';

    $scope.homeAlert = {
        model: {
            country: countryName,
            phone: ''
        }
    };

    $scope.setCountry = function (country) {
        if (country) {
            $scope.homeAlertView.errorUpdateCountry = false;
            country = country.replace(/\(.+\)/g, '');
            if (country === 'Vietnam') country = 'Viet Nam';
            $http.post('/Api/Profile/Set', {
                key: 'Country', value: country, isInitial: true
            }).success(function (response) {
                if (response.success) {
                    rguNotify.add('Updated profile successfully');
                } else {
                    $scope.homeAlertView.errorUpdateCountry = true;
                }
                $scope.homeAlertView.showingCountryEditor = false;
                $scope.homeAlertView.confirmedCountry = true;
            }).error(function (response) {
                console.log(response);
                $scope.homeAlertView.errorUpdateCountry = true;
            });
        } else {
            $scope.homeAlertView.confirmedCountry = true;
            $scope.homeAlertView.showingCountryEditor = false;
            $http.post('/Api/Profile/Status', {
                key: 'LocationDetected', value: false
            }).success(function (response) {
            }).error(function (response) {
                console.log(response);
            });
        }
    };
    $scope.resendVerifyEmail = function () {

        $scope.homeAlertView.errorResendVerifyEmail = false;
        $http.post('/Api/Account/VerifyEmail/Resend')
            .success(function (response) {
                if (response.success) {
                    rguNotify.add('Verification email resent');
                } else {
                    $scope.homeAlertView.errorResendVerifyEmail = true;
                }
            }).error(function (response) {
            console.log(response);
            $scope.homeAlertView.errorResendVerifyEmail = true;
        });

    };

    if (regitGlobal.homeAlert && regitGlobal.homeAlert.noPhone) {
        $scope.$watch('homeAlertView.validatePhone.invalid', function (value, oldVal) {
            if (value === false) {
                $timeout(function () {
                    var phone = $scope.homeAlert.model.phone;
                    if (!phone.length) return;
                    $scope.homeAlertView.verifyPhone.validNumber = true;
                }, 200);
            } else {
                $scope.homeAlertView.verifyPhone.validNumber = false;

            }
        });

    }

    $scope.verifyPhone = function (resend) {
        $scope.homeAlertView.verifyPhone.error = false;
        var phoneNumber = $scope.homeAlert.model.phone;
        if (!phoneNumber) return;
        $scope.homeAlertView.verifyPhone.sending = true;
        $http.post('/Api/Account/VerifyPhone/Send?phoneNumber=' + phoneNumber)
            .success(function (response) {
                $scope.homeAlertView.verifyPhone.sending = false;
                if (response.success) {
                    console.log(response.data);
                    $scope.homeAlertView.verifyPhone.sent = true;
                    $scope.homeAlertView.verifyPhone.validNumber = false;
                } else {
                    $scope.homeAlertView.verifyPhone.error = true;
                }
            }).error(function (response) {
            console.log(response);
            $scope.homeAlertView.verifyPhone.sending = false;
            $scope.homeAlertView.verifyPhone.error = true;
        });
    };
    $scope.verifyPhone = function (resend) {
        $scope.homeAlertView.verifyPhone.error = false;
        var phoneNumber = $scope.homeAlert.model.phone;
        if (!phoneNumber) return;
        $scope.homeAlertView.verifyPhone.sending = true;
        $http.post('/Api/Account/VerifyPhone/Send?phoneNumber=' + phoneNumber)
            .success(function (response) {
                $scope.homeAlertView.verifyPhone.sending = false;
                if (response.success) {
                    console.log(response.data);
                    $scope.homeAlertView.verifyPhone.requestId = response.data.requestId;
                    $scope.homeAlertView.verifyPhone.sent = true;
                    $scope.homeAlertView.verifyPhone.validNumber = false;
                } else {
                    $scope.homeAlertView.verifyPhone.error = 'Error sending verification message';
                }
            }).error(function (response) {
            console.log(response);
            $scope.homeAlertView.verifyPhone.sending = false;
            $scope.homeAlertView.verifyPhone.error = 'Error sending verification message';
        });
    };
    $scope.onPINInput = function () {
        $scope.homeAlertView.verifyPhone.pinError = false;
        var pin = $scope.homeAlertView.verifyPhone.pin;
        if (pin.length !== 4) return;
        $http.post('/Api/Account/VerifyPhone/Check', {
            phoneNumber: $scope.homeAlert.model.phone,
            requestId: $scope.homeAlertView.verifyPhone.requestId,
            token: pin
        })
            .success(function (response) {
                $scope.homeAlertView.verifyPhone.sending = false;
                if (response.success) {
                    console.log(response.data);
                    $scope.homeAlertView.verifyPhone.sent = false;
                    $scope.homeAlertView.verifyPhone.saving = true;
                    $http.post('/Api/Account/AccessProfile/PhoneNumber', {
                        phoneNumber: $scope.homeAlert.model.phone, isInitial: true
                    }).success(function (response) {
                        if (response.success) {
                            rguNotify.add('Phone number verified successfully');
                            $scope.homeAlertView.verifyPhone.saving = false;
                            $scope.homeAlertView.verifyPhone.complete = true;
                        } else {
                            console.log(response);
                            $scope.homeAlertView.verifyPhone.error = 'Error saving phone number';
                        }
                    }).error(function (response) {
                        console.log(response);
                        $scope.homeAlertView.verifyPhone.error = 'Error saving phone number';
                    });
                } else {
                    console.log(response);
                    if (response.error === 'token.incorrect') {
                        $scope.homeAlertView.verifyPhone.pinError = 'Incorrect PIN';
                    }
                    else if (response.error === 'token.expired')
                        $scope.homeAlertView.verifyPhone.pinError = 'PIN expired';
                    else if (response.error === 'token.cancelled')
                        $scope.homeAlertView.verifyPhone.pinError = 'PIN cancelled due to too many attempts';
                    else
                        $scope.homeAlertView.verifyPhone.pinError = 'Error checking PIN';
                }
            }).error(function (response) {
            console.log(response);
            $scope.homeAlertView.verifyPhone.pinError = 'Error checking PIN';
        });
    };

});

myApp.directive('verifyPhonePinInput', function ($timeout) {
        return {
            restrict: 'C',
            link: function (scope, el, attrs) {
                scope.$watch('homeAlertView.verifyPhone.sent', function (value) {
                    if (value === true) {
                        $timeout(function () {
                            el[0].focus();
                        });
                    }
                });
            }
        };
    }
);