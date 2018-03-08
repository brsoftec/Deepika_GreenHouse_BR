$(document).ready(function () {
    var body = $(document.body);
    var header = $('header');
    var footer = $('footer');
    var isChrome = /Chrome/.test(navigator.userAgent) && /Google Inc/.test(navigator.vendor);
    var isSafari = /Safari/.test(navigator.userAgent) && /Apple Computer/.test(navigator.vendor);
    if (isChrome) {
        body.addClass('chrome');
    }
    var responsive;// = Modernizr.mq('(max-width: 840px)');
    var home = body.hasClass('home');
    var business = body.hasClass('business');

    function gotoIntro() {
        window.location.href = '#signup';
        // setupSlimScroll();
        if (!responsive) {
            /*            $('.sections').slimScroll({
             start: $('#intro')
             });*/
        }
    }

    $('.user-links button, .link-learnmore').click(function () {
        gotoIntro();
    });

    function updateLayout() {

        responsive = Modernizr.mq('(max-width: 840px)');
        var width = $(window).width(), height = $(window).height();
        // responsive = width<700;
        if (!responsive) {
            body.removeClass('responsive');
            // $('#home').height(height - 120);
            // $('#home').css('min-height', '800px');
        } else {
            // $('#home').css('min-height','550px');
        }
        if (responsive) {
            body.addClass('responsive');
        }
    }

    window.setTimeout(updateLayout, 200);
    $(window).resize(function () {
        updateLayout();
    });

    //  SUBSCRIPTION FORM LOGIC
    //var form = $('.form-subscribe');
    //form.each(function (index, el) {
    //    var tester = (this.id === 'form-subscribe-tester');
    //    var url = tester ?
    //        'http://regit.us13.list-manage.com/subscribe/post?u=baf77e65ec440bc6ee1779f7a&amp;id=dda4a2fcc9'
    //        : ' http://regit.us13.list-manage.com/subscribe/post?u=baf77e65ec440bc6ee1779f7a&id=30ffeed36a';

    //    $(this).ajaxChimp(
    //        {
    //            url: url,
    //            callback: onSubscribe.bind(this)
    //        }
    //    );
    //});
    //form.submit(function (e) {
    //    flash(this, 'Sending your subscription');
    //});

});
function onSubscribe(resp) {
    var form = this;
    if (resp.result === 'success') {
        flash(form, 'Please check your email to confirm your subscription.')
    } else {
        flash(form, 'Error subscribing you to the list: ' + resp.msg.substr(3));
    }
}

// Submit Message
// 'submit': 'Submitting...'

// Mailchimp Responses
// 0: 'We have sent you a confirmation email'
// 1: 'Please enter a value'
// 2: 'An email address must contain a single @'
// 3: 'The domain portion of the email address is invalid (the portion after the @: )'
// 4: 'The username portion of the email address is invalid (the portion before the @: )'
// 5: 'This email address looks fake or invalid. Please enter a real email address'

function flash(form, msg) {
    var flashbox = $(form).find('.flash');
    flashbox.show().html(msg);
}


