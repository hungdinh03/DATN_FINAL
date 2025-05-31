function applyJS() {
    $('#registerForm input').on('input', function () {
        $('.success-line').eq(0).hide();
        $('.validation-summary').eq(0).hide();
    });

    $('#loginForm input').on('input', function () {
        $('.success-line').eq(1).hide();
        $('.validation-summary').eq(1).hide();
    });

    $('#forgotPasswordForm input').on('input', function () {
        $('.success-line').eq(1).hide();
        $('.validation-summary').eq(1).hide();
    });
}

var loginHtml, forgotPasswordHtml;
$(document).ready(function () {
    applyJS();

    // Register
    $(document).on('submit', '#registerForm', function (e) {
        e.preventDefault();
        if (!$('.validation-summary').eq(0).is(':visible')) {
            $('.success-line').eq(0).text("Bạn hãy chờ chút ...").show();
        }
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    $('.success-line').eq(0).text("Quý khách vui lòng kiểm tra link xác thực được gửi tới email đã đăng ký!").show();
                } else if (response.error) {
                    setTimeout(function () {
                        $('.success-line').eq(0).hide();
                        $('.validation-summary').eq(0).show();
                        $('.validation-error').eq(0).text(response.error);
                    }, 100) 
                }
            },
            error: function (xhr) {
                if (xhr.status === 500) {
                    alert('Bạn đã đăng nhập ở tab khác. Hãy thử tải lại trang.');
                } else {
                    alert('Error: ' + xhr.status);
                }
            }
        })
    });    

    // Login
    $(document).on('submit', '#loginForm', function (e) {
        e.preventDefault();
        if (!$('.validation-summary').eq(1).is(':visible')) {
            $('.success-line').eq(1).show();
        }
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    window.location.href = (document.referrer.includes('/Account/ConfirmEmail') || !document.referrer) ? '/' : document.referrer;
                } else if (response.errors) {
                    setTimeout(function () {
                        setTimeout(function () {
                            $('.validation-summary').eq(1).show();
                            $('.validation-error').eq(1).empty();
                            $.each(response.errors, function (index, error) {
                                $('.validation-error').eq(1).append('<li>' + error + '</li>');
                            });
                        }, 100)
                    }, 100)
                }
            },
            error: function (xhr) {
                if (xhr.status === 500) {
                    alert('Bạn đã đăng nhập ở tab khác. Hãy thử tải lại trang.');
                } else {
                    alert('Error: ' + xhr.status);
                }
            }
        })
    });    

    // Forgot Password
    $(document).on('click', '.forgotPasswordLink', function (e) {
        e.preventDefault();
        $.ajax({
            url: '/Account/ForgotPassword',
            type: 'GET',
            success: function (view) {
                $('.login-container .field-validation-error').text('');
                $('.validation-summary').hide();
                loginHtml = $('.login-content').html();
                $('.login-container h2').hide();
                $('.login-content').html(view);
                $('html, body').animate({ scrollTop: 150 }, '300');
                applyJS();
            }
        })
    });

    $(document).on('submit', '#forgotPasswordForm', function (e) {
        e.preventDefault();
        if (!$('.validation-summary').eq(1).is(':visible')) {
            $('.success-line').eq(1).show();
        }
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                $('.login-container .field-validation-error').text('');
                $('.validation-summary').eq(1).hide();
                forgotPasswordHtml = $('.login-content').html();
                if (response.success) {
                    $.ajax({
                        url: '/Account/ForgotPasswordConfirmation',
                        type: 'GET',
                        success: function (view) {
                            $('.login-content').html(view);
                            $('html, body').animate({ scrollTop: 150 }, '300');
                        }
                    })
                } else if (response.errors) {
                    $('.validation-summary').eq(1).show();
                    $('.validation-error').eq(1).empty();
                    $.each(response.errors, function (index, error) {
                        $('.validation-error').eq(1).append('<li>' + error + '</li>');
                    });
                }
            }
        });        
    });

    // Reset Password
    $(document).on('submit', '#resetPasswordForm', function (e) {
        e.preventDefault();
        $('.success-line').show();     
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    $.ajax({
                        url: '/Account/ResetPasswordConfirmation',
                        type: 'GET',
                        success: function (view) {
                            $('.reset-password-container').html(view);
                            $('html, body').animate({ scrollTop: 0 }, '300');
                        }
                    })
                } else if (response.error) {
                    $('.success-line').hide();
                    alert(response.error);
                } else {
                    $('.success-line').hide();
                }
            }
        })
    });

    $(document).on('click', '.returnLoginLink', function (e) {
        $('.login-content').html(loginHtml);
        $('.login-container h2').show();
        $('.success-line').eq(1).hide();
        $('html, body').animate({ scrollTop: 150 }, '300');
        applyJS();
    });

    $(document).on('click', '.returnFPLink', function (e) {
        $('.login-content').html(forgotPasswordHtml);
        $('.success-line').eq(1).hide();
        $('html, body').animate({ scrollTop: 150 }, '300');
        applyJS();
    });

    $('.register-content').hide();
    $('.register-container').css('flex-basis', '25%');
    $('.login-container').css('flex-basis', '75%');

    $('.register-container').hover(function () {
        var leftHeight = $(this).outerHeight();
        $('.register-content').show();
        $('.login-content').hide();

        $('.register-container').css({ "flex-basis": "75%", "background": "#fff" });
        $('.login-container').css({ "flex-basis": "25%", "background": "linear-gradient(135deg, #c5a25d, #ffffff)" });
    }, function () {
    });

    $('.login-container').hover(function () {
        var rightHeight = $(this).outerHeight();
        $('.login-content').show();
        $('.register-content').hide();

        $('.login-container').css({ "flex-basis": "75%", "background": "#fff" });
        $('.register-container').css({ "flex-basis": "25%", "background": "linear-gradient(135deg, #c5a25d, #ffffff)" });

        $('html, body').animate({ scrollTop: 150 }, '300');
    }, function () {
    });
});