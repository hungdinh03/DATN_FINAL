function loadData(url) {
    $.ajax({
        url: url,
        type: 'GET',
        data: {
            searchText: $('input[name="searchText"]').val(),
            page: $(this).data('page')
        },
        success: function (data) {
            $('#myTable').html($('<div></div>').html(data).find('#myTable').html());
        },
        error: function () {
            alert("Lỗi khi tải thông tin.");
        }
    });
}

function adjustCheckBoxes() {
    $('.cbkItem').on('change', function () {
        if ($('.cbkItem:checked').length !== $('.cbkItem').length) {
            $('#selectAll').prop('checked', false);
        } else if ($('.cbkItem:checked').length === $('.cbkItem').length) {
            $('#selectAll').prop('checked', true);
        }
    });
}

function browseServer(field) {
    var finder = new CKFinder();
    finder.selectActionFunction = function (fileUrl) {
        document.getElementById(field).value = fileUrl;
    };
    finder.popup();
}

function ckEditor() {
    CKEDITOR.replace('txtDetail', {
        customConfig: '/content/ckeditor/config.js',
        extraAllowedContent: 'span',
    });
}

$(document).ready(function () {
    adjustCheckBoxes();

    $(document).off('click').on('click', '.pagination a', function (e) {
        e.preventDefault();
        if ($(this).parent().hasClass('active')) {
            return;
        }
        var url = $(this).attr('href');
        loadData(url);
    });

    $('#btnSearch').on('click', function (e) {
        e.preventDefault();
        var url = '/Admin/News';
        loadData(url);
    });

    $('body').on('click', '.btnDelete', function () {
        var id = $(this).data('id');
        var conf = confirm('Bạn có muốn xóa bản ghi này không?');
        if (conf === true) {
            $.ajax({
                url: '/Admin/News/Delete',
                type: 'POST',
                data: { id: id },
                success: function (result) {
                    if (result.success) {
                        $('#trow_' + id).remove();

                        // Cập nhật lại số thứ tự sau khi xóa
                        $('#myTable tbody tr').each(function (index) {
                            $(this).find('td:nth-child(2)').text(index + 1); // cập nhật lại STT
                        });
                    }
                }
            });
        }
    });
    $('body').on('click', '.btnActive', function (e) {
        e.preventDefault();
        var btn = $(this);
        var id = btn.data('id');
        $.ajax({
            url: '/Admin/News/IsActive',
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
        });
    });
    $('body').on('click', '.btnHome', function (e) {
        e.preventDefault();
        var btn = $(this);
        var id = btn.data('id');
        $.ajax({
            url: '/Admin/News/IsHome',
            type: 'POST',
            data: { id: id },
            success: function (result) {
                if (result.success) {
                    if (result.isHome) {
                        btn.html("<i class='fa fa-check text-success'></i>");
                    }
                    else {
                        btn.html("<i class='fa fa-times text-danger'></i>");
                    }
                }
            }
        });
    });
    $('body').on('change', '#selectAll', function () {
        var checkStatus = this.checked;
        var checkbox = $(this).parents('.card-body').find('tr td input:checkbox');
        checkbox.each(function () {
            this.checked = checkStatus;
            if (this.checked) {
                checkbox.attr('selected', 'checked');
            } else {
                checkbox.attr('selected', '');
            }
        });
    });
    $('body').on('click', '#btnDeleteAll', function (e) {
        e.preventDefault();
        var str = '';
        var checkbox = $(this).parents('.card').find('tr td input:checkbox');
        var i = 0;
        checkbox.each(function () {
            if (this.checked) {
                checkbox.attr('selected', 'checked');
                var _id = $(this).val();
                if (i === 0) {
                    str += _id;
                } else {
                    str += ',' + _id;
                }
                i++;
            } else {
                checkbox.attr('selected', '');
            }
        });
        if (str.length > 0) {
            var conf = confirm('Bạn có muốn xóa (các) bản ghi này không?');
            if (conf === true) {
                $.ajax({
                    url: '/Admin/News/DeleteAll',
                    type: 'POST',
                    data: { ids: str },
                    success: function (result) {
                        if (result.success) {
                            location.reload();
                        }
                    }
                });
            }
        }
    });

    $('body').on('submit', '#addForm', function (e) {
        e.preventDefault();
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    alert("Thêm tin tức thành công");
                    window.location.href = '/Admin/News';
                } else {
                    alert("Thêm tin tức thất bại");
                }
            }
        })
    });

    $('body').on('submit', '#editForm', function (e) {
        e.preventDefault();
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    alert("Cập nhật tin tức thành công");
                    window.location.href = '/Admin/News';
                } else {
                    alert("Cập nhật tin tức thất bại");
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