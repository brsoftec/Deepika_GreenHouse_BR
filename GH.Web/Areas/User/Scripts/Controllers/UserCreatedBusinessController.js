var myApp = getApp("myApp", true);
// var myApp = angular.module('myApp');

myApp.getController("AddBusinessController", ['$scope', '$document', '$http', 'rguView', 'rguModal', 'rguNotify', 'rguAlert','fileUpload', function ($scope, $document, $http, rguView, rguModal, rguNotify, rguAlert, fileUpload) {

    $scope.view = {
        showingAddBusinessPicker: true
    };
    $scope.rguView = rguView;

    $(".country-input").countrySelect();

    $scope.model = {};

    $scope.openAddBusinessModal = function () {
        $scope.model = {
            id: '',
            name: '',
            industry: '',
            country: '',
            city: '',
            address: '',
            phone: '',
            email: '',
            website: '',
            avatar: '',
            description: ''
        };
        $scope.uploadedFile = $scope.savedFileName = null;
        rguModal.openModal('add.business', $scope);
    };

    $scope.openAddBusinessPicker = function (evt) {
        $scope.view.openingAddBusinessPicker = true;
        $scope.openPopover('search-business-add', evt);
    };
    $scope.closeAddBusinessPicker = function () {
        $scope.view.openingAddBusinessPicker = false;
        $scope.closePopover('search-business-add');
    };

    $scope.canSend = function () {
        return $scope.model.name.length > 0;
    };

    $scope.$on("upload:selected", function (event, args) {
        $scope.$apply(function () {
            switch (args.field) {
                case "ucbAvatar":
                    var file = $scope.uploadedFile = args.file;
                    if (file && /^image\/.+$/.test(file.type)) {
                        // var fileReader = new FileReader();
                        // fileReader.readAsDataURL(file);
                        // fileReader.onload = function (e) {
                        //     $scope.$apply(function () {
                        //         $scope.model.avatar = e.target.result;
                        //     })
                        // };
                        // $scope.model.avatarFile = file;
                        var uploadUrl = "/api/Ucb/Avatar/" + new Date().getTime();
                        fileUpload.uploadFileToUrl(file, uploadUrl, function (response) {
                            $scope.savedFileName = response.fileName;
                        }, function () {

                        });
                    } else {
                        rguNotify.add('Please upload an image file.');
                        // $scope.model.avatarFile = null;
                        $scope.uploadedFile = null;
                        $scope.savedFileName = null;
                    }
                    break;
                default:
                    break;
            }
        });
    });

    $scope.closeAddBusinessModal = function (hideFunc) {

        if (angular.isFunction(hideFunc)) {
            hideFunc();
        }
    };


    $scope.selectActor = function (actor) {
        if (actor === 'business') {
            location.href = '/BusinessAccount/Signup';
        } else {
            $scope.openAddBusinessModal();
        }
        $scope.closeAddBusinessPicker();
    };

    $scope.sendAddBusiness = function (hideFunc) {
        $scope.closeAddBusinessModal(hideFunc);
        if ($scope.model.country.length) {
            $scope.model.country = $scope.model.country.replace(/\(.+\)/g, '');
        }
        if ($scope.savedFileName) {
            $scope.model.avatar = $scope.savedFileName;
        }

        $http.post('/Api/Ucb/New', $scope.model)
            .success(function (response) {
                rguAlert('The profile has been sent to Regit admin team for review before publishing.');
            }).error(function (errors) {
            console.log('Error adding business: ', errors)
        });

    };
}])

    .getController("UcbProfileController", ['$scope', '$document', '$http', 'rguView', 'rguModal', 'rguNotify', function ($scope, $document, $http, rguView, rguModal, rguNotify) {
        var ucb = regitGlobal.ucb;
        $scope.viewedProfile = {
            displayName: ucb.name,
            industry: ucb.industry,
            avatar: ucb.avatar,
            location: {
                street: ucb.address,
                country: ucb.country,
                city: ucb.city
            },
            publicPhone: ucb.phone,
            publicEmail: ucb.email,
            website: ucb.website,
            description: ucb.description
        };

        $scope.claimBusiness = function () {
            $scope.claim = {
                ucb: ucb,
                form: {
                    name: '',
                    phone: '',
                    email: '',
                    message: ''
                }
            };
            rguModal.openModal('claim.business', $scope);
        };

        $scope.closeClaimBusinessModal = function (hideFunc) {

            if (angular.isFunction(hideFunc)) {
                hideFunc();
            }
        };


        $scope.canClaim = function () {
            return $scope.claim.form.name.length && ($scope.claim.form.phone.length || $scope.claim.form.email.length);
        };

        $scope.sendClaim = function (hideFunc) {
            $scope.closeClaimBusinessModal(hideFunc);
            var claim = $scope.claim;
            var model = {
                ucbId: claim.ucb.id,
                ucbName: claim.ucb.name,
                name: claim.form.name,
                phone: claim.form.phone,
                email: claim.form.email,
                message: claim.form.message
            };
            // console.log(model);

            $http.post('/Api/Ucb/Claim', model)
                .success(function (response) {
                    rguNotify.add('Your request to claim business ' + claim.ucb.name + ' has been submitted to the Regit team for review.'
                        + ' Additional information may be required to verify ownership. We appreciate your patience during this time.');
                }).error(function (errors) {
                console.log('Error claiming business: ', errors)
            });

        };

    }]);

