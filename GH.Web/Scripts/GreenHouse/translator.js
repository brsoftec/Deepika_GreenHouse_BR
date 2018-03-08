var translationApp = getApp('TranslationModule', []);

translationApp.factory('TranslationService', ['$http', '$q', function ($http, $q) {
    var _dictionaries = {};

    var _getDictionaryFromServer = function (language) {
        var deferer = $q.defer();

        $http.get('/api/Translators', { params: { language: language } }).success(function (dictionary) {
            var lowerizeDict = {};
            for (var key in dictionary) {
                lowerizeDict[key.toLowerCase()] = dictionary[key];
            }

            deferer.resolve(lowerizeDict);
        }).error(function (error, status) {
            __promiseHandler.Error(errors, status, deferer);
        });

        return deferer.promise;
    };

    var _getDictionary = function (language) {
        var deferer = $q.defer();
        if (!language)
            deferer.reject('GetDictionary require language code as input parameter');

        if (_dictionaries[language]) {
            deferer.resolve(_dictionaries[language]);
        } else {
            if (typeof Storage != 'undefined') {
                if (localStorage.Dictionaries) {
                    _dictionaries = JSON.parse(localStorage.Dictionaries);
                    if (_dictionaries[language]) {
                        deferer.resolve(_dictionaries[language]);
                    } else {
                        _getDictionaryFromServer(language).then(function (dictionary) {
                            _dictionaries[language] = dictionary;
                            localStorage.Dictionaries = JSON.stringify(_dictionaries);
                            deferer.resolve(dictionary);
                        }, function (errors) {
                            deferer.reject(errors);
                        });
                    }
                } else {
                    _getDictionaryFromServer(language).then(function (dictionary) {
                        _dictionaries[language] = dictionary;
                        localStorage.Dictionaries = JSON.stringify(_dictionaries);
                        deferer.resolve(dictionary);
                    }, function (errors) {
                        deferer.reject(errors);
                    });
                }
            } else {
                _getDictionaryFromServer(language).then(function (dictionary) {
                    _dictionaries[language] = dictionary;
                    deferer.resolve(dictionary);
                }, function (errors) {
                    deferer.reject(errors);
                });
            }
        }

        return deferer.promise;
    };

    var _clearCachedDictionaries = function () {
        if (typeof Storage != 'undefined') {
            if (localStorage.Dictionaries) {
                localStorage.removeItem('Dictionaries');
            }
        }
        _dictionaries = {};
    }

    var _getLanguages = function () {
        var deferer = $q.defer();

        $http.get('/Api/Translators/Languages').success(function (languages) {
            deferer.resolve(languages);
        }).error(function (errors, status) {
            __promiseHandler.Error(errors, status, deferer);
        })

        return deferer.promise;
    }

    return {
        Dictionaries: function () { return _dictionaries },
        GetDictionary: _getDictionary,
        ClearCachedDictionaries: _clearCachedDictionaries,
        GetLanguages: _getLanguages
    };
}]);

translationApp.run(['$cookies', '$rootScope', '$http', 'TranslationService', function ($cookies, $rootScope, $http, _translator) {
    var currentLanguage = $cookies.get('regit-language');
    if (!currentLanguage) {
        currentLanguage = 'en-US';
        var date = new Date();
        date.setDate(date.getDate() + 14);
        $cookies.put('regit-language', 'en-US', { expires: date, path: '/' });
    }

    $rootScope.translate = function (key) {
        if (_translator.Dictionaries()[currentLanguage]) {
            var content = _translator.Dictionaries()[currentLanguage][key.toLowerCase()];
            if (typeof content !== 'undefined') {
                return content
            } else {
                return _translator.Dictionaries()['en-US'][key.toLowerCase()];
            }
            return _translator.Dictionaries()[currentLanguage][key.toLowerCase()];
        } else {
            return undefined;
        }
    }

    _translator.GetDictionary(currentLanguage).then(function () {
        $http.get('/api/Translators/Version').success(function (version) {
            if ($rootScope.translate('TRANSLATION_VERSION') != version) {
                _translator.ClearCachedDictionaries();
                _translator.GetDictionary(currentLanguage);
            };
        });
    });

    if (currentLanguage != 'en-US') {
        _translator.GetDictionary('en-US');
    }

}]);

translationApp.getController('LanguageController', ['$scope', '$rootScope', '$cookies', '$http', 'TranslationService', function ($scope, $rootScope, $cookies, $http, _translator) {
    $scope.languages = [];
    $scope.language = null;

    _translator.GetLanguages().then(function (languages) {
        $scope.languages = languages;
        
        var currentLanguage = $cookies.get('regit-language');
        if (!currentLanguage) {
            currentLanguage = 'en-US';
            var date = new Date();
            date.setDate(date.getDate() + 14);
            $cookies.put('regit-language', 'en-US', { expires: date, path: '/' });
        }

        angular.forEach($scope.languages, function (language) {
            if (language.Code == currentLanguage) {
                $scope.language = language;
            }
        })

        if (moment) {
            moment.locale(currentLanguage.substring(0, 2));
        }
    })

    $rootScope.changeLanguage = function (lang) {
        if (lang != $scope.language) {
            var date = new Date();
            date.setDate(date.getDate() + 14);
            $cookies.put('regit-language', lang.Code, { expires: date, path: '/' });
            $http.post('/Api/AccountSettings/Language', '"' + lang.Code + '"').success(function () {
                window.location.reload(true);
            })
        }
    }

}]);

