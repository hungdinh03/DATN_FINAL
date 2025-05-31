$(document).ready(function () {
    $('body').on('click', '.btnDelete', function () {
        var id = $(this).data('id');
        var conf = confirm('Bạn có muốn xóa bản ghi này không?');
        if (conf === true) {
            $.ajax({
                url: '/Admin/Category/Delete',
                type: 'POST',
                data: { id: id },
                success: function (result) {
                    if (result.success) {
                        $('#trow_' + id).remove();
                        $('#myTable tbody tr').each(function (index) {
                            $(this).find('#catePos').text(index + 1);
                        });
                    }
                }
            });
        }
    });

    $(document).on('submit', '#addForm', function (e) {
        e.preventDefault();
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    alert("Thêm danh mục thành công");
                    window.location.href = '/Admin/Category';
                } else {
                    alert("Thêm danh mục thất bại");
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
                    alert("Sửa danh mục thành công");
                    window.location.href = '/Admin/Category';
                } else {
                    alert("Sửa danh mục thất bại");
                }
            }
        })
    })

    $('#positionTxtBox').on('change', function () {
        var pos = Math.min(parseInt($(this).attr('max')), Math.max(parseInt($(this).attr('min')), parseInt($(this).val())));
        $(this).val(pos);
    })
});