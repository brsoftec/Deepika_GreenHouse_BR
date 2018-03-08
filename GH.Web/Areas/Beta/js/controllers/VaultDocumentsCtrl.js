angular.module('regitApp').controller('VaultDocumentsCtrl', function ($scope, $location, $http, rguModal, docsService) {

    $scope.goBack = function() {
        $location.path('/Vault/Categories');
    };

    $scope.vaultDocSearch = {
        query: '',
        pageStart: 0,
        pageSize: 7,
        pages: [0]
    };
    $scope.clearDocSearch = function () {
        $scope.vaultDocSearch.query = '';
    };

    $scope.docs = docsService.getDocs();

    //  VAULT TREE
    $http.get('js/vault-search-new.json').then(function (response) {
        $scope.vaultInit(response.data);
    });

    $scope.vaultInit = function(vault) {
        $scope.vaultTree = vault;
        var entries = [];
        function traverseVault(node, level, path, jsPath) {
            if (!angular.isObject(node) || node.nosearch) return;
            angular.forEach(node, function (entry, name) {
                if (!angular.isObject(entry) || entry.nosearch)
                    return;
                var label = entry.label;
                var list = angular.isUndefined(label);
                if (!angular.isObject(entry.value) && !list) {

                } else {
                    if (list) {
                        label = entry.description;

                    }
                    var vaultEntry = {
                        id: entry._id,
                        label: label,
                        leaf: false,
                        level: level,
                        path: path,
                        jsPath: jsPath + '.' + name

                    };
                    if (list) {
                        vaultEntry.list = true;
                    }
                    if (list) {
                        entries.push(vaultEntry);
                    }


                    if (!list && entry.hasDoc) {
                    traverseVault(entry.value, level + 1, path + '/' + entry.label, jsPath + '.' + name);
                    }
                }
            });
        }
        traverseVault($scope.vaultTree, 1, '', '');
        $scope.vaultEntries = entries;
    };


    $scope.uploadDoc = function() {
        var doc = {
            name: '',
            cats: '',
            type: 'doc',
            uploaded: new Date(),
            url: ''
        };
        $scope.openDocEditor(doc, true)
    };

    $scope.openDocEditor = function(doc, newDoc) {
        $scope.doc = doc;
        $scope.newDoc = newDoc;
        rguModal.openModal('vault.doc.editor', $scope);
    };

    $scope.fileName = '';
    $scope.$watch('fileName', function(nv,ov) {
    });

    $scope.deleteDoc = function(doc) {
        var index = $.inArray(doc, $scope.docs);
        if (index>=0) {
            $scope.docs.splice(index, 1);
        }
    };

    $scope.deleteDocByIndex = function(index) {
        $scope.docs.splice(index,1);
    };

    $scope.filterDocsByQuery = function (doc) {
        var re = new RegExp($scope.vaultDocSearch.query, 'i');
        return re.test(doc.name) || re.test(doc.cat);
    };
    $scope.initPages = function () {
        var pageCount = Math.ceil($scope.matchedDocs.length  / $scope.vaultDocSearch.pageSize);
        $scope.vaultDocSearch.pages = [];
        if ($scope.vaultDocSearch.pageStart >= $scope.matchedDocs.length) {
            $scope.vaultDocSearch.pageStart = $scope.matchedDocs.length-1;
        }
        for (i=1; i<=pageCount; i++) {
            $scope.vaultDocSearch.pages.push(i);
        }
    };

    $scope.gotoPage = function(page) {
        $scope.vaultDocSearch.pageStart = page * $scope.vaultDocSearch.pageSize;
    };
    $scope.onVaultDocSearchInput = function() {
        // $scope.initPages();
    };

    $scope.onLinkSelect = function(item,model) {
        console.log(item.jsPath);
    }
});



