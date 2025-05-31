$(document).ready(function () {
    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var conf = confirm('Bạn có muốn xóa bản ghi này không?');
        if (conf === true) {
            $.ajax({
                url: '/Admin/SystemConfig/Delete',
                type: 'POST',
                data: { id: id },
                success: function (result) {
                    if (result.success) {
                        $('#trow_' + id).remove();
                        $('#myTable tbody tr').each(function (index) {
                            $(this).find('#configPos').text(index + 1);
                        });

                        var maxPos = parseInt($('.position').attr('max')) - 1;
                        $('.position').attr('max', maxPos);
                        $('.position').attr('placeholder', `Nhập số từ 1 - ${maxPos}`);
                    }
                }
            });
        }
    });

    $('body').on('click', '.btnAdd', function (e) {
        e.preventDefault();
        $('#modal-default-add').modal('show');
    });

    $('body').on('submit', '#addForm', function (e) {
        e.preventDefault();
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    alert("Thêm cấu hình thành công");
                    window.location.href = '/Admin/SystemConfig';
                } else {
                    alert("Thêm cấu hình thất bại");
                }
            }
        })
    });

    $('body').on('click', '.btnEdit', function (e) {
        e.preventDefault();
        var row = $('#trow_' + $(this).data('id') + ' td');
        $('#modal-default-edit #systemConfigId').val($(this).data('id'));
        $('#modal-default-edit #title').val(row.eq(0).text());
        $('#modal-default-edit #position').val(row.eq(1).text());
        $('#modal-default-edit').modal('show');
    });

    $('body').on('submit', '#editForm', function (e) {
        e.preventDefault();
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    alert("Sửa cấu hình thành công");
                    window.location.href = '/Admin/SystemConfig';
                } else {
                    alert("Sửa cấu hình thất bại");
                }
            }
        })
    });

    $('.position').on('change', function () {
        var pos = Math.min(parseInt($(this).attr('max')), Math.max(parseInt($(this).attr('min')), parseInt($(this).val())));
        $(this).val(pos);
    })
});