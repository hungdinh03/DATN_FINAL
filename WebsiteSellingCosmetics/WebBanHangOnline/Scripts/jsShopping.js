$(document).ready(function () {
    // Add to cart
    $('body').on('click', '.btnAddToCart', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var quantity = 1;

        // Nếu đang ở trang chi tiết sản phẩm và có hiển thị số lượng
        if ($('#quantity').length) {
            quantity = parseInt($('#quantity').text());
        }
        // Nếu có input số lượng (trường hợp trang chi tiết có input)
        else if ($('#product-quantity').length) {
            quantity = parseInt($('#product-quantity').val());
        }

        if (isNaN(quantity)) {
            quantity = 1;
        }

        addToCart(id, quantity);
    });

    // Buy Now
    $('body').on('click', '.btnBuyNow', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var quantity = 1;

        // Nếu đang ở trang chi tiết sản phẩm và có hiển thị số lượng
        if ($('#quantity').length) {
            quantity = parseInt($('#quantity').text());
        }
        // Nếu có input số lượng (trường hợp trang chi tiết có input)
        else if ($('#product-quantity').length) {
            quantity = parseInt($('#product-quantity').val());
        }

        if (isNaN(quantity)) {
            quantity = 1;
        }

        buyNow(id, quantity);
    });

    // Cart item interaction
    updateTotalPrice();

    $('#selectAll').on('change', function () {
        $('.cbkItem').prop('checked', this.checked);
        updateTotalPrice();
    });

    $('.cbkItem').on('change', function () {
        if ($('.cbkItem:checked').length !== $('.cbkItem').length) {
            $('#selectAll').prop('checked', false);
        } else if ($('.cbkItem:checked').length === $('.cbkItem').length) {
            $('#selectAll').prop('checked', true);
        }
        updateTotalPrice();
    });

    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var conf = confirm("Bạn muốn xóa sản phẩm này khỏi giỏ hàng?");
        if (conf === true) {
            $.ajax({
                url: '/ShoppingCart/Delete',
                type: 'POST',
                data: { id: id },
                success: function (result) {
                    $('#cartItemCount').html(result.count);
                    $('#trow_' + id).remove();
                    updateTotalPrice();
                    updateSTT();
                    if ($('#cartItemCount').html() === '0') {
                        loadCart();
                        $('.btnDeleteAll, .btnOrder').hide();
                    }
                }
            });
        }
    });

    $('body').on('click', '.btnDeleteAll', function (e) {
        e.preventDefault();
        var conf = confirm("Bạn muốn xóa hết sản phẩm trong giỏ hàng?");
        if (conf === true) {
            $.ajax({
                url: '/ShoppingCart/DeleteAll',
                type: 'POST',
                success: function (result) {
                    // location.reload();
                    $('#cartItemCount').html(0);
                    loadCart();
                    $('.btnDeleteAll, .btnOrder').hide();
                }
            });
        }
    });

    // Update item's quantity
    $('.quantity-input').on('change', function () {
        var quantity = parseInt($(this).val());
        quantity = Math.min(parseInt($(this).attr('max')), Math.max(parseInt($(this).attr('min')), quantity));
        $(this).val(quantity);

        var price = parseFloat($(this).data('price')) || 0;
        var productId = $(this).attr('id');

        if (quantity > 0) {
            var totalPrice = quantity * price;
            $('#totalPrice_' + productId).html(totalPrice.toLocaleString('vi-VN') + '<u>đ</u>');

            // Cập nhật thuộc tính data-price của checkbox
            $('.cbkItem[value="' + productId + '"]').attr('data-price', totalPrice);
            updateTotalPrice();

            $.ajax({
                url: '/ShoppingCart/Update',
                type: 'POST',
                data: { id: productId, quantity: quantity },
                success: function (response) {
                    if (!response.success) {
                        alert("Có lỗi xảy ra khi cập nhật giỏ hàng.");
                    }
                },
                error: function () {
                    alert("Có lỗi xảy ra khi gửi yêu cầu.");
                }
            });
        }
    });

    // Order
    $('.btnOrder').on('click', function (e) {
        e.preventDefault();
        var selectedProductIds = [];
        $('.cbkItem:checked').each(function () {
            selectedProductIds.push($(this).val());
        });

        if (selectedProductIds.length === 0) {
            alert("Vui lòng chọn sản phẩm để đặt hàng!");
            return;
        }

        $.ajax({
            type: 'POST',
            url: '/ShoppingCart/ItemCheckOut',
            data: { selectedProductIds: selectedProductIds },
            success: function (response) {
                if (response.success) {
                    window.location.href = '/thanh-toan';
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert("Lỗi khi lưu ID các sản phẩm được chọn.");
            }
        })
    });

    // Checkout
    $('body').on('change', '#typePayment', function () {
        var type = $(this).val();
        $('#loadFormPayment').hide();
        if (type == "2") {
            $('#loadFormPayment').show();
        }
    });

    $('#typePayment').on('mouseover', '*', function () {
        $(this).css('background-color', '#c5a25d').css('color', '#fff');
    }).on('mouseout', '*', function () {
        $(this).css('background-color', '').css('color', '');
    });

    $("#checkOutForm").on('submit', function (e) {
        e.preventDefault();
        $('.success-line').show();
        $.ajax({
            type: this.method,
            url: this.action,
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    window.location.href = response.redirectUrl;
                } else {
                    $('.success-line').hide();
                    alert("Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.");
                }
            },
            error: function () {
                $('.success-line').hide();
                alert("Có lỗi xảy ra khi gửi yêu cầu.");
            }
        });
    });

    // Xử lý nút tăng giảm số lượng ở trang chi tiết sản phẩm
    $('.quantity-selector .plus').on('click', function () {
        var currentQuantity = parseInt($('#quantity').text());
        var maxQuantity = parseInt($(this).data('max')) || 100;

        if (currentQuantity < maxQuantity) {
            $('#quantity').text(currentQuantity + 1);
            if ($('#product-quantity').length) {
                $('#product-quantity').val(currentQuantity + 1);
            }
        }
    });

    $('.quantity-selector .minus').on('click', function () {
        var currentQuantity = parseInt($('#quantity').text());

        if (currentQuantity > 1) {
            $('#quantity').text(currentQuantity - 1);
            if ($('#product-quantity').length) {
                $('#product-quantity').val(currentQuantity - 1);
            }
        }
    });
    
});

