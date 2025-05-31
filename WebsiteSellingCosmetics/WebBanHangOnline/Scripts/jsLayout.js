$('.header').show();
$('.hamburger-menu').show();

$(document).ready(function () {
    // Update cart items amount display
    showCount();

    $('#logoffLink').on('click', function (e) {
        e.preventDefault();
        var confirmation = confirm("Bạn có chắc muốn đăng xuất?");
        if (confirmation) {
            $('#logoffForm').submit();
        }
    });

    // Xử lý sự kiện khi nhập liệu vào ô tìm kiếm
    $('#search_input').on('input', function () {
        var query = $(this).val();
        if (query.length > 0) {
            $.ajax({
                url: '/Product/SearchProducts',
                type: 'GET',
                data: { searchTerm: query },
                success: function (data) {
                    $('#search_results').html(''); // Xóa kết quả cũ
                    if (data.length > 0) {
                        data.forEach(function (item) {
                            $('#search_results').append('<p><a class="search-result" href="/chi-tiet/' + item.Alias + '-p' + item.Id + '">' + item.Title + '</a></p>');
                        });
                        $("#no_results").hide();
                    } else {
                        $("#no_results").show();
                    }
                }
            });
        } else {
            $('#search_results').html(''); // Nếu ô input trống, xóa kết quả
        }
    });

    $('.hamburger-container').click(function () {
        $('.navbar_menu').eq(0).hide();
        $('.navbar_menu').eq(1).find('li').each(function () {
            $(this).addClass('menu-item');
        });

        $('.navbar_menu').eq(1).removeClass('navbar_menu');
    });

    $(window).resize(function () {
        if ($('.hamburger-container').css('display') === 'none') {
            $('.navbar_menu').show();
        }
    });

    $('#search_btn').click(function (e) {
        e.preventDefault(); // Ngăn trang cuộn về đầu
        $('#search_popup').toggle();
        $('#search_input').focus();
    });

    // Xử lý sự kiện nhấp chuột bên ngoài popup
    $(document).mouseup(function (e) {
        var container = $("#search_popup");
        var button = $('#search_btn');

        // Kiểm tra nếu nhấp chuột bên ngoài popup và nút tìm kiếm
        if (!container.is(e.target) && container.has(e.target).length === 0 && !button.is(e.target) && !button.has(e.target).length) {
            container.hide();
        }
    });

    $('li.account').hover(function () {
        $(this).find('ul.account_selection').show(150);
    }, function () {
        // Khi rời hover khỏi .account, ẩn ul.account_selection ngay lập tức
        $(this).find('ul.account_selection').hide();
    });

    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $('.back-to-top').fadeIn();
        } else {
            $('.back-to-top').fadeOut();
        }
    });

    $('.back-to-top').on('click', function (e) {
        e.preventDefault();
        $('html, body').animate({ scrollTop: 0 }, '300');
    });
});

function showCount() {
    $.ajax({
        url: '/ShoppingCart/ShowCount',
        type: 'GET',
        success: function (result) {
            $('#cartItemCount').html(result.count);
        }
    });
}