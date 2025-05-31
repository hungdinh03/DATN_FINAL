function loadData(url) {
    $.ajax({
        url: url,
        type: 'GET',
        data: { searchText: $('input[name="searchText"]').val(), page: 1 }, // Lấy giá trị từ ô tìm kiếm
        success: function (data) {
            $('#myTable').html($('<div></div>').html(data).find('#myTable').html());
            history.pushState(null, "", url);
            $('html, body').animate({ scrollTop: 150 }, '300');
            adjustCheckBoxes();
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
        addImageProduct(fileUrl);
    };
    finder.popup();
}

function ckEditor() {
    CKEDITOR.replace('txtDetail', {
        customConfig: '/content/ckeditor/config.js',
        extraAllowedContent: 'span',
    });
}

function addImageProduct(url) {
    var temp = $('#tCurrentId').val();
    var currentId = parseInt(temp) + 1;
    var str = "";
    if (currentId == 1) {
        str += `
            <tr id="trow_${currentId}">
                <td>${currentId}</td>
                <td>
                    <img width="100" src="${url}" />
                    <input type="hidden" value="${url}" name="Images" />
                </td>
                <td class="text-center">
                    <input type="radio" name="rDefault" value="${currentId}" checked="checked" />
                </td>
                <td class="text-center">
                    <input type="radio" name="rHover" value="${currentId}" checked="checked" />
                </td>
                <td class="text-center">
                    <input type="radio" name="rFeature" value="${currentId}" checked="checked" />
                </td>
                <td>
                    <a href="#" data-id="${currentId}" class="btn btn-sm btn-danger btnXoaAnh">Xóa</a>
                </td>
            </tr>
        `;
    } else {
        str += `
            <tr id="trow_${currentId}">
                <td>${currentId}</td>
                <td>
                    <img width="100" src="${url}" />
                    <input type="hidden" value="${url}" name="Images" />
                </td>
                <td class="text-center">
                    <input type="radio" name="rDefault" value="${currentId}" />
                </td>
                <td class="text-center">
                    <input type="radio" name="rHover" value="${currentId}" />
                </td>
                <td class="text-center">
                    <input type="radio" name="rFeature" value="${currentId}" />
                </td>
                <td>
                    <a href="#" data-id="${currentId}" class="btn btn-sm btn-danger btnXoaAnh">Xóa</a>
                </td>
            </tr>
        `;
    }
    $('#tbHtml').append(str);
    $('#tCurrentId').val(currentId);
}

function updateSTT() {
    $('.row_index').each(function (index) {
        $(this).text(index + 1);
    });
}

$(document).ready(function () {
    adjustCheckBoxes();

    $('body').off('click').on('click', '.pagination a', function (e) {
        e.preventDefault();
        if ($(this).parent().hasClass('active')) {
            return;
        }
        var url = $(this).attr('href');
        loadData(url);
    });

    $('#btnSearch').on('click', function (e) {
        e.preventDefault();
        var url = '/Admin/Product';
        loadData(url);
    });

    $('body').on('click', '.btnDelete', function () {
        var id = $(this).data('id');
        var conf = confirm('Bạn có muốn xóa bản ghi này không?');
        if (conf === true) {
            $.ajax({
                url: '/Admin/Product/Delete',
                type: 'POST',
                data: { id: id },
                success: function (result) {
                    if (result.success) {
                        $('#trow_' + id).remove();
                        updateSTT();
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
            url: '/Admin/Product/IsActive',
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

    $('body').on('click', '.btnFeature', function (e) {
        e.preventDefault();
        var btn = $(this);
        var id = btn.data('id');
        $.ajax({
            url: '/Admin/Product/IsFeature',
            type: 'POST',
            data: { id: id },
            success: function (result) {
                if (result.success) {
                    if (result.isFeature) {
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
                    url: '/Admin/Product/DeleteAll',
                    type: 'POST',
                    data: { ids: str },
                    success: function (result) {
                        if (result.success) {
                            location.reload();
                        }
                    }
                })
            }
        }
    });

    $('body').on('click', '#linkTaiAnh', function () {
        browseServer(field);
    });

    $('body').on('click', '.btnXoaAnh', function () {
        var conf = confirm("Bạn muốn xóa bản ghi này?");
        if (conf === true) {
            var _id = $(this).data('id');
            $('#trow_' + _id).remove();
            var temp = $('#tCurrentId').val();
            var currentId = parseInt(temp) - 1;
            $('#tCurrentId').val(currentId);
        }
    });

    $('body').on('submit', '#addForm', function (e) {
        e.preventDefault();
        if ($('#addForm .input-validation-error').length) {
            $('.nav-item a[href="#activity"]').tab('show');
            return;
        }
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    alert("Thêm sản phẩm thành công");
                    window.location.href = '/Admin/Product';
                } else {
                    alert("Thêm sản phẩm thất bại");
                    $('.nav-item a[href="#activity"]').tab('show');
                    $('#btnSave').trigger('click');
                }
            }
        })
    });

    $('body').on('submit', '#editForm', function (e) {
        e.preventDefault();
        if ($('#editForm .input-validation-error').length) {
            $('.nav-item a[href="#activity"]').tab('show');
            return;
        }
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    alert("Cập nhật sản phẩm thành công");
                    window.location.href = document.referrer;
                } else {
                    alert("Cập nhật sản phẩm thất bại");
                    $('.nav-item a[href="#activity"]').tab('show');
                    $('#btnSave').trigger('click');
                }
            }
        })
    });
});