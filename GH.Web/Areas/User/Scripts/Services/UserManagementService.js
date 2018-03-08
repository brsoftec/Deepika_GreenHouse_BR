var _userApp = getApp('UserModule');

_userApp.factory('UserManagementService', ['$http', '$q', function ($http, $q) {

    var _hasLocalPassword = function () {
        var deferer = $q.defer();
        $http.get('/api/Account/HasLocalPassword')
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
            deferer.reject();
        })
        return deferer.promise;
    }

    var _changePassword = function (changePasswordModel, verifiedToken) {
        var deferer = $q.defer();
        var model = angular.copy(changePasswordModel);
        model.VerifiedToken = verifiedToken;
        $http.post('/api/Account/ChangePassword', model).success(function () {
            deferer.resolve();
        }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _updateProfile = function (model) {
        var deferer = $q.defer();
        var formData = new FormData();
        var keys = {
            DisplayName: 'DisplayName',
            Status: 'Status',
            Description: 'Description',
            City: 'City',
            Region: 'Region',
            Street: 'Street',
            ZipPostalCode: 'ZipPostalCode',
            Country: 'Country',

            Gender: 'Gender',
            Birthdate: 'Birthdate',

            PhotoUrl: 'PhotoUrl',
            UpdateFields: 'UpdateFields',
            ForAccount: 'ForAccount'
        }

        for (var key in keys) {
            if (model[keys[key]] != undefined && model[keys[key]] != null) {
                formData.append(keys[key], model[keys[key]]);
            }
        }

        $http.post('/Api/AccountSettings/Profile', formData,
            {
                transformRequest: angular.identity,
                headers: {'Content-Type': undefined}
            }
        ).success(function (response) {
                deferer.resolve(response);
        }).error(function (errors, status) {
            console.log(errors)
            __promiseHandler.Error(errors, status, deferer);
        });

        return deferer.promise;
    }
    var _getCurrentUserProfile = function () {
        var deferer = $q.defer();

        $http.get('/Api/AccountSettings/Profile')
            .success(function (response) {
                if (response.success)
                    deferer.resolve(response.data);
                else
                    console.log(response.message)
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        });

        return deferer.promise;
    }

    var _getCurrentUserPrivacies = function () {
        var deferer = $q.defer();

        $http.get('/Api/AccountSettings/Privacies')
            .success(function (response) {
                deferer.resolve(response)
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer)
        })

        return deferer.promise;
    }

    var _updateCurrentUserPrivacies = function (model) {
        var deferer = $q.defer();
        var basePrivacy = {
            FindMe: true,
            ShareMyActivity: true,
            ViewMyProfile: true,
            NotFollowBusinessSendMeAds: true,
            SendMeMessage: 'All',
            AutoDeleteMessage: false
        }

        __common.MergeObject(basePrivacy, model);

        $http.post('/Api/AccountSettings/Privacies', basePrivacy)
            .success(function (response) {
                deferer.resolve(response)
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer)
        })

        return deferer.promise;
    }

    var _getCurrentUserActivityLogSettings = function () {
        var deferer = $q.defer();

        $http.get('/Api/AccountSettings/ActivityLog')
            .success(function (response) {
                deferer.resolve(response)
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer)
        })

        return deferer.promise;
    }

    var _updateCurrentUserActivityLogSettings = function (model) {
        var deferer = $q.defer();
        var baseActLogSettings = {
            RecordAccess: true,
            RecordProfile: true,
            RecordAccount: true,
            RecordNetwork: true,
            RecordVault: true,
            RecordDelegation: true,
            RecordInteraction: true,
            RecordSocialActivity: true,
            RecordProfileBusiness: true,
            RecordAccountBusiness: true,
            RecordBusinessSystem: true,
            RecordCampaign: true,
            RecordWorkflow: true
        }

        __common.MergeObject(baseActLogSettings, model);

        $http.post('/Api/AccountSettings/ActivityLog', baseActLogSettings)
            .success(function (response) {
                deferer.resolve(response)
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer)
        })

        return deferer.promise;
    }

    var _getSecurityQuestionsAnswers = function () {
        var deferer = $q.defer();

        $http.get('/Api/AccountSettings/SecurityQuestions')
            .success(function (questionsAnswers) {
                deferer.resolve(questionsAnswers);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _updateSecurityQuestionsAnswers = function (model, verifiedToken) {
        var deferer = $q.defer();

        var body = angular.copy(model);
        body.VerifiedToken = verifiedToken;

        $http.post('/Api/AccountSettings/SecurityQuestions', body)
            .success(function () {
                deferer.resolve()
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer)
        })

        return deferer.promise;
    }

    var _getEncodedPhoneNumber = function () {
        var deferer = $q.defer();

        $http.get('/Api/AccountSettings/EncodedPhoneNumber')
            .success(function (phoneNumber) {
                deferer.resolve(phoneNumber);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _updatePhoneNumber = function (model, newPhonePIN, newPhoneRequestId, verifiedToken) {
        var deferer = $q.defer();

        var body = angular.copy(model);
        body.NewPhonePIN = newPhonePIN;
        body.NewPhoneRequestId = newPhoneRequestId;
        body.VerifiedToken = verifiedToken;

        $http.post('/Api/AccountSettings/PhoneNumber', body)
            .success(function (phoneNumber) {
                deferer.resolve(phoneNumber);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _updatePhoneNumberForBusinessAccount = function (model, newPhonePIN, newPhoneRequestId) {
        var deferer = $q.defer();

        var body = angular.copy(model);
        body.NewPhonePIN = newPhonePIN;
        body.NewPhoneRequestId = newPhoneRequestId;

        $http.post('/Api/AccountSettings/Business/PhoneNumber', body)
            .success(function (phoneNumber) {
                deferer.resolve(phoneNumber);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })
        return deferer.promise;
    }
    /*Pin Code */

    var _getEncodedPinCode = function () {
        var deferer = $q.defer();
        $http.get('/Api/AccountSettings/EncodedPinCode')
            .success(function (pinCode) {
                deferer.resolve(pinCode);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })
        return deferer.promise;
    }

    var _updatePinCode = function (model, verifiedToken) {
        var deferer = $q.defer();
        var body = angular.copy(model);
        body.VerifiedToken = verifiedToken;
        $http.post('/Api/AccountSettings/PinCode', body)
            .success(function (pinCode) {
                deferer.resolve(pinCode);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }
    /*End Pin Code */
    var _updateAccountNotificationSetting = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/AccountSettings/NotificationSettings', model)
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _updateBusinessPrivacyAccount = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/AccountSettings/Business/Privacy', model)
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _checkSocialNetworks = function () {
        var deferer = $q.defer();

        $http.get('/Api/Social/CheckSocialNetworks')
            .success(function (check) {
                deferer.resolve(check);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _checkBASocialNetworks = function (id) {
        var deferer = $q.defer();

        $http.get('/Api/Social/CheckBASocialNetworks', {params: {id: id}})
            .success(function (check) {
                deferer.resolve(check);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _linkAccount = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/AccountLink/Link', model)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _unlinkAccount = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/AccountLink/Unlink', model)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _getEncodedEmail = function (hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/AccountSettings/EncodedEmail', {params: {hideAjaxLoader: hideAjaxLoader}})
            .success(function (encodedEmail) {
                deferer.resolve(encodedEmail);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _updateEmail = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/AccountSettings/Email', model)
            .success(function (encodedEmail) {
                deferer.resolve(encodedEmail);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _getUserProfile = function (id) {
        var deferer = $q.defer();

        $http.get('/Api/AccountSettings/GetShortProfile', {params: {id: id}})
            .success(function (profile) {
                deferer.resolve(profile);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _getFolloeeOfCurrentUser = function (paramter) {
        var deferer = $q.defer();
        $http.get('/Api/AccountSettings/Followee', {params: paramter})
            .success(function (followees) {
                deferer.resolve(followees);
            })
            .error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            });

        return deferer.promise;
    }

    var _getFollowTransactions = function (paramter) {
        var deferer = $q.defer();
        $http.get('/Api/AccountSettings/GetFollowTransactions', {params: paramter})
            .success(function (transactions) {
                deferer.resolve(transactions);
            })
            .error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            });

        return deferer.promise;
    }

    var _checkPhoneNumber = function (model) {
        var deferer = $q.defer();
        $http.post('/Api/AccountSettings/CheckPhoneNumber', model)
            .success(function (phoneNumber) {
                deferer.resolve(phoneNumber);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _getPhoneCode = function (model) {
        var deferer = $q.defer();
        $http.post('/Api/AccountSettings/GetPhoneCode', model)
            .success(function (phoneNumber) {
                deferer.resolve(phoneNumber);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }
    var _validPhone = function (model) {
        var deferer = $q.defer();
        $http.post('/Api/AccountSettings/ValidPhoneNumber', model)
            .success(function (phoneNumber) {
                deferer.resolve(phoneNumber);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })
        return deferer.promise;
    }

    var _getCurrentBusinessProfile = function () {
        var deferer = $q.defer();

        $http.get('/Api/AccountSettings/ProfileBusinessFull')
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        });

        return deferer.promise;
    }

    var _getProfileFromVault = function () {
        var deferer = $q.defer();
        $http.get('/api/Account/GetProfileByAccountId')
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    var _updateProfileFromVault = function (model) {
        var deferer = $q.defer();
        var profile = new Object();
        var bsonString = JSON.stringify(model);
        profile.BsonString = bsonString;
        $http.post('/api/Account/UpdateProfileByAccountId', profile)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })
        return deferer.promise;
    }

    // Close spend
    var _disableUser = function (disableUserModel) {
        var deferer = $q.defer();
        var model = angular.copy(disableUserModel);
        $http.post('/api/DisableUser/Disabled', model)
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        });
        return deferer.promise;
    };

    var _getDisableUsers = function (disableUserParameter) {
        var deferer = $q.defer();
        $http.get("/api/DisableUser/GetDisabledUsers", {params: disableUserParameter})
            .success(function (response) {
                deferer.resolve(response);
            })
            .error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            });
        return deferer.promise;
    };

    var _enableUsers = function (users) {
        var deferer = $q.defer();
        var model = angular.copy(users);
        $http.post("/api/DisableUser/EnableUser", model)
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        });
        return deferer.promise;
    };

    var _enableUser = function (id) {
        var deferer = $q.defer();
        $http.post("/api/DisableUser/Enable", null, {params: {userId: id}})
            .success(function (response) {
                deferer.resolve(response);
            })
            .error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            });
        return deferer.promise;
    };
    var _isDisableUser = function () {
        var deferer = $q.defer();
        $http.get("/api/DisableUser/IsDisabled")
            .success(function (response) {
                deferer.resolve(response);
            })
            .error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            });
        return deferer.promise;
    };
    var _findUsersByKeyword = function (keyword) {
        var deferer = $q.defer();
        $http.get("/api/DisableUser/FindUsersByKeyword", {params: {keyword: keyword}})
            .success(function (response) {
                deferer.resolve(response);
            })
            .error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            });
        return deferer.promise;
    };

    // Vu

    var _getDisabledUserByEmail = function (email) {
        var deferer = $q.defer();

        $http.get('/api/DisableUser/GetDisabledUserByEmail', {params: {email: email}})
            .success(function (rs) {
                deferer.resolve(rs);
            }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }
    // End close spend

    return {
        HasLocalPassword: _hasLocalPassword,
        ChangePassword: _changePassword,
        GetCurrentUserProfile: _getCurrentUserProfile,
        UpdateProfile: _updateProfile,
        GetCurrentUserPrivacies: _getCurrentUserPrivacies,
        UpdateCurrentUserPrivacies: _updateCurrentUserPrivacies,
        GetCurrentUserActivityLogSettings: _getCurrentUserActivityLogSettings,
        UpdateCurrentUserActivityLogSettings: _updateCurrentUserActivityLogSettings,
        GetSecurityQuestionsAnswers: _getSecurityQuestionsAnswers,
        UpdateSecurityQuestionsAnswers: _updateSecurityQuestionsAnswers,
        GetEncodedPhoneNumber: _getEncodedPhoneNumber,
        UpdatePhoneNumber: _updatePhoneNumber,
        GetEncodedPinCode: _getEncodedPinCode,
        UpdatePinCode: _updatePinCode,
        CheckSocialNetworks: _checkSocialNetworks,
        CheckBASocialNetworks: _checkBASocialNetworks,
        LinkAccount: _linkAccount,
        UnlinkAccount: _unlinkAccount,
        UpdatePhoneNumberForBusinessAccount: _updatePhoneNumberForBusinessAccount,
        GetEncodedEmail: _getEncodedEmail,
        UpdateEmail: _updateEmail,
        GetUserProfile: _getUserProfile,
        CurrentUser: null,
        GetFolloeeOfCurrentUser: _getFolloeeOfCurrentUser,
        UpdateBusinessPrivacyAccount: _updateBusinessPrivacyAccount,
        UpdateAccountNotificationSetting: _updateAccountNotificationSetting,
        GetFollowTransactions: _getFollowTransactions,
        CheckPhoneNumber: _checkPhoneNumber,
        GetPhoneCode: _getPhoneCode,
        ValidPhone: _validPhone,
        GetCurrentBusinessProfile: _getCurrentBusinessProfile,
        GetProfileFromVault: _getProfileFromVault,
        UpdateProfileFromVault: _updateProfileFromVault,

        DisableUser: _disableUser,
        GetDisableUsers: _getDisableUsers,
        EnableUsers: _enableUsers,
        EnableUser: _enableUser,
        IsDisableUser: _isDisableUser,
        FindUsersByKeyword: _findUsersByKeyword,
        GetDisabledUserByEmail: _getDisabledUserByEmail
    }
}])
