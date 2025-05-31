/* JS Document */

/******************************

[Table of Contents]

0. Vars and Inits
1. Set Header
2. Init Review Ajax
3. Init Menu
4. Init Thumbnail
5. Init Quantity
6. Init Star Rating
7. Init Favorite
8. Init Tabs

******************************/

jQuery(document).ready(function($)
{
	"use strict";

	/* 

	0. Vars and Inits

	*/

	var header = $('.header');
	var topNav = $('.top-nav')
	var hamburger = $('.hamburger-container');
	var menu = $('.hamburger-menu');
	var menuActive = false;
	var hamburgerClose = $('.hamburger-close');
	var fsOverlay = $('.fs-menu-overlay');

	setHeader();

	$(window).on('resize', function()
	{
		setHeader();
	});

	$(document).on('scroll', function()
	{
		setHeader();
	});

	initMenu();
	initThumbnail();
	initQuantity();
	initStarRating();
	initFavorite();
	initTabs();

	/* 

	1. Set Header

	*/

	function setHeader()
	{
		if (window.innerWidth < 992)
		{
			if ($(window).scrollTop() > 100)
			{
				header.css({'top':"0"});
			}
			else
			{
				header.css({'top':"0"});
			}
		}
		else
		{
			if ($(window).scrollTop() > 100)
			{
				header.css({'top':"-50px"});
			}
			else
			{
				header.css({'top':"0"});
			}
		}
		if (window.innerWidth > 991 && menuActive)
		{
			closeMenu();
		}
	}

	/*

	2. Init Review Ajax

	*/

	$(document).off('click').on('click', '.pagination a', function (event) {
		event.preventDefault();

		// Kiểm tra nếu nút phân trang đang được active
		if ($(this).parent().hasClass('active')) {
			return;
		}

		var url = $(this).attr('href'); // Lấy URL từ link phân trang

		$.ajax({
			url: url,
			type: 'GET',
			success: function (data) {
				$('#loadReview').html(data); // Cập nhật nội dung đánh giá
			},
			error: function () {
				alert("Lỗi khi tải đánh giá.");
			}
		});
	});

	/* 

	3. Init Menu

	*/

	function initMenu()
	{
		if (hamburger.length)
		{
			hamburger.on('click', function()
			{
				if (!menuActive)
				{
					openMenu();
				}
			});
		}

		if (fsOverlay.length)
		{
			fsOverlay.on('click', function()
			{
				if (menuActive)
				{
					closeMenu();
				}
			});
		}

		if (hamburgerClose.length)
		{
			hamburgerClose.on('click', function()
			{
				if (menuActive)
				{
					closeMenu();
				}
			});
		}

		if ($('.menu-item').length)
		{
			var items = document.getElementsByClassName('menu-item');
			var i;

			for(i = 0; i < items.length; i++)
			{
				if (items[i].classList.contains("has-children"))
				{
					items[i].onclick = function()
					{
						this.classList.toggle("active");
						var panel = this.children[1];
					    if (panel.style.maxHeight)
					    {
					    	panel.style.maxHeight = null;
					    }
					    else
					    {
					    	panel.style.maxHeight = panel.scrollHeight + "px";
					    }
					}
				}	
			}
		}
	}

	function openMenu()
	{
		menu.addClass('active');
		// menu.css('right', "0");
		fsOverlay.css('pointer-events', "auto");
		menuActive = true;
	}

	function closeMenu()
	{
		menu.removeClass('active');
		fsOverlay.css('pointer-events', "none");
		menuActive = false;
	}

	/* 

	4. Init Thumbnail

	*/

	function initThumbnail()
	{
		if ($('.single-product-thumbnails ul li').length)
		{
			var thumbs = $('.single-product-thumbnails ul li');
			var singleImage = $('.single-product-image-background');

			thumbs.each(function()
			{
				var item = $(this);
				item.on('click', function()
				{
					thumbs.removeClass('active');
					item.addClass('active');
					var img = item.find('img').data('image');
					singleImage.css('background-image', 'url(' + img + ')');
				});
			});
		}	

		$('.product-image img').hover(
			function () {
				// Khi hover vào ảnh
				var hoverSrc = $(this).data('src-hover');
				$(this).stop().fadeTo(300, 0.5, function () {
					$(this).attr('src', hoverSrc).stop().fadeTo(300, 1);
				});
			},
			function () {
				// Khi rời khỏi ảnh
				var defaultSrc = $(this).data('src');
				$(this).stop().fadeTo(300, 0.5, function () {
					$(this).attr('src', defaultSrc).stop().fadeTo(300, 1);
				});
			}
		);
	}

	/* 

	5. Init Quantity	

	*/


	function initQuantity()
	{
		var plus = $('.plus');
		var minus = $('.minus');
		var value = $('#quantity');
		if (plus.length && minus.length) {
			var leftQuantity = parseInt($('.quantity-left').find('span').text());

			plus.on('click', function () {
				var x = parseInt(value.text());
				value.text(Math.min(x + 1, leftQuantity));
			});

			minus.on('click', function () {
				var x = parseInt(value.text());
				value.text(Math.max(x - 1, 1));
			});
		}
	}



	/* 

	6. Init Star Rating

	*/

	function initStarRating()
	{
		if ($('.user_star-rating li').length)
		{
			var stars = $('.user_star-rating li');
			var dem = 0;

			stars.each(function()
			{
				var star = $(this);

				star.on('click', function()
				{
					var i = star.index();
					dem = 0;
					stars.find('i').each(function()
					{
						$(this).removeClass('fa-star');
						$(this).addClass('fa-star-o');
					});
					for(var x = 0; x <= i; x++)
					{
						$(stars[x]).find('i').removeClass('fa-star-o');
						$(stars[x]).find('i').addClass('fa-star');
						dem++;
					};
					$("#txtRate").val(dem);
				});
			});
		}
	}

	/* 

	7. Init Favorite

	*/

	function initFavorite()
	{
		if ($('.product_favorite').length)
		{
			var fav = $('.product_favorite');

			fav.on('click', function()
			{
				fav.toggleClass('active');
			});
		}
	}

	/* 

	8. Init Tabs

	*/

	function initTabs()
	{
		if ($('.tabs').length)
		{
			var tabs = $('.tabs li');
			var tabContainers = $('.tab-container');

			tabs.each(function()
			{
				var tab = $(this);
				var tab_id = tab.data('active-tab');

				tab.on('click', function()
				{
					if (!tab.hasClass('active'))
					{
						tabs.removeClass('active');
						tabContainers.removeClass('active');
						tab.addClass('active');
						$('#' + tab_id).addClass('active');
					}
				});
			});
		}
	}
});