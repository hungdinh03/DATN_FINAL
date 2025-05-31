$(document).ready(function () {
    var startX, endX;
    var threshold = 100;
    var carousel = $('#customCarousel');

    // Handle touch events for swipe on mobile
    carousel.on('touchstart', function (e) {
        startX = e.originalEvent.touches[0].clientX;
    });

    carousel.on('touchmove', function (e) {
        endX = e.originalEvent.touches[0].clientX;
        e.preventDefault();
    });

    carousel.on('touchend', function () {
        handleSwipe();
    });

    // Handle mouse events for swipe on desktop
    carousel.on('mousedown', function (e) {
        startX = e.clientX;
    });

    carousel.on('mousemove', function (e) {
        if (startX) {
            endX = e.clientX;
        }
    });

    carousel.on('mouseup', function () {
        handleSwipe();
        startX = null;
    });

    $('.product-grid').isotope({
        filter: '.product-item.all'
    });

    $('.best-seller-img').hover(
        function () {
            var hoverImage = $(this).attr('data-hover');
            $(this).stop(true, true).fadeOut(200, function () {
                $(this).attr('src', hoverImage).fadeIn(200);
            });
        },
        function () {
            var defaultImage = $(this).attr('data-default');
            $(this).stop(true, true).fadeOut(200, function () {
                $(this).attr('src', defaultImage).fadeIn(200);
            });
        }
    );

    function handleSwipe() {
        if (startX && endX) {
            if (endX - startX > threshold) {
                carousel.carousel('prev'); // swipe right to go to the previous item
            } else if (startX - endX > threshold) {
                carousel.carousel('next'); // swipe left to go to the next item
            }
        }
    }
})
