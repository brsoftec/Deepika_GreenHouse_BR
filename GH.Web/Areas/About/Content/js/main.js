function showMessage(msg) {
    $('.message').html(msg).show();
}
function clearMessage(msg) {
    $('.message').html('&nbsp');//hide();
}

var qudyApi = 'http://api.qudy.com.sg/';

$(document).ready(function () {
    var contactBtn = $('#contact-submit');
    var contactForm = $('#contact-form');
    contactForm.submit(function () {
        var emailInput = $('#contact-email');
        var email = emailInput.val();
        var message = $('#contact-message').val();
        // if (!email.checkValidity(emailInput.get(0))) {
        //     showMessage('Please enter a valid email address');
        //     return false;
        // }
        if (email.length < 5) {
            showMessage('Email empty or too short');
            return false;
        }
        else if (message.length < 10) {
            showMessage('Message empty or too short');
            return false;
        }
        var post = {
            from: 'Regit Plan Inquiry',
            email: email,
            message: message,
            created: new Date()
        };
        showMessage('Sending your message...');
        var jqxhr = $.post({
            url: qudyApi + 'qudy/posts',
            data: post,
            headers: {'Authorization': 'qudy@sing'},
            success: function () {
                clearMessage();
                contactForm.hide().after('<div class="message" style="display:block">Your message was sent successfully. We\'ll get back to you shortly.</div>');
            }
        }).fail(function () {
            showMessage('Error sending message. Please try again later or email us directly.');
        });
        return false;
    });
});





