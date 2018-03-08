var _userApp = getApp('UserModule');

_userApp.factory('AuthorizationService', ['$http', '$q', '$cookies', '$interval', '$timeout', function ($http, $q, $cookies, $interval, $timeout) {
    var _authorized = false;

    var _accessToken = $cookies.get('access_token');
    if (_accessToken) {
        _authorized = true;
    }

    var _isAuthorized = function () {
        return ($cookies.get('access_token') ? true : false);
    }

    var _auth = function (accessToken) {
        _authorized = true;
        $cookies.put('access_token', accessToken, { path: '/' });

    }

    var _unAuth = function () {
        _authorized = false;
        $cookies.remove('access_token', { path: '/' });
    }

    var _verifyUser = function () {
        var deferer = $q.defer();

        $http.post('/api/Account/VerifyUser')
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
                deferer.reject();
            })

        return deferer.promise;
    }

    var _signInLocal = function (username, password) {
        var deferer = $q.defer();
        
        var data = {
            username: username.toLowerCase(),
            password: password,
            grant_type: 'password'
        };

        $http.post('/token', data, {
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            transformRequest: function (obj) {
                var str = [];
                for (var p in obj)
                    str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
                return str.join("&");
            }
        }).success(function (response) {
            if (!response.success) {
                deferer.reject(response);
            } else {
                _auth(response.access_token)
                deferer.resolve();
            }
        }).error(function (errors, status) {
            deferer.reject(errors);
        })

        return deferer.promise;
    }

    var _signUpLocal = function (registerModel) {
        var deferer = $q.defer();

        console.log(registerModel)

        $http.post('/api/Account/Register', registerModel)
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            });

        return deferer.promise;
    }

    var _prepareSignUpExternal = function (externalAccessToken, externalProvider) {
        $cookies.put('register_external', externalAccessToken, { path: '/' });
        $cookies.putObject('external_provider', externalProvider, { path: '/' });
        window.location.href = '/User/SignUp';
    }

    var _getExternalProvider = function () {
        var provider = $cookies.getObject('external_provider');
        $cookies.remove('external_provider', { path: '/' });
        return provider;
    }

    var externalAccessToken = null;
    var _isSignUpExternal = function () {
        externalAccessToken = $cookies.get('register_external');
        $cookies.remove('register_external', { path: '/' })
        return externalAccessToken != undefined && externalAccessToken != null;
    }

    var _getSignUpExternal = function () {
        var deferer = $q.defer();

        $http.get('/api/Account/ExternalLoginInfo', {
            headers: {
                Authorization: externalAccessToken
            }
        }).success(function (info) {
            deferer.resolve(info);
        }).error(function (errors, status) {
            __promiseHandler.Error(errors, status);
        });

        return deferer.promise;
    }

    var _getExternalLoginProviders = function () {
        var deferer = $q.defer();

        $http.get('/api/Account/ExternalLogins', { params: { returnUrl: '/User/ExternalLoginSuccess' } })
            .success(function (providers) {
                deferer.resolve(providers);
            }).error(function (errors, status) {
                deferer.reject();
            })

        return deferer.promise;
    }

    var _signOutExternal = function () {
        var deferer = $q.defer();

        $http.post('/Api/Account/SignOutExternal')
            .success(function () {
                deferer.resolve();
            })
            .error(function (errors, status) {
                __promiseHandler.Error(errors, status);
            })

        return deferer.promise;
    }

    var _externalLogin = function (provider, callback) {
        var deferer = $q.defer();
        var resolved = false;
        window.__authCallback = function (fragment) {
            resolved = true;
            var copy = {};
            for (var i in fragment) {
                copy[i] = fragment[i];
            }
            deferer.resolve(copy);
        }

        _signOutExternal().then(function () {
            var oauthWindow = window.open(provider.Url, "_blank");
            var checkClosed = $interval(function () {
                if (oauthWindow.closed) {
                    $interval.cancel(checkClosed)
                    $timeout(function () {
                        if (!resolved) {
                            deferer.reject('User cancelled');
                        }
                    }, 100)
                }
            }, 200);
        });

        return deferer.promise;
    }

    var _externalLoginWithExternalBearer = function () {
        var deferer = $q.defer();

        $http.post('/api/Account/LoginWithExternalBearer', null, {
            headers: {
                Authorization: externalAccessToken
            }
        }).success(function (accessToken) {
            _auth(accessToken);
            deferer.resolve();
        }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        });

        return deferer.promise;
    }

    var _registerExternal = function (model) {
        var deferer = $q.defer();

        $http.post('/api/Account/RegisterExternal', model, {
            headers: {
                Authorization: externalAccessToken
            }
        }).success(function (response) {
            deferer.resolve(response);
        }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        });

        return deferer.promise;
    }

    var _getUserAuthInfo = function (accessToken) {
        var deferer = $q.defer();

        $http.get('/api/Account/UserInfo', {
            headers: {
                Authorization: accessToken
            }
        }).success(function (response) {
            console.log(response);
            deferer.resolve(response);
        }).error(function (errors, status) {
            deferer.reject();
        })

        return deferer.promise;
    }

    var _logout = function () {
        var deferer = $q.defer();

        $http.post('/api/Account/Logout', null).success(function () {
            _unAuth();
            deferer.resolve();
        }).error(function (errors, status) {
            deferer.reject();
        })

        return deferer.promise;
    }

    var _getAllSecurityQuestions = function () {
        var deferer = $q.defer();

        $http.get('/Api/Account/SecurityQuestions')
            .success(function (questions) {
                deferer.resolve(questions);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }
   

    var _verifyPhoneNumber = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Account/Verify/PhoneNumber', model)
            .success(function (requestId) {
                deferer.resolve(requestId);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _verifyPIN = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Account/Verify/PIN', model)
            .success(function (verifiedToken) {
                deferer.resolve(verifiedToken);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _validateRegistrationInfo = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Account/ValidateRegistrationInfo', model)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _validateExternalRegistrationInfo = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Account/ValidateExternalRegistrationInfo', model)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _checkPIN = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Account/Verify/CheckPIN', model)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _checkSetPIN = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Account/Verify/CheckSetPIN', model)
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }
    var _setStaticPIN = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Account/Verify/SetStaticPIN', model)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }


    var _isLinkedWithBusinessAccount = function (hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/Account/IsLinkedWithBusinessAccount', { params: { hideAjaxLoader: hideAjaxLoader } })
            .success(function (linked) {
                deferer.resolve(linked);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _checkDuplicatedEmail = function (model, hideAjaxLoader) {
        var deferer = $q.defer();

        $http.post('/Api/Account/CheckDuplicatedEmail', model, { params: { hideAjaxLoader: hideAjaxLoader } })
            .success(function (duplicated) {
                deferer.resolve(duplicated);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _verifySecurityQuestions = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Account/VerifySecurityQuestions', model)
            .success(function (accountInfo) {
                deferer.resolve(accountInfo);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _forgotPassword = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Account/ForgotPassword', model)
            .success(function (verificationRequestId) {
                deferer.resolve(verificationRequestId);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _resendResetPasswordToken = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Account/ResendResetPasswordToken', model)
            .success(function (verificationRequestId) {
                deferer.resolve(verificationRequestId);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _verifyResetPasswordToken = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Account/VerifyResetPasswordToken', model)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }
   
    var _resetPassword = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/Account/ResetPassword', model)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _sendVerifyEmail = function (email) {
        var deferer = $q.defer();
        $http.post('/Api/Account/SendVerifyEmail', { Email: email })
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })
        return deferer.promise;
    }

     var _sendSMS = function () {
        var deferer = $q.defer();
        $http.post('/Api/Account/SendSMS')
            .success(function (requestId) {
                deferer.resolve(requestId);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })
        return deferer.promise;
    }
    var _getQuestionByEmail = function (email) {
        var deferer = $q.defer();

        $http.post('/Api/Account/GetQuestionsByEmail', { Email: email })
            .success(function (requestId) {
                deferer.resolve(requestId);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }
    var _lockResetPassword = function (email) {
        var deferer = $q.defer();

        $http.post('/Api/Account/LockResetPassword', { Email: email })
            .success(function (requestId) {
                deferer.resolve(requestId);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _getOutsiteById = function (outsiteId) {
        var deferer = $q.defer();
       

        $http.post('/Api/Account/GetOutsiteById', { OutsiteId: outsiteId })
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _insertOutsite = function (outsite) {
        var deferer = $q.defer();


        $http.post('/Api/Account/InsertOutsite', outsite)
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }
    // Outsite campaign Id

    var _getOutsiteSyncByCampaignId = function () {
        var deferer = $q.defer();
        $http.get('/Api/Account/GetOutsiteSyncByCampaignId')
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _getOutsiteSyncByUserId = function () {
        var deferer = $q.defer();

        $http.get('/Api/Account/GetOutsiteByUserId')
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }
    //GetOutsiteSynMailNotiByCampaignId
    var _getOutsiteSyncByCampaignId = function (outsite) {
        var deferer = $q.defer();

        $http.post('/Api/Account/GetOutsiteSyncByCampaignId', outsite)
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }
    var _updateOutsiteSync= function (model) {
        var deferer = $q.defer();


        $http.post('/Api/Account/UpdateOutsiteEmailSync', model)
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }


    var _getListHandShakeOutsite = function (outsite) {
        var deferer = $q.defer();

        $http.post('/Api/Account/GetListHandShakeOutsite', outsite)
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }
//
    var _getPrivacy = function (accountId) {
        var deferer = $q.defer();

        $http.get('/api/Account/GetProfilePrivacyByAccountId', { params: { accountId: accountId } })
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }
    //
    var _updatePrivacy = function (privacy) {
        var deferer = $q.defer();

        $http.post('/Api/Account/UpdateProfilePrivacyByAccountId', privacy)
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }
   //
    return {
        IsAuthorized: _isAuthorized,
        Auth: _auth,
        VerifyUser: _verifyUser,
        SignInLocal: _signInLocal,
        SignUpLocal: _signUpLocal,
        GetExternalLoginProviders: _getExternalLoginProviders,
        GetExternalProvider: _getExternalProvider,
        SignOutExternal: _signOutExternal,
        ExternalLogin: _externalLogin,
        ExternalLoginWithExternalBearer: _externalLoginWithExternalBearer,
        PrepareSignUpExternal: _prepareSignUpExternal,
        IsSignUpExternal: _isSignUpExternal,
        GetSignUpExternal: _getSignUpExternal,
        RegisterExternal: _registerExternal,
        GetUserAuthInfo: _getUserAuthInfo,
        GetAllSecurityQuestions: _getAllSecurityQuestions,
        Logout: _logout,
        VerifyPhoneNumber: _verifyPhoneNumber,
        VerifyPIN: _verifyPIN,
        ValidateRegistrationInfo: _validateRegistrationInfo,
        ValidateExternalRegistrationInfo: _validateExternalRegistrationInfo,
        CheckSetPIN: _checkSetPIN,
        CheckPIN: _checkPIN,
        IsLinkedWithBusinessAccount: _isLinkedWithBusinessAccount,
        CheckDuplicatedEmail: _checkDuplicatedEmail,
        VerifySecurityQuestions: _verifySecurityQuestions,
        ForgotPassword: _forgotPassword,
        ResendResetPasswordToken: _resendResetPasswordToken,
        VerifyResetPasswordToken: _verifyResetPasswordToken,
        ResetPassword: _resetPassword,
        SendVerifyEmail: _sendVerifyEmail,
        SendSMS: _sendSMS,
        GetQuestionByEmail: _getQuestionByEmail,
        SetStaticPIN: _setStaticPIN,

        GetOutsiteById: _getOutsiteById,
        GetOutsiteSyncByCampaignId: _getOutsiteSyncByCampaignId,
        GetListHandShakeOutsite: _getListHandShakeOutsite,
        InsertOutsite: _insertOutsite,
        GetOutsiteSyncByUserId: _getOutsiteSyncByUserId,
        UpdateOutsiteSync: _updateOutsiteSync,
        GetPrivacy: _getPrivacy,
        UpdatePrivacy: _updatePrivacy,
        LockResetPassword: _lockResetPassword
    }
}])
