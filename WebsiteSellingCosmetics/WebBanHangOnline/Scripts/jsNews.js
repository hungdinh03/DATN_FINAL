$(document).ready(function () {
    $(document).off('click').on('click', '.pagination a', function (e) {
        e.preventDefault();
        if ($(this).parent().hasClass('active')) {
            return;
        }
        $.ajax({
            url: $(this).attr('href'),
            type: 'GET',
            success: function (data) {
                $('.news-list').html($('<div></div>').html(data).find('.news-list').html());
                $('html, body').animate({ scrollTop: 0 }, '300');
            },
            error: function () {
                alert("Lỗi khi tải thông tin.");
            }
        });
    })
})