var _businessApp = getApp('BusinessAccountModule', []);

_businessApp.factory('BusinessAccountService', ['$http', '$q', function ($http, $q) {

    var _registerBusinessAccount = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/BusinessAccount/Register', model)
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _validateRegistrationInfo = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/BusinessAccount/ValidateRegistration', model)
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _getManagedFacebookPages = function () {
        var deferer = $q.defer();

        $http.get('/Api/AccountLink/ListFacebookPages')
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _connectToFacebookPage = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/AccountLink/ConnectToFacebookPage', model)
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _disconnectToFacebookPage = function () {
        var deferer = $q.defer();

        $http.post('/Api/AccountLink/DisconnectFacebookPage')
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _getConnectedFacebookPage = function () {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/GetConnectedFacebookPage')
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _searchMembersForInvitation = function (keyword, hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/SearchMembersForInvitation', { params: { keyword: keyword, hideAjaxLoader: hideAjaxLoader } })
            .success(function (users) {
                deferer.resolve(users);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _inviteMemberToBusiness = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/BusinessAccount/Invite', model)
            .success(function (invitation) {
                deferer.resolve(invitation);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _acceptInvitation = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/BusinessAccount/Invite/Accept', model)
           .success(function () {
               deferer.resolve();
           }).error(function (errors, status) {
               __promiseHandler.Error(errors, status, deferer);
           })

        return deferer.promise;
    }

    var _denyInvitation = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/BusinessAccount/Invite/Deny', model)
          .success(function () {
              deferer.resolve();
          }).error(function (errors, status) {
              __promiseHandler.Error(errors, status, deferer);
          })

        return deferer.promise;
    }

    var _getMembersInBusiness = function (hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/Members', { params: { hideAjaxLoader: hideAjaxLoader } })
          .success(function (result) {
              deferer.resolve(result);
          }).error(function (errors, status) {
              __promiseHandler.Error(errors, status, deferer);
          })

        return deferer.promise;
    }


        var _getMembersFollowInBusiness = function (businessId, hideAjaxLoader) {
        var deferer = $q.defer();

            $http.get('/Api/BusinessAccount/GetBusinessFollowers', { params: {businessId: businessId, hideAjaxLoader: hideAjaxLoader } })
          .success(function (result) {
              deferer.resolve(result);
          }).error(function (errors, status) {
              __promiseHandler.Error(errors, status, deferer);
          })

        return deferer.promise;
    }


    var _getAllRoles = function (hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/Roles', { params: { hideAjaxLoader: hideAjaxLoader } })
         .success(function (result) {
             deferer.resolve(result);
         }).error(function (errors, status) {
             __promiseHandler.Error(errors, status, deferer);
         })

        return deferer.promise;
    }

    var _BAPullFeed = function (start, take) {
        var deferer = $q.defer();

        $http.get('/Api/Social/GetBusinessFeed?start=' + start + '&take=' + take)
         .success(function (response) {
             defer.resolve(response);
         }).error(function (errors, status) {
             __promiseHandler.Error(errors, status, defer);
         });

        return defer.promise;
    }

    var _updateMembersInBusiness = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/BusinessAccount/UpdateMembers', model)
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var postNewFeedTobusiness = function (url, newPost) {
        var defer = $q.defer();

        var formData = new FormData();
        //append properties
        formData.append("Message", newPost.Message);
        formData.append("IsPostGreenHouse", newPost.IsPostGreenHouse);
        formData.append("IsPostFacebook", newPost.IsPostFacebook);
        formData.append("IsPostTwitter", newPost.IsPostTwitter);
        formData.append("NumberOfPhotos", newPost.Photos.length);
        formData.append("IsPrivate", newPost.Privacy.Private);
        //append photo file
        newPost.Photos.forEach(function (file, index) {
            formData.append("photo" + index, file.file);
        });

        $http.post(url, formData,
        {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        }).success(function (response) {
            defer.resolve(response);
        }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, defer);
        });

        return defer.promise;
    }

    var _EditFeedTobusiness = function (newPost) {
        var defer = $q.defer();

        var formData = new FormData();
        //append properties
        formData.append("Id", newPost.Id);
        formData.append("Message", newPost.Message);
        formData.append("IsPostGreenHouse", newPost.IsPostGreenHouse);
        formData.append("IsPostFacebook", newPost.IsPostFacebook);
        formData.append("IsPostTwitter", newPost.IsPostTwitter);
        formData.append("DeletedPhoto", newPost.DeletedPhoto);
        formData.append("IsPrivate", newPost.Privacy.Private);
        //append photo file
        newPost.Photos.forEach(function (file, index) {
            if (file.file) {
                formData.append("photo" + index, file.file);
            }
        });

        $http.post("/API/Social/BAEditPostFeed", formData,
        {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        }).success(function (response) {
            defer.resolve(response);
        }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, defer);
        });

        return defer.promise;
    }

    var _getBusinessPost = function (id, hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/Social/BusinessPost', { params: { id: id, hideAjaxLoader: hideAjaxLoader } })
            .success(function (post) {
                deferer.resolve(post);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _approveBusinessPost = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/BusinessAccount/ApprovePost', model)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _rejectBusinessPost = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/BusinessAccount/RejectPost', model)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _getPictureAlbum = function (businessId, hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/PictureAlbum', { params: { businessId: businessId, hideAjaxLoader: hideAjaxLoader } })
            .success(function (album) {
                deferer.resolve(album);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _uploadPictureAlbum = function (newPhotos, deletePhotos) {
        var deferer = $q.defer();
        var formData = new FormData();
        if (deletePhotos && deletePhotos.length > 0) {
            formData.append('DeletePhotos', deletePhotos);
        }
        if (newPhotos && newPhotos.length > 0) {
            for (var i = 0; i < newPhotos.length; i++) {
                formData.append('NewPhoto' + i, newPhotos[i]);
            }
        }
        $http.post('/Api/BusinessAccount/PictureAlbum', formData,
            {
                transformRequest: angular.identity,
                headers: { 'Content-Type': undefined }
            }
        ).success(function (response) {
            deferer.resolve(response);
        }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        });
        return deferer.promise;
    }

    var _getWorkTime = function (hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/WorkTime', { params: { hideAjaxLoader: hideAjaxLoader } })
            .success(function (worktime) {
                deferer.resolve(worktime);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _updateWorkTime = function (worktime) {
        var deferer = $q.defer();

        $http.post('/Api/BusinessAccount/WorkTime', worktime)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _getPublicProfile = function (id, hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/PublicProfile', { params: { businessId: id, hideAjaxLoader: hideAjaxLoader } })
            .success(function (profile) {
                deferer.resolve(profile);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }


    var _followBusiness = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/BusinessAccount/Follow', model)
            .success(function (followSummary) {
                deferer.resolve(followSummary);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _unfollowBusiness = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/BusinessAccount/Unfollow', model)
            .success(function (followSummary) {
                deferer.resolve(followSummary);
            })
            .error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            });

        return deferer.promise;
    }

    var _BAGetProfile = function () {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/BusinessAccountProfile')
            .success(function (profile) {
                deferer.resolve(profile);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _summarizeNumberOfFollowersByTime = function (fromDate, toDate, hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/SummarizeNumberOfFollowersByTime', { params: { FromDate: fromDate, ToDate: toDate, hideAjaxLoader: hideAjaxLoader } })
            .success(function (response) {
                deferer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _summarizeNumberOfFollowersByGenders = function (hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/SummarizeNumberOfFollowersByGenders', { params: { hideAjaxLoader: hideAjaxLoader } })
           .success(function (response) {
               deferer.resolve(response);
           }).error(function (errors, status) {
               __promiseHandler.Error(errors, status, deferer);
           })

        return deferer.promise;
    }

    var _summarizeNumberOfFollowersByCountries = function (hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/SummarizeNumberOfFollowersByCountries', { params: { hideAjaxLoader: hideAjaxLoader } })
             .success(function (response) {
                 deferer.resolve(response);
             }).error(function (errors, status) {
                 __promiseHandler.Error(errors, status, deferer);
             })

        return deferer.promise;
    }

    var _summarizeNumberOfFollowersByCities = function (hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/SummarizeNumberOfFollowersByCities', { params: { hideAjaxLoader: hideAjaxLoader } })
           .success(function (response) {
               deferer.resolve(response);
           }).error(function (errors, status) {
               __promiseHandler.Error(errors, status, deferer);
           })

        return deferer.promise;
    }

    var _getRolesOfCurrentUser = function (hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/RolesOfCurrentUser', { params: { hideAjaxLoader: hideAjaxLoader } })
            .success(function (roles) {
                deferer.resolve(roles);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _getWebsite = function (hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/WebsiteUrl', { params: { hideAjaxLoader: hideAjaxLoader } })
            .success(function (website) {
                deferer.resolve(website);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _updateWebsite = function (model) {
        var deferer = $q.defer();

        $http.post('/Api/BusinessAccount/WebsiteUrl', model)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }


    var _checkSocialNetworks = function () {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/CheckSocialNetworks')
            .success(function (check) {
                deferer.resolve(check);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    // VU
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
            RecordProfileBusiness: true,
            RecordAccountBusiness: true,
            RecordBusinessSystem: true,
            RecordCampaign: true,
            RecordWorkflow: true,

            RecordProfile: true,
            RecordAccount: true,
            RecordNetwork: true,
            RecordVault: true,
            RecordDelegation: true,
            RecordInteraction: true,
            RecordSocialActivity: true
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


    var _getPublicProfileFull = function (id, hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/PublicProfileFull', { params: { businessId: id, hideAjaxLoader: hideAjaxLoader } })
            .success(function (profile) {
                deferer.resolve(profile);
            }).error(function (errors, status) {
            console.log(errors)
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _getCompanyObjectDetails = function (hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/GetJsonCompanyDetails', { params: { hideAjaxLoader: hideAjaxLoader } })
            .success(function (profile) {
                deferer.resolve(profile);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }
    var _getCompanyObjectDetailsById = function (id, hideAjaxLoader) {
        var deferer = $q.defer();

        $http.get('/Api/BusinessAccount/GetJsonCompanyDetails', { params: { businessId: id, hideAjaxLoader: hideAjaxLoader } })
            .success(function (profile) {
                deferer.resolve(profile);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

    var _saveCompanyObjectDetails = function (model) {
        var deferer = $q.defer();
        //
        var bsonString = JSON.stringify(model.details);
        var businessId = model.id;
        var company = new Object();
        company.BsonString = bsonString;
        company.BusinessId = businessId;



        //
        $http.post('/Api/BusinessAccount/SaveJsonCompanyDetails', company)
            .success(function () {
                deferer.resolve();
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, deferer);
            })

        return deferer.promise;
    }

   

    return {
        RegisterBusinessAccount: _registerBusinessAccount,
        ValidateRegistrationInfo: _validateRegistrationInfo,
        GetManagedFacebookPages: _getManagedFacebookPages,
        GetConnectedFacebookPage: _getConnectedFacebookPage,
        ConnectToFacebookPage: _connectToFacebookPage,
        DisconnectToFacebookPage: _disconnectToFacebookPage,
        SearchMembersForInvitation: _searchMembersForInvitation,
        InviteMemberToBusiness: _inviteMemberToBusiness,
        AcceptInvitation: _acceptInvitation,
        DenyInvitation: _denyInvitation,
        GetMembersInBusiness: _getMembersInBusiness,
        GetAllRoles: _getAllRoles,
        UpdateMembersInBusiness: _updateMembersInBusiness,
        BAPullFeed: _BAPullFeed,
        postNewFeedTobusiness: postNewFeedTobusiness,
        GetBusinessPost: _getBusinessPost,
        ApproveBusinessPost: _approveBusinessPost,
        RejectBusinessPost: _rejectBusinessPost,
        GetPictureAlbum: _getPictureAlbum,
        UploadPictureAlbum: _uploadPictureAlbum,
        GetWorkTime: _getWorkTime,
        UpdateWorkTime: _updateWorkTime,
        EditFeedTobusiness: _EditFeedTobusiness,
        GetPublicProfile: _getPublicProfile,
        FollowBusiness: _followBusiness,
        UnfollowBusiness: _unfollowBusiness,
        GetBusinessProfile: _BAGetProfile,
        SummarizeNumberOfFollowersByTime: _summarizeNumberOfFollowersByTime,
        SummarizeNumberOfFollowersByGenders: _summarizeNumberOfFollowersByGenders,
        SummarizeNumberOfFollowersByCountries: _summarizeNumberOfFollowersByCountries,
        SummarizeNumberOfFollowersByCities: _summarizeNumberOfFollowersByCities,
        GetRolesOfCurrentUser: _getRolesOfCurrentUser,
        GetWebsite: _getWebsite,
        UpdateWebsite: _updateWebsite,
        CheckSocialNetworks: _checkSocialNetworks,
        GetCurrentUserActivityLogSettings: _getCurrentUserActivityLogSettings,
        UpdateCurrentUserActivityLogSettings: _updateCurrentUserActivityLogSettings,
        GetPublicProfileFull: _getPublicProfileFull,
        GetCompanyObjectDetails: _getCompanyObjectDetails,
        SaveCompanyObjectDetails: _saveCompanyObjectDetails,
        GetCompanyObjectDetailsById: _getCompanyObjectDetailsById,
        GetMembersFollowInBusiness: _getMembersFollowInBusiness
     

    }
}])

//UpdateCurrentUserActivityLogSettings: _updateCurrentUserActivityLogSettings