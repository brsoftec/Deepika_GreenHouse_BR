﻿var myApp = getApp("myApp", true);

myApp.getController('BillingController', function ($scope, $timeout, rguModal, rguView, $http) {
    $scope.billingConfig = {

        quotaLabels: {
            communication: 'Interactions',
            syncRelationships: 'Handshake Relationships',
            businessUsers: 'Business Users'
        },
        plans: []
    };
    $scope.paymentConfig = {
        cardBrands: [
            {
                name: 'visa',
                label: 'VISA'
            },
            {
                name: 'mastercard',
                label: 'MasterCard'
            },
            {
                name: 'amex',
                label: 'American Express'
            },
            {
                name: 'discover',
                label: 'Discover'
            },
            {
                name: 'jcb',
                label: 'JCB'
            },
            {
                name: 'diners',
                label: 'Diners\' Club'
            }
        ]
    };
    $scope.Init = function () {
        $http.post('/Api/BusinessAccount/GetSubcriptionByUser', null)
            .success(function (response) {
                console.log(response);
                $scope.billingConfig.plans = response.SubcriptionPlans.Data;

                $scope.billing = {
                    accountNumber: response.AccountId,
                    accountOwnerName: response.AccountName,
                    address: response.AccountName.AccountAddress,
                    promoCode: "",
                    subscription: response.Subcription
                };
                $($scope.billingConfig.plans).each(function (index, object) {
                    if (object.name === response.Subcription.subcription.CurrentPlan)
                        $scope.billing.plan = $scope.billingConfig.plans[index];
                });

                $scope.isAdmin = response.isAdmin;

                $scope.payment = {
                    methods: [],
                    method: null
                };
                $(response.ListPaymentCard).each(function (index, object) {
                    $scope.payment.methods.push({
                        id: object.Id,
                        cardType: object.cardtype,
                        fullName: object.cardname,
                        cardNumber: object.cardnumber,
                        expires: {
                            month: object.expiredmonth,
                            year: object.expiredyear
                        },
                        securityCode: object.cardsecuritycode,
                        isDefault: object.isdefault
                    });

                });
                $scope.billHistory = {
                    bills: []
                };
                $(response.ListBilling).each(function (index, object) {
                    if (!$scope.billing.currentBilling && object.isCurrent) {
                        $scope.billing.currentBilling = object;
                        console.log(object);
                        $scope.billing.nextCharge = new Date(object.datestart) > new Date() ? object.datestart : object.dateend;
                    }
                    $scope.billHistory.bills.push({
                        startdate: object.datestart,
                        enddate: object.dateend,
                        productname: object.productname,
                        status: object.status,
                        amount: object.amount,
                        transactionId: object.transactionid,
                        methodpayment: object.methodpayment
                    });

                });

                $scope.renderPlanInfo = function (plan) {

                    if (!plan) return '';
                    var html = '<div class="billing-plan">';
                    html += '<div class="billing-plan-heading">' + plan.label + '</div>';
                    if (plan.name === 'enterprise')
                        html += '<div class="billing-plan-price">Pay per monthly use</div>';
                    else
                        html += '<div class="billing-plan-price">US$<span class="billing-plan-price-value">' + (plan.price || '0') + '</span> monthly</div>';
                    html += '<div class="billing-plan-quota">';
                    html += '<ul>';
                    angular.forEach(plan.quota, function (item, name) {
                        if (!$scope.billingConfig.quotaLabels.hasOwnProperty(name))
                            return;
                        var label = $scope.billingConfig.quotaLabels[name];
                        var value = plan.quota[name];
                        if (value === 'unlimited') {
                            value = 'Unlimited';
                        }
                        html += '<li><span class="billing-plan-quota-value">' + value + '</span> ' + label + '</li>';
                    });
                    html += '</div></div>';
                    return html;
                };
                $scope.planSelect = {
                    selectedPlan: null //$scope.billing.plan
                };

            })
            .error(function (errors, status) {
            });
    };

    $scope.upgradePlan = function () {
        rguModal.openModal('billing.plan.select', $scope);
    };
    $scope.selectPlan = function (plan) {
        if ($scope.billing.currentBilling && $scope.billing.currentBilling.isPending) return;
        if (plan.name !== $scope.billing.plan.name)
            $scope.planSelect.selectedPlan = plan;
    };
    $scope.canChangePlan = function (plan) {
        return $scope.planSelect.selectedPlan && ($scope.planSelect.selectedPlan.name === 'free' || $scope.planSelect.selectedPlan.name === 'enterprise' || $scope.payment.methods.length || $scope.billing.promoCode.length);
    };
    $scope.savePlan = function (close) {
        var workflow = parseInt($scope.planSelect.selectedPlan.quota.businessUsers, 10);
        var syncRe = parseInt($scope.planSelect.selectedPlan.quota.syncRelationships, 10);
        if ($scope.syncMembers > syncRe || $scope.workflowMembers > workflow) {
            swal(
                'UpgradePlan',
                "You use over quota of this plan (" + $scope.planSelect.selectedPlan.name + ")",
                'error'
            );
            return;
        }
        //if ($scope.planSelect.selectedPlan.name == "free" && $scope.interactions > 0) {
        //    swal(
        //         'UpgradePlan',
        //           "You use over quota of this plan (" + $scope.planSelect.selectedPlan.name + ")",
        //         'error'
        //     );
        //    return;
        //}

        var data = {
            PaymentPlanName: $scope.planSelect.selectedPlan.name
        };

        if ($scope.billing.promoCode.length) {
            data.PromoCode = $scope.billing.promoCode;

            $http.post('/api/BusinessAccount/UpgradePlan', data)
                .success(function (response) {
                    if (!response.ReturnStatus) {
                        swal(
                            'UpgradePlan',
                            response.ReturnMessage[0],
                            'error'
                        );
                        // $scope.planSelect.selectedPlan = $scope.billing.plan;
                    }
                    else {
                        $scope.Init();
                        close();
                    }
                })
                .error(function (errors, status) {

                });
        }
        else {
            if ($scope.planSelect.selectedPlan.name !== 'enterprise') {
                if (!$scope.payment.methods.length) {
                    swal(
                        'Upgrade Plan',
                        "Please add a payment method",
                        'error'
                    );
                    // $scope.planSelect.selectedPlan = $scope.billing.plan;
                    return true;
                }
                var defaultMethod = $scope.payment.methods.find(function (m) {
                    return m.isdefault;
                });
                if (!defaultMethod) defaultMethod = $scope.payment.methods[0];
            }
            $http.post('/api/BusinessAccount/UpgradePlan', data)
                .success(function (response) {
                    if (!response.ReturnStatus) {
                        swal(
                            'UpgradePlan',
                            response.ReturnMessage[0],
                            'error'
                        );
                        // $scope.planSelect.selectedPlan = $scope.billing.plan;
                    }
                    else {
                        $scope.Init();
                        close();
                    }
                })
                .error(function (errors, status) {

                });
        }


    };

    $scope.workflowMembers = 0;
    $scope.syncMembers = 0;

    $scope.getInfoQuota = function () {
        $http.post('/api/BusinessAccount/GetPaymentDetailByUserId', null)
            .success(function (response) {
                $scope.interactions = response.PaymentPlanDetailInteraction.currentnumber;
                $scope.syncMembers = response.PaymentPlanDetailSyncForm.currentnumber;
                $scope.workflowMembers = response.PaymentPlanDetailWorkFlow.currentnumber;
            });
    };
    setTimeout(function () {
        $scope.getInfoQuota();
    }, 1000);

    $scope.view = {
        promoCode: {
            editing: false,
            message: '',
            model: ''
        }
    };
    $scope.openEditPromoCode = function () {
        $scope.view.promoCode.editing = true;
        $scope.view.promoCode.model = $scope.billing.currentBilling.promoCode;
    };
    $scope.cancelEditPromoCode = function () {
        $scope.view.promoCode.editing = false;
        $scope.view.promoCode.message = '';
        $scope.view.promoCode.model = '';
    };
    $scope.savePromoCode = function () {
        var promoCode = $scope.view.promoCode.model;
        if (!promoCode.length) return;
        if (promoCode === $scope.billing.currentBilling.promocode) {
            $scope.view.promoCode.message = 'Promo code already being used';
            return;
        }
        $http.post('/Api/Billing/Subscription/PromoCode/Add?promoCode=' + promoCode)
            .success(function (response) {
                if (response.success) {
                    $scope.billing.subscription.PendingPromoCode = $scope.view.promoCode.model;
                    $scope.view.promoCode.editing = false;
                    $scope.view.promoCode.message = '';
                    $scope.view.promoCode.model = '';
                } else {
                    console.log('Error updating payment method', response);
                    $scope.view.promoCode.message = response.message;
                }
            }).error(function (response) {
            console.log('Error updating payment method', response);
        });
    };


    //  PAYMENT METHODS

    $scope.testMethod = ({
        id: 0,
        index: 0,
        cardType: 'visa',
        fullName: 'Quay T Do',
        cardNumber: '4242424242424242',
        expires: {
            month: '12',
            year: '2020'
        },
        securityCode: '123',
        isDefault: true
    });

    // $scope.payment = {
    //     methods: [],
    //     method: $scope.testMethod,
    //     defaultMethod: $scope.testMethod
    // };
    // $scope.payment.methods.push($scope.testMethod);

    $scope.getCardTypeIconUrl = function (brand) {
        if (brand !== 'visa' && brand !== 'mastercard' && brand !== 'amex' && brand !== 'discover' && brand !== 'jcb' && brand !== 'diners') {
            brand = 'unknown';
        }
        return '/Areas/Beta/img/card-' + brand + '.svg';
    };
    $scope.addPaymentMethod = function () {
        $scope.payment.method = {
            creating: true,
            id: Date.now(),
            cardType: 'visa',
            fullName: '',
            cardNumber: '',
            expires: {
                month: '',
                year: ''
            },
            securityCode: '',
            isDefault: true
        };

        $scope.view = {
            cardError: {
                type: '',
                code: '',
                message: ''
            }
        };

        $scope.clearCardError = function (error) {
            $scope.view.cardError.type = '';
            $scope.view.cardError.code = '';
            $scope.view.cardError.message = '';
        };
        $scope.showCardError = function (error) {
            $scope.view.cardError.type = error.type;
            $scope.view.cardError.code = error.code;
            $scope.view.cardError.message = error.message;
        };

        rguModal.openModal('billing.payment.card', $scope);

    };
    $scope.editPaymentMethod = function (method, index) {
        $scope.view = {
            cardError: {
                type: '',
                code: '',
                message: ''
            }
        };

        $scope.clearCardError = function (error) {
            $scope.view.cardError.type = '';
            $scope.view.cardError.code = '';
            $scope.view.cardError.message = '';
        };
        $scope.showCardError = function (error) {
            $scope.view.cardError.type = error.type;
            $scope.view.cardError.code = error.code;
            $scope.view.cardError.message = error.message;
        };

        $scope.payment.method = angular.copy(method);
        $scope.payment.method.index = index;
        rguModal.openModal('billing.payment.card', $scope);
    };
    $scope.removePaymentMethod = function (method, index) {
        swal({
            title: "Are you sure to remove this card?",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes",
            cancelButtonText: "No"

        }).then(function () {
            var data = new Object();
            data.PaymentCardId = method.id;

            $http.post('/api/BusinessAccount/DeletePaymentCard', data)
                .success(function (response) {
                    if (!response.ReturnStatus) {
                        swal(
                            'Credit Card',
                            response.ReturnMessage[0],
                            'error'
                        );
                    }
                    else {
                        $scope.Init();
                        // close();
                    }

                    //data.CardName
                })
                .error(function (errors, status) {

                });
        });


        //  $scope.payment.methods.splice(index, 1);
    };
    $scope.canSetDefaultMethod = function (method) {
        return !method.isDefault;
    };
    $scope.setDefaultPaymentMethod = function (method) {
        $http.post('/Api/Billing/PaymentMethod/SetDefault/' + method.id)
            .success(function (response) {
                if (response.success) {
                    angular.forEach($scope.payment.methods, function (method) {
                        method.isDefault = false;
                    });
                    method.isDefault = true;
                    $scope.payment.defaultMethod = method;
                } else {
                    console.log('Error updating payment method', response);
                }
            }).error(function (response) {
            console.log('Error updating payment method', response);
        });
    };

    //  Payment method modal
    $scope.selectCardType = function (type) {
        $scope.payment.method.cardType = type.name;
    };
    $scope.canSaveMethod = function () {
        if (!$scope.payment.method) return false;
        if (!$scope.payment.method.fullName) return false;
        if (!/^\d{16,}$/.test($scope.payment.method.cardNumber)) return false;
        if (!/^\d{1,2}$/.test($scope.payment.method.expires.month)) return false;
        if (!/^\d{4}$/.test($scope.payment.method.expires.year)) return false;
        if ($scope.payment.method.securityCode && !/^\d{3,4}$/.test($scope.payment.method.securityCode)) return false;

        $scope.payment.method.expires.month = parseInt($scope.payment.method.expires.month);
        if ($scope.payment.method.expires.month < 1 || $scope.payment.method.expires.month > 13) return false;
        $scope.payment.method.expires.year = parseInt($scope.payment.method.expires.year);
        if ($scope.payment.method.expires.year < 2017 || $scope.payment.method.expires.year > 3000) return false;
        return true;
    };
    $scope.saveMethod = function (close) {
        if ($scope.canSaveMethod()) {
            if ($scope.payment.method.isDefault) {
                $scope.setDefaultPaymentMethod($scope.payment.method);
            }
            //save creadit card

            var data = new Object();
            data.CardName = $scope.payment.method.fullName;
            data.CardType = $scope.payment.method.cardType;
            data.CardNumber = $scope.payment.method.cardNumber;
            if ($scope.payment.method != undefined && $scope.payment.method != null)
                data.PaymentCardId = $scope.payment.method.id;
            data.CardExpiredMonth =
                $scope.payment.method.expires.month;
            data.CardExpiredYear =
                $scope.payment.method.expires.year;
            data.CardSecurity =
                $scope.payment.method.securityCode;
            data.IsDefault = $scope.payment.method.isDefault;
            $http.post('/api/BusinessAccount/SavePaymentCard', data)
                .success(function (response) {
                    if (!response.ReturnStatus) {
                        swal(
                            'Credit Card',
                            response.ReturnMessage[0],
                            'error'
                        );
                    }
                    else {
                        $scope.Init();
                        close();
                    }

                    //data.CardName
                })
                .error(function (errors, status) {

                });
            close();


        }
    };

    //  BILLING HISTORY

    $scope.renderPaymentMethod = function (methodId) {
        var method = null;
        angular.forEach($scope.payment.methods, function (m) {
            if (m.id === methodId) {
                method = m;
            }
        });
        if (!method) return '';
        var html = '<div class="payment-method-badge">';
        html += '<div class="payment-method-icon"><img src="' + $scope.getCardTypeIconUrl(method.cardType) + '"></div>';
        html += '<div class="payment-method-number">****' + method.cardNumber.slice(14) + '</div>';
        html += '</div';
        return html;
    };

    $scope.Init();

    $scope.promo = {
        plans: ['medium', 'heavy'],
        plan: 'medium',
        durations: ['1 month', '3 months', '6 months'],
        duration: '1 month',
        usages: ["1", "10", "100", 'unlimited'],
        usage: 1,
        code: ''
    };
    $scope.generateCode = function () {
        var data = {};
        data.PromoCode = Date.now().toString();
        data.PromoCodeType = $scope.promo.plan;
        switch ($scope.promo.duration) {
            case "1 months":
                data.NumberMonthExpired = "1";
                break;
            case "6 months":
                data.NumberMonthExpired = "6";
                break;
            case "3 months":
                data.NumberMonthExpired = "3";
                break;
            default:
                data.NumberMonthExpired = "1";
        }
        switch ($scope.promo.usage) {
            case "1":
                data.NumberReUse = "1";
                break;
            case "10":
                data.NumberReUse = "10";
                break;
            case "100":
                data.NumberReUse = "100";
                break;
            default:
                data.NumberReUse = "";
        }

        $http.post('/api/BusinessAccount/InsertPromoCode', data)
            .success(function (response) {
                if (!response.ReturnStatus) {
                    swal(
                        'generate promo',
                        response.ReturnMessage[0],
                        'error'
                    );
                }
                else {
                    $scope.promo.code = data.PromoCode;
                }

                //data.CardName
            })
            .error(function (errors, status) {

            });

    };
    $scope.generatePromoCode = function () {
        $scope.promo.plan = 'medium';
        $scope.promo.code = '';
        rguModal.openModal('billing.promocode.generate', $scope);
    };

    //  STRIPE
    $scope.openPayForm = function () {
        if (!$scope.payment.methods.length || !$scope.payment.defaultMethod) return;
        $scope.payment.method = $scope.payment.defaultMethod;
        // $scope.payment.method.index = 0;

        $scope.view = {
            cardError: {
                type: '',
                code: '',
                message: ''
            }
        };

        $scope.clearCardError = function (error) {
            $scope.view.cardError.type = '';
            $scope.view.cardError.code = '';
            $scope.view.cardError.message = '';
        };
        $scope.showCardError = function (error) {
            $scope.view.cardError.type = error.type;
            $scope.view.cardError.code = error.code;
            $scope.view.cardError.message = error.message;
        };

        rguModal.openModal('billing.payment.pay', $scope);

        var stripe = Stripe('pk_test_QnUjj2P8ItaziTn3xwgvM9DE');
        var elements = stripe.elements();

        $timeout(function () {

            var baseStyle = {
                /*iconColor: '#666EE8',
                 color: '#31325F',
                 lineHeight: '40px',
                 fontWeight: 300,
                 fontFamily: '"Roboto", Helvetica, sans-serif',
                 fontSize: '15px',
                 '::placeholder': {
                 color: '#CFD7E0'
                 }*/
            };
            $scope.cardNumber = elements.create('cardNumber', {});
            $scope.cardNumber.mount('#card-number');

            $scope.cardExpiry = elements.create('cardExpiry', {});
            $scope.cardExpiry.mount('#card-expiry');

            $scope.cardCvc = elements.create('cardCvc', {});
            $scope.cardCvc.mount('#card-cvc');

            function setOutcome(result, element) {
                $scope.$apply(function () {
                    $scope.clearCardError();

                    if (result.error) {

                        $scope.showCardError(result.error);

                    } else if (element === 'cardNumber') {
                        $scope.payment.method.brand = result.brand;
                    }
                    if (result.token) {
                        // Use the token to create a charge or a customer
                        // https://stripe.com/docs/charges
                        console.log(result.token);
                        $scope.payment.tokenId = (result.token.id);
                    }
                });
            }

            $scope.cardNumber.on('change', function (event) {

                setOutcome(event, 'cardNumber');
            });
            $scope.cardExpiry.on('change', function (event) {
                setOutcome(event, 'cardExpiry');
            });
            $scope.cardCvc.on('change', function (event) {
                setOutcome(event, 'cardCvc');
            });

            $scope.submitCard = function () {
                var extraDetails = {
                    name: $scope.payment.method.fullName
                };
                stripe.createToken($scope.cardNumber, extraDetails).then(setOutcome);
            };
        }, 1000);

    };

    // $scope.openPayForm();

})

    .directive('promocodeEditInput', function ($timeout) {
            return {
                restrict: 'C',
                link: function (scope, el, attrs) {
                    scope.$watch('view.promoCode.editing', function (value) {
                        if (value === true) {
                            $timeout(function () {
                                el[0].focus();
                            });
                        }
                    });
                }
            };
        }
    )

    .controller('PaymentCardController', function ($scope, $timeout) {
        var stripe = Stripe('pk_test_QnUjj2P8ItaziTn3xwgvM9DE');
        var elements = stripe.elements();

        $timeout(function () {

            var baseStyle = {
                /*iconColor: '#666EE8',
                 color: '#31325F',
                 lineHeight: '40px',
                 fontWeight: 300,
                 fontFamily: '"Roboto", Helvetica, sans-serif',
                 fontSize: '15px',
                 '::placeholder': {
                 color: '#CFD7E0'
                 }*/
            };
            $scope.cardNumber = elements.create('cardNumber', {});
            $scope.cardNumber.mount('#card-number');

            $scope.cardExpiry = elements.create('cardExpiry', {});
            $scope.cardExpiry.mount('#card-expiry');

            $scope.cardCvc = elements.create('cardCvc', {});
            $scope.cardCvc.mount('#card-cvc');

            function setOutcome(result, element) {
                $scope.$apply(function () {
                    $scope.clearCardError();

                    if (result.error) {

                        $scope.showCardError(result.error);

                    } else if (element === 'cardNumber') {
                        $scope.payment.method.brand = result.brand;
                    }
                    if (result.token) {
                        // Use the token to create a charge or a customer
                        // https://stripe.com/docs/charges
                        console.log(result.token);
                        $scope.payment.tokenId = (result.token.id);
                    }
                });
            }

            $scope.cardNumber.on('change', function (event) {

                setOutcome(event, 'cardNumber');
            });
            $scope.cardExpiry.on('change', function (event) {
                setOutcome(event, 'cardExpiry');
            });
            $scope.cardCvc.on('change', function (event) {
                setOutcome(event, 'cardCvc');
            });

            $scope.submitCard = function () {
                var extraDetails = {
                    name: $scope.payment.method.fullName
                };
                stripe.createToken($scope.cardNumber, extraDetails).then(setOutcome);
            };
        }, 1000);


    });
