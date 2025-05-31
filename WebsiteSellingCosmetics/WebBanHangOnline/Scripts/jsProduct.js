$(document).ready(function () { 
	applyCSSforProductGrid();
	$('.filter_button').on('click', function (e) {
		e.preventDefault();
		var priceMin, priceMax;
		if ($(this).hasClass('active')) {
			$(this).removeClass('active');
		} else {
			$(this).addClass('active');
			var priceRange = $('#amount').val().split('-');
			priceMin = parseFloat(priceRange[0].replace(/[đ,.]/g, '').trim());
			priceMax = parseFloat(priceRange[1].replace(/[đ,.]/g, '').trim());
		}
		var sortType = $('.type_sorting_text').val();
		var url = $('.active a').attr('href');

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
				$('html, body').animate({ scrollTop: 150 }, '300');
				history.pushState(null, '', url);
				applyCSSforProductGrid();
			},
			error: function () {
				alert("Lỗi khi tải sản phẩm.");
			}
		})
	})

	$('.type_sorting_btn').on('click', function (e) {
		e.preventDefault();

		var priceRange, priceMin, priceMax;
		if ($('.filter_button').hasClass('active')) {
			priceRange = $('#amount').val().split('-');
			priceMin = parseFloat(priceRange[0].replace(/[đ,.]/g, '').trim());
			priceMax = parseFloat(priceRange[1].replace(/[đ,.]/g, '').trim());
		}
		var sortType = JSON.parse($(this).attr('data-isotope-option')).sortBy;

		var url = $('.active a').attr('href');
		console.log(url);
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
				history.pushState(null, '', url);
				applyCSSforProductGrid();
			},
			error: function () {
				alert("Lỗi khi tải sản phẩm.");
			}
		})
	})

	$(document).off('click').on('click', '.pagination a', function (e) {
		e.preventDefault();
		if ($(this).parent().hasClass('active')) {
			return;
		}
		var url = $(this).attr('href');
		$.ajax({
			url: url,
			type: 'GET',
			success: function (data) {
				$('#productGrid').html($('<div></div>').html(data).find('#productGrid').html());
				history.pushState(null, '', url);
				applyCSSforProductGrid();
			},
			error: function () {
				alert("Lỗi khi tải sản phẩm.");
			}
		})
	});
})

function applyCSSforProductGrid() {
	$('.product-image img').hover(
		function () {
			var hoverSrc = $(this).data('src-hover');
			$(this).stop().fadeTo(300, 0.5, function () {
				$(this).attr('src', hoverSrc).stop().fadeTo(300, 1);
			});
		},
		function () {
			var defaultSrc = $(this).data('src');
			$(this).stop().fadeTo(300, 0.5, function () {
				$(this).attr('src', defaultSrc).stop().fadeTo(300, 1);
			});
		}
	);

	$('.product-grid').isotope({
		itemSelector: '.product-item',
		layoutMode: 'fitRows',
		getSortData: {
			price: function (itemElement) {
				var priceEle = $(itemElement).find('.product-price').text().replace('đ', '');
				return parseFloat(priceEle);
			},
			name: '.product-name'
		},
		animationOptions: {
			duration: 750,
			easing: 'linear',
			queue: false
		}
	});
}