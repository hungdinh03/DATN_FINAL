/* JS Document */

/******************************

[Table of Contents]

1. Vars and Inits
2. Set Header
3. Init Menu
4. Init Favorite
5. Init Fix Product Border
6. Init Isotope Filtering
7. Init Price Slider
8. Init Checkboxes
9. Init Hover


******************************/

jQuery(document).ready(function($)
{
	"use strict";

	/* 

	1. Vars and Inits

	*/

	var header = $('.header');
	var topNav = $('.top-nav')
	var mainSlider = $('.main-slider');
	var hamburger = $('.hamburger-container');
	var menu = $('.hamburger-menu');
	var menuActive = false;
	var hamburgerClose = $('.hamburger-close');
	var fsOverlay = $('.fs-menu-overlay');

	setHeader();

	$(window).on('resize', function()
	{
		initFixProductBorder();
		setHeader();
	});

	$(document).on('scroll', function()
	{
		setHeader();
	});

	initMenu();
	initFavorite();
	initFixProductBorder();
	initIsotopeFiltering();
	initPriceSlider();
	initCheckboxes();

	/* 

	2. Set Header

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

			for (i = 0; i < items.length; i++)
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

	4. Init Favorite

	*/

    function initFavorite()
    {
    	if ($('.favorite').length)
    	{
    		var favs = $('.favorite');

    		favs.each(function()
    		{
    			var fav = $(this);
    			var active = false;
    			if (fav.hasClass('active'))
    			{
    				active = true;
    			}

    			fav.on('click', function()
    			{
    				if (active)
    				{
    					fav.removeClass('active');
    					active = false;
    				}
    				else
    				{
    					fav.addClass('active');
    					active = true;
    				}
    			});
    		});
    	}
    }

    /* 

	5. Init Fix Product Border

	*/

    function initFixProductBorder()
    {
    	if ($('.product-filter').length)
    	{
			var products = $('.product-filter:visible');
    		var wdth = window.innerWidth;

    		// reset border
    		products.each(function()
    		{
    			$(this).css('border-right', 'solid 1px #e9e9e9');
    		});

    		// if window width is 991px or less

    		if (wdth < 480)
			{
				for(var i = 0; i < products.length; i++)
				{
					var product = $(products[i]);
					product.css('border-right', 'none');
				}
			}

    		else if (wdth < 576)
			{
				if (products.length < 5)
				{
					var product = $(products[products.length - 1]);
					product.css('border-right', 'none');
				}
				for(var i = 1; i < products.length; i+=2)
				{
					var product = $(products[i]);
					product.css('border-right', 'none');
				}
			}

    		else if (wdth < 768)
			{
				if (products.length < 5)
				{
					var product = $(products[products.length - 1]);
					product.css('border-right', 'none');
				}
				for(var i = 2; i < products.length; i+=3)
				{
					var product = $(products[i]);
					product.css('border-right', 'none');
				}
			}

    		else if (wdth < 992)
			{
				if (products.length < 5)
				{
					var product = $(products[products.length - 1]);
					product.css('border-right', 'none');
				}
				for(var i = 2; i < products.length; i+=3)
				{
					var product = $(products[i]);
					product.css('border-right', 'none');
				}
			}

			//if window width is larger than 991px
			else
			{
				if (products.length < 5)
				{
					var product = $(products[products.length - 1]);
					product.css('border-right', 'none');
				}
				for(var i = 3; i < products.length; i+=4)
				{
					var product = $(products[i]);
					product.css('border-right', 'none');
				}
			}	
    	}
    }

    /* 

	6. Init Isotope Filtering

	*/

    function initIsotopeFiltering()
    {
    	var sortTypes = $('.type_sorting_btn');
    	var sortNums = $('.num_sorting_btn');
    	var sortTypesSelected = $('.sorting_type .item_sorting_btn is-checked span');
    	var filterButton = $('.filter_button');

    	if ($('.product-grid').length)
    	{
    		$('.product-grid').isotope({
    			itemSelector: '.product-item',
	            getSortData: {
	            	price: function(itemElement)
	            	{
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

    		// Short based on the value from the sorting_type dropdown
			sortTypes.each(function () {
				$(this).on('click', function () {
					var option = $(this).attr('data-isotope-option');
					option = JSON.parse(option);
					$('.type_sorting_text').text($(this).text()).val(option.sortBy);					

					// Sửa lại phần sắp xếp giá
					if (option.sortBy === 'price') {
						option.getSortData = {
							price: function (itemElement) {
								var priceEle = $(itemElement).find('.product-price').clone().children().remove().end().text().replace(/[đ,.]/g, '').trim();
								return parseFloat(priceEle);
							}
						};
					}

					$('.product-grid').isotope(option);
				});
			});

	        // Show only a selected number of items
	        sortNums.each(function()
	        {
	        	$(this).on('click', function()
	        	{
	        		var numSortingText = $(this).text();
					var numFilter = ':nth-child(-n+' + numSortingText + ')';
	        		$('.num_sorting_text').text($(this).text());
    				$('.product-grid').isotope({ filter: numFilter });
	        	});
	        });	

			// Filter based on the price range slider
			filterButton.on('click', function () {				
				$('.product-grid').isotope({
					filter: function () {
						var priceRange = $('#amount').val().split('-');
						var priceMin = parseFloat(priceRange[0].replace(/[đ,.]/g, '').trim());
						var priceMax = parseFloat(priceRange[1].replace(/[đ,.]/g, '').trim());
						var itemPrice = $(this).find('.product-price').clone().children().remove().end().text().replace(/[đ,.]/g, '').trim();

						return (itemPrice >= priceMin) && (itemPrice <= priceMax);
					},
					animationOptions: {
						duration: 750,
						easing: 'linear',
						queue: false
					}
				});
			});
    	}
    }

    /* 

	7. Init Price Slider

	*/

	function formatCurrency(value) {
		return value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
	}

	function initPriceSlider() {
		$("#slider-range").slider(
			{
				range: true,
				min: 0,
				max: 1000000,
				values: [0, 1000000],
				slide: function (event, ui) {
					$("#amount").val(formatCurrency(ui.values[0]) + "đ - " + formatCurrency(ui.values[1]) + "đ");
				}
			});

		$("#amount").val(formatCurrency($("#slider-range").slider("values", 0)) + "đ - " + formatCurrency($("#slider-range").slider("values", 1)) + "đ");
	}


    /* 

	8. Init Checkboxes

	*/

    function initCheckboxes()
    {
    	if ($('.checkboxes li').length)
    	{
    		var boxes = $('.checkboxes li');

    		boxes.each(function()
    		{
    			var box = $(this);

    			box.on('click', function()
    			{
    				if (box.hasClass('active'))
    				{
    					box.find('i').removeClass('fa-square');
    					box.find('i').addClass('fa-square-o');
    					box.toggleClass('active');
    				}
    				else
    				{
    					box.find('i').removeClass('fa-square-o');
    					box.find('i').addClass('fa-square');
    					box.toggleClass('active');
    				}
    				// box.toggleClass('active');
    			});
    		});

    		if ($('.show-more').length)
    		{
    			var checkboxes = $('.checkboxes');

    			$('.show-more').on('click', function()
    			{
    				checkboxes.toggleClass('active');
    			});
    		}
    	};
	}

	/*
	
	9. Init Hover

	*/

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
});