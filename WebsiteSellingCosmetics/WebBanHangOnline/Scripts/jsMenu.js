$(document).ready(function () {
    $('.navbar_menu a').off('click').on('click', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');

        $.ajax({
            url: url,
            type: 'POST',
            success: function () {
                window.location.href = url;
            },
            error: function () {
                window.location.href = '/Home/Error';
            }
        });
    });

    $('.sidebar_categories a').on('click', function (e) {
        e.preventDefault();

        $('.sidebar_categories li').removeClass('active').find('span').remove();
        $(this).parent('li').addClass('active').find('a').prepend('<span><i class="fa fa-angle-double-right" aria-hidden="true"></i></span>');

        var priceRange, priceMin, priceMax;
        if ($('.filter_button').hasClass('active')) {
            priceRange = $('#amount').val().split('-');
            priceMin = parseFloat(priceRange[0].replace(/[đ,.]/g, '').trim());
            priceMax = parseFloat(priceRange[1].replace(/[đ,.]/g, '').trim());
        }
        var sortType = $('.type_sorting_text').val();
        var url = $(this).attr('href');
        var cateName = $(this).text();

        $.ajax({
            url: url,
            type: 'GET',
            data: {
                priceMin: priceMin,
                priceMax: priceMax,
                sortType: sortType
            },
            success: function (data) {
                $('#productGrid').html($('<div></div>').html(data).find('#productGrid').html());
                $('.category-name').text(cateName);
                history.pushState(null, '', url);
            },
            error: function () {
                alert("Lỗi khi tải sản phẩm.");
            }
        });
    });
});