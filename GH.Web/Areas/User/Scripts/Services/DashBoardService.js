var socialModule = getApp("SocialModule", []);

socialModule.factory('DashboardService', [
    '$http', '$q', function ($http, $q) {

        var pullDashboardFeed = function (url,start, take,id) {

            var defer = $q.defer();
            $http.get(url + "?start=" + start + "&take=" + take + "&BAId=" + id).success(function (response) {
                defer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, defer);
            });

            return defer.promise;
        }

        var pullMoreDashboardFeed = function (url, start, take) {

            var defer = $q.defer();
            $http.get(url + "?start=" + start + "&take=" + take).success(
                function (response) {
                    defer.resolve(response);
                }).error(function (errors, status) {
                    __promiseHandler.Error(errors, status, defer);
                });

            return defer.promise;
        }

        var getCommentByPost = function (socialPostId) {
            var defer = $q.defer();
            $http.get('/API/Social/socialpost/comments?socialPostId=' + socialPostId).success(
                function (response) {
                    defer.resolve(response);
                }).error(function (errors, status) {
                    __promiseHandler.Error(errors, status, defer);
                });

            return defer.promise;
        }

        var likePost = function (url, id) {
            var defer = $q.defer();
            $http.post(url + '?id=' + id).success(function (response) {
                defer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, defer);
            });
            return defer.promise;
        }

        var addComment = function (url, id, message) {
            var defer = $q.defer();

            $http.post(url, { Id: id, Message: message }).success(
                function (response) {
                    defer.resolve(response);
                }).error(function (errors, status) {
                    __promiseHandler.Error(errors, status, defer);
                });

            return defer.promise;
        }

        var sharePost = function (url, socialPostId, typeOfShares, message) {
            var defer = $q.defer();
            var isShareFacebook = isShareTwitter = isShareGreenHouse = false;
            if (typeOfShares.indexOf('facebook') != -1)
                isShareFacebook = true;
            if (typeOfShares.indexOf('twitter') != -1)
                isShareTwitter = true;
            if (typeOfShares.indexOf('greenhouse') != -1)
                isShareGreenHouse = true;
            if (socialPostId) {
                $http.post(url, { SocialPostId: socialPostId, IsShareFacebook: isShareFacebook, IsShareTwitter: isShareTwitter, IsShareGreenHouse: isShareGreenHouse, Message: message }).success(function (response) {
                    defer.resolve(response);
                }).error(function (errors, status) {
                    __promiseHandler.Error(errors, status, defer);
                });
            }

            return defer.promise;
        }

        var deletePost = function (socialPostId, isPersonal) {
            var defer = $q.defer();
            $http.delete('API/Social/Delete', {
                params: {
                    id: socialPostId,
                    isPersonal: isPersonal
                }
            }).success(function (response) {
                defer.resolve(response);
            }).error(function (errors, status) {
                __promiseHandler.Error(errors, status, defer);
            });
            return defer.promise;
        }

        var shareAFacebookPostToTwitter = function (url, socialPostId, postMongoId) {
            var defer = $q.defer();

            if (socialPostId) {
                $http.post(url, { SocialPostId: socialPostId, IsShareToTwitter: true }).success(function (response) {
                    defer.resolve(response);
                }).error(function (errors, status) {
                    __promiseHandler.Error(errors, status, defer);
                });
            }

            return defer.promise;
        }

        var postNewFeedToSocial = function (url, newPost) {
            var defer = $q.defer();

            var formData = new FormData();
            //append properties
            formData.append("Message", newPost.Message);
            formData.append("IsPostGreenHouse", newPost.IsPostGreenHouse);
            formData.append("IsPostFacebook", newPost.IsPostFacebook);
            formData.append("IsPostTwitter", newPost.IsPostTwitter);
            formData.append("NumberOfPhotos", newPost.Photos.length);
            //formData.append("IsPublic", newPost.Privacy.Public);
            formData.append("IsFriends", newPost.Privacy.Friends);
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

        var _searchPersonalPosts = function (criteria) {
            var deferer = $q.defer();

            var baseCriteria = {
                Keyword: null,
                Regit: false,
                Facebook: false,
                Twitter: false,
                From: null,
                To: null,
                Start: 0,
                Length: 10,
                SearchBaseTime: null
            }

            __common.MergeObject(baseCriteria, criteria);

            $http.get('/Api/Social/SearchPostsForPersonalScope', { params: baseCriteria })
                .success(function (response) {
                    deferer.resolve(response);
                }).error(function (errors, status) {
                    __promiseHandler.Error(errors, status, deferer);
                })

            return deferer.promise;
        }

        var _searchGlobalPosts = function (criteria) {
            var deferer = $q.defer();

            var baseCriteria = {
                Keyword: null,
                Regit: false,
                Facebook: false,
                Twitter: false,
                From: null,
                To: null,
                Start: 0,
                Length: 10,
                SearchBaseTime: null
            }

            __common.MergeObject(baseCriteria, criteria);

            $http.get('/Api/Social/SearchPostsForGlobalScope', { params: baseCriteria })
                .success(function (response) {
                    deferer.resolve(response);
                }).error(function (errors, status) {
                    __promiseHandler.Error(errors, status, deferer);
                })

            return deferer.promise;
        }

        var _searchPersonalUsers = function (criteria) {
            var deferer = $q.defer();

            var baseCriteria = {
                Keyword: null,
                Start: 0,
                Length: 10,
                SearchBaseTime: null
            }

            __common.MergeObject(baseCriteria, criteria);

            $http.get('/Api/Social/SearchUsersForPersonalScope', { params: baseCriteria })
                .success(function (response) {
                    deferer.resolve(response);
                }).error(function (errors, status) {
                    __promiseHandler.Error(errors, status, deferer);
                })

            return deferer.promise;
        }

        var _searchGlobalUsers = function (criteria) {
            var deferer = $q.defer();

            var baseCriteria = {
                Keyword: null,
                Start: 0,
                Length: 10,
                SearchBaseTime: null
            }

            __common.MergeObject(baseCriteria, criteria);

            $http.get('/Api/Social/SearchUsersForGlobalScope', { params: baseCriteria })
                .success(function (response) {
                    deferer.resolve(response);
                }).error(function (errors, status) {
                    __promiseHandler.Error(errors, status, deferer);
                })

            return deferer.promise;
        }

        return {
            PullDashboardFeed: pullDashboardFeed,
            PullMoreDashboardFeed: pullMoreDashboardFeed,
            GetCommentByPost: getCommentByPost,
            LikePost: likePost,
            AddComment: addComment,
            SharePost: sharePost,
            ShareAFacebookPostToTwitter: shareAFacebookPostToTwitter,
            PostNewFeedToSocial: postNewFeedToSocial,
            DeletePost: deletePost,
            SearchPersonalPosts: _searchPersonalPosts,
            SearchGlobalPosts: _searchGlobalPosts,
            SearchPersonalUsers: _searchPersonalUsers,
            SearchGlobalUsers: _searchGlobalUsers
        };
    }
]);