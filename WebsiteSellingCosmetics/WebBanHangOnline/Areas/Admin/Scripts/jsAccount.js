function loadData(url) {
    $.ajax({
        url: url,
        type: 'GET',
        data: { searchText: $('input[name="searchText"]').val(), page: 1 },
        success: function (data) {
            $('#myTable').html($('<div></div>').html(data).find('#myTable').html());
            history.pushState(null, '', url);
            $('html, body').animate({ scrollTop: 150 }, '300');
        },
        error: function () {
            alert('Lỗi khi tải thông tin.');
        }
    })
}

$(document).ready(function () {
    $('body').on('submit', '#loginForm', function (e) {
        e.preventDefault();
        if (!$('.validation-summary').is(':visible')) {
            $('.success-line').show();
        }        
        $.ajax({
            url: this.url,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                console.log(response);
                if (response.success) {
                    window.location.href = response.redirectUrl;
                } else if (response.error) {
                    $('.success-line').hide();
                    $('.validation-summary').show();
                    $('.validation-error').text(response.error);
                } else {
                    $('.success-line').hide();
                    $('.validation-summary').hide();
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

    $('#loginForm input').on('input', function () {
        $('.success-line').hide();
        $('.validation-summary').hide();
    });

    $('#btnSearch').on('click', function (e) {
        e.preventDefault();
        var url = '/Admin/Account';
        loadData(url);
    });

    $(document).off('click').on('click', '.pagination a', function (e) {
        e.preventDefault();
        if ($(this).parent().hasClass('active')) {
            return;
        }
        var url = $(this).attr('href');
        loadData(url);
    });

    $(document).on('click', '.btnActive', function (e) {
        e.preventDefault();
        var btn = $(this);
        var id = btn.data('id');
        $.ajax({
            url: '/Admin/Account/IsActive',
            type: 'POST',
            data: { id: id },
            success: function (result) {
                if (result.success) {
                    if (result.isActive) {
                        btn.html("<i class='fa fa-check text-success'></i>");
                    }
                    else {
                        btn.html("<i class='fa fa-times text-danger'></i>");
                    }
                }
            }
        })
    });

    $(document).on('click', '.btnDelete', function () {
        var id = $(this).data('id');
        var conf = confirm('Bạn có muốn xóa bản ghi này không?');
        if (conf === true) {
            $.ajax({
                url: '/Admin/Account/Delete',
                type: 'POST',
                data: { id: id },
                success: function (response) {
                    if (response.success) {
                        $('#trow_' + id).remove();
                        $('#myTable tbody tr').each(function (index) {
                            $(this).find('td:first').text(index + 1);
                        });
                    } else {
                        alert(response.error);
                    }
                },
                error: function () {
                    alert('Có lỗi xảy ra khi xóa dữ liệu.');
                }
            })
        }
    });

    $(document).on('submit', '#createForm', function (e) {
        e.preventDefault();
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    $('#validation-summary').hide();
                    alert('Tạo tài khoản thành công');
                    window.location.href = '/Admin/Account';
                } else if (response.error) {
                    console.log(response.error);
                    $('#validation-summary').show();
                    $('#validation-error').text(response.error);
                } else {
                    $('#validation-summary').hide();
                }
            }
        })
    });



    $(document).on('submit', '#editForm', function (e) {
        e.preventDefault();
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    $('#validation-summary').hide();
                    alert("Sửa cấu hình thành công");
                    window.location.href = '/Admin/Account';
                } else if (response.error) {
                    $('#validation-summary').show();
                    $('#validation-error').text(response.error);
                } else {
                    $('#validation-summary').hide();
                }
            }
        })
    });

    $('.nav-link').eq(0).on('click change', function () {
        var body = $('body');
        if (body.hasClass('sidebar-closed')) {
            body.removeClass('sidebar-closed sidebar-collapse').addClass('sidebar-open');
            $('.content-wrapper').css('min-height', '527px');
        } else {
            body.removeClass('sidebar-open').addClass('sidebar-closed sidebar-collapse');
            $('.content-wrapper').css('min-height', '851.5px');
        }
    });
});