function getUserInfo() {
    $.ajax({
        url: '/Account/GetUserInfo',
        type: 'GET',
        success: function (data) {
            if (data) {
                $('#CustomerName').val(data.FullName);
                $('#Phone').val(data.Phone);
                $('#Address').val(data.Address);
                $('#Email').val(data.Email);
            }
        },
        error: function () {
            console.log('Không thể lấy thông tin người dùng.');
        }
    });
}

function updateTotalPrice() {
    var total = 0;
    $('.cbkItem:checked').each(function () {
        var price = parseFloat($(this).attr('data-price')) || 0;
        total += price;
    });
    $('#totalPrice').html(total.toLocaleString('vi-VN') + '<u>đ</u>');
}

function updateSTT() {
    $('.row_index').each(function (index) {
        $(this).text(index + 1);
    });
}

function loadCart() {
    $.ajax({
        url: '/ShoppingCart/ItemCart',
        type: 'POST',
        success: function (result) {
            $('#loadItemCart').html(result);
        }
    });
}

function failure(result) {
    if (!result.success) {
        $("#checkOutReturn").html("Bạn mua hàng thất bại! Xin vui lòng thử lại!");
    }
}

function addToCart(id, quantity) {
    $.ajax({
        url: '/ShoppingCart/AddToCart',
        type: 'POST',
        data: { id: id, quantity: quantity },
        success: function (result) {
            if (result.redirectToLogin) {
                window.location.href = result.redirectToLogin;
            } else if (result.success) {
                $('#cartItemCount').html(result.count);
                alert("Thêm sản phẩm thành công");
            } else {
                alert("Đã có lỗi xảy ra.");
            }
        }
    });
}

// Hàm xử lý chức năng "Mua ngay"
function buyNow(id, quantity) {
    $.ajax({
        url: '/ShoppingCart/BuyNow',
        type: 'POST',
        data: { id: id, quantity: quantity },
        success: function (result) {
            if (result.redirectToLogin) {
                window.location.href = result.redirectToLogin;
            } else if (result.success) {
                window.location.href = result.redirectUrl;
            } else {
                if (result.message) {
                    alert(result.message);
                } else {
                    alert("Đã có lỗi xảy ra khi mua sản phẩm.");
                }
            }
        },
        error: function () {
            alert("Đã có lỗi xảy ra khi gửi yêu cầu.");
        }
    });
}