// Add this at the top of your file to help with debugging
function debugLog(message, data) {
    console.log(message, data);
    if (data && data.length === 0) {
        console.warn("Empty data array received");
    }
}

$(function () {
    // Your existing date handling code...
    $('#fromDate').change(function () {
        var fromDate = new Date($('#fromDate').val());
        var toDate = new Date($('#toDate').val());

        if (fromDate > toDate) {
            $('#toDate').val($('#fromDate').val());
        }
    });

    $('#toDate').change(function () {
        var fromDate = new Date($('#fromDate').val());
        var toDate = new Date($('#toDate').val());

        if (toDate < fromDate) {
            $('#fromDate').val($('#toDate').val());
        }
    });

    $('#btnSearch').click(function () {
        var fromDate = $('#fromDate').val();
        var toDate = $('#toDate').val();
        var viewMode = $('#viewMode').val();
        loadStatisticalData(fromDate, toDate, viewMode);
    });

    // Load default data for last 15 days
    var defaultFromDate = moment().subtract(14, 'days').format('YYYY-MM-DD');
    var defaultToDate = moment().format('YYYY-MM-DD');
    $('#fromDate').val(defaultFromDate);
    $('#toDate').val(defaultToDate);
    $('#viewMode').val('day');
    loadStatisticalData(defaultFromDate, defaultToDate, 'day');

    // Load product statistics
    loadProductStatistics();

    // Add a slight delay before loading top/bottom products to ensure DOM is ready
    setTimeout(function () {
        console.log("Loading top and bottom products...");
        // Test if canvas elements exist
        console.log("topProductsChart exists:", $('#topProductsChart').length > 0);
        console.log("bottomProductsChart exists:", $('#bottomProductsChart').length > 0);

        loadTopProducts();
        loadBottomProducts();
    }, 500);

    // Sorting handlers
    $('.sortable').click(function () {
        var sortField = $(this).data('sort-field');
        var currentSortOrder = $(this).data('sort-order') || 'desc';
        var newSortOrder = currentSortOrder === 'desc' ? 'asc' : 'desc';
        $(this).data('sort-order', newSortOrder);

        // Reset icons
        $('.sortable i').removeClass('fa-caret-up fa-caret-down').addClass('fa-sort');

        // Update icon
        if (newSortOrder === 'asc') {
            $(this).find('i').removeClass('fa-sort fa-caret-down').addClass('fas fa-caret-up');
        } else {
            $(this).find('i').removeClass('fa-sort fa-caret-up').addClass('fas fa-caret-down');
        }

        loadProductStatistics(sortField, newSortOrder);
    });
});

// Your existing revenue chart code...
var barChart;
function loadStatisticalData(fromDate, toDate, viewMode) {
    var arrRevenue = [];
    var arrProfit = [];
    var arrDate = [];

    $.ajax({
        url: '/Admin/Statistical/GetStatistical',
        type: 'GET',
        data: { fromDate: fromDate, toDate: toDate, viewMode: viewMode },
        success: function (result) {
            var isMonthView = viewMode === 'month';
            var isYearView = viewMode === 'year';

            $.each(result.Data, function (i, item) {
                var strDate = isMonthView ? moment(item.Date).format('MM/YYYY')
                    : isYearView ? moment(item.Date).format('YYYY')
                        : moment(item.Date).format('DD/MM/YYYY');
                arrDate.push(strDate);
                arrRevenue.push(item.Revenue);
                arrProfit.push(item.Profit);
            });

            var areaChartData = {
                labels: arrDate,
                datasets: [
                    {
                        label: '', // Lợi nhuận
                        //backgroundColor: 'rgba(60,141,188,0.9)',
                        backgroundColor: '#fff',
                        borderColor: 'rgba(60,141,188,0.8)',
                        pointRadius: false,
                        pointColor: '#3b8bba',
                        pointStrokeColor: 'rgba(60,141,188,1)',
                        pointHighlightFill: '#fff',
                        pointHighlightStroke: 'rgba(60,141,188,1)',
                        data: arrProfit,
                        hidden: true // Ẩn cột lợi nhuận
                    },
                    {
                        label: '', // Doanh thu
                        backgroundColor: 'rgba(210, 214, 222, 1)',
                        //backgroundColor: '#fff',
                        borderColor: 'rgba(210, 214, 222, 1)',
                        pointRadius: false,
                        pointColor: 'rgba(210, 214, 222, 1)',
                        pointStrokeColor: '#c1c7d1',
                        pointHighlightFill: '#fff',
                        pointHighlightStroke: 'rgba(220,220,220,1)',
                        data: arrRevenue
                    },
                ]
            };

            if (barChart) {
                barChart.destroy();
            }

            var barChartCanvas = $('#barChart').get(0).getContext('2d');
            var barChartData = $.extend(true, {}, areaChartData);
            var temp0 = areaChartData.datasets[0];
            var temp1 = areaChartData.datasets[1];
            barChartData.datasets[0] = temp1;
            barChartData.datasets[1] = temp0;

            var barChartOptions = {
                responsive: true,
                maintainAspectRatio: false,
                datasetFill: false,
                tooltips: {
                    callbacks: {
                        label: function (tooltipItem, data) {
                            var dataset = data.datasets[tooltipItem.datasetIndex];
                            var value = dataset.data[tooltipItem.index];
                            var formattedValue = value.toLocaleString('vi-VN') + 'đ';
                            //return dataset.label + ': ' + formattedValue;
                            return formattedValue;
                        }
                    }
                },
                legend: {
                    display: false // Ẩn nhãn của cả hai cột
                }
            };

            barChart = new Chart(barChartCanvas, {
                type: 'bar',
                data: barChartData,
                options: barChartOptions
            });

            loadRevenue(result.Data, viewMode);
        }
    });
}

function loadRevenue(data, viewMode) {
    var strHtml = '';
    $('#headerDate').text(viewMode === 'month' ? 'Tháng' : viewMode === 'year' ? 'Năm' : 'Ngày');
    $.each(data, function (i, item) {
        var strDate = viewMode === 'month' ? moment(item.Date).format('MM/YYYY')
            : viewMode === 'year' ? moment(item.Date).format('YYYY')
                : moment(item.Date).format('DD/MM/YYYY');
        var formattedRevenue = item.Revenue.toLocaleString('vi-VN') + "<u>đ</u>";
        //var formattedProfit = item.Profit.toLocaleString('vi-VN') + "<u>đ</u>";
        strHtml += "<tr>";
        strHtml += "<td>" + (i + 1) + "</td>";
        strHtml += "<td>" + strDate + "</td>";
        strHtml += "<td>" + formattedRevenue + "</td>";
        //strHtml += "<td>" + formattedProfit + "</td>";
        strHtml += "</tr>";
    });
    $('#loadRevenue').html(strHtml);
}

function loadProductStatistics(sortField = 'soldQuantity', sortOrder = 'desc') {
    $.ajax({
        url: '/Admin/Statistical/GetProductStatistics',
        type: 'GET',
        data: { sortField: sortField, sortOrder: sortOrder },
        success: function (result) {
            var strHtml = '';
            $.each(result.Data, function (index, item) {
                // Chuyển đổi ExpiredDate từ /Date(<timestamp>)/ sang đối tượng Date
                var expiredDate = new Date(parseInt(item.ExpiredDate.replace("/Date(", "").replace(")/", "")));

                // Định dạng lại ExpiredDate thành "dd//MM/yyyy"
                var formattedDate = expiredDate.getDate().toString().padStart(2, '0') + '/' +
                    (expiredDate.getMonth() + 1).toString().padStart(2, '0') + '/' +
                    expiredDate.getFullYear();

                strHtml += '<tr>';
                strHtml += '<td>' + (index + 1) + '</td>';
                strHtml += '<td><img src="' + item.ProductImage + '" style="width: 50px; height: 50px;" /></td>';
                strHtml += '<td>' + item.ProductName + '</td>';
                strHtml += '<td>' + item.SoldQuantity + '</td>';
                strHtml += '<td>' + item.RemainingQuantity + '</td>';
                strHtml += '<td>' + formattedDate + '</td>';
                strHtml += '</tr>';
            });
            $('#loadProductStatistic').html(strHtml);
        }
    });
}

// Global variables for charts
let topProductsChart = null;
let bottomProductsChart = null;

function loadTopProducts() {
    console.log("Fetching top products data...");
    $.ajax({
        url: '/Admin/Statistical/GetTopProducts',
        type: 'GET',
        data: { isTop: true, count: 5 },
        dataType: 'json',
        success: function (response) {
            console.log("Top products data received:", response);
            if (response && response.Data) {
                debugLog("Top products data:", response.Data);
                renderTopProductsTable(response.Data);

                // Only try to render chart if we have data
                if (response.Data.length > 0) {
                    renderTopProductsChart(response.Data);
                } else {
                    console.warn("No data available for top products chart");
                    $('#loadTopProducts').html('<tr><td colspan="4" class="text-center">Không có dữ liệu</td></tr>');
                }
            } else {
                console.error('No data received for top products');
                $('#loadTopProducts').html('<tr><td colspan="4" class="text-center">Không có dữ liệu</td></tr>');
            }
        },
        error: function (err) {
            console.error("Error fetching top products:", err);
            alert('Có lỗi xảy ra khi tải dữ liệu sản phẩm bán chạy');
        }
    });
}

function loadBottomProducts() {
    console.log("Fetching bottom products data...");
    $.ajax({
        url: '/Admin/Statistical/GetTopProducts',
        type: 'GET',
        data: { isTop: false, count: 5 },
        dataType: 'json',
        success: function (response) {
            console.log("Bottom products data received:", response);
            if (response && response.Data) {
                debugLog("Bottom products data:", response.Data);
                renderBottomProductsTable(response.Data);

                // Only try to render chart if we have data
                if (response.Data.length > 0) {
                    renderBottomProductsChart(response.Data);
                } else {
                    console.warn("No data available for bottom products chart");
                    $('#loadBottomProducts').html('<tr><td colspan="4" class="text-center">Không có dữ liệu</td></tr>');
                }
            } else {
                console.error('No data received for bottom products');
                $('#loadBottomProducts').html('<tr><td colspan="4" class="text-center">Không có dữ liệu</td></tr>');
            }
        },
        error: function (err) {
            console.error("Error fetching bottom products:", err);
            alert('Có lỗi xảy ra khi tải dữ liệu sản phẩm bán chậm');
        }
    });
}

function renderTopProductsTable(data) {
    console.log("Rendering top products table with data:", data);
    var html = '';
    if (data.length === 0) {
        html = '<tr><td colspan="4" class="text-center">Không có dữ liệu</td></tr>';
    } else {
        $.each(data, function (index, item) {
            html += '<tr>';
            html += '<td>' + (index + 1) + '</td>';
            html += '<td><img src="' + item.ProductImage + '" style="width:50px; height:50px;" /></td>';
            html += '<td>' + item.ProductName + '</td>';
            html += '<td>' + item.SoldQuantity + '</td>';
            html += '</tr>';
        });
    }
    $('#loadTopProducts').html(html);
}

function renderBottomProductsTable(data) {
    console.log("Rendering bottom products table with data:", data);
    var html = '';
    if (data.length === 0) {
        html = '<tr><td colspan="4" class="text-center">Không có dữ liệu</td></tr>';
    } else {
        $.each(data, function (index, item) {
            html += '<tr>';
            html += '<td>' + (index + 1) + '</td>';
            html += '<td><img src="' + item.ProductImage + '" style="width:50px; height:50px;" /></td>';
            html += '<td>' + item.ProductName + '</td>';
            html += '<td>' + item.SoldQuantity + '</td>';
            html += '</tr>';
        });
    }
    $('#loadBottomProducts').html(html);
}

function renderTopProductsChart(data) {
    console.log("Rendering top products chart...");
    try {
        var canvas = document.getElementById('topProductsChart');
        if (!canvas) {
            console.error('Canvas element topProductsChart not found');
            return;
        }

        var ctx = canvas.getContext('2d');
        if (!ctx) {
            console.error('Could not get 2d context for topProductsChart');
            return;
        }

        // Destroy existing chart if it exists
        if (topProductsChart) {
            topProductsChart.destroy();
        }

        // Prepare data for chart
        var labels = data.map(item => item.ProductName);
        var values = data.map(item => item.SoldQuantity);

        console.log("Chart data - labels:", labels, "values:", values);

        // Generate colors
        var backgroundColors = generateRandomColors(data.length);

        // Create a simpler chart first to test
        topProductsChart = new Chart(ctx, {
            type: 'bar', // Changed to bar for simplicity
            data: {
                labels: labels,
                datasets: [{
                    label: 'Số lượng bán',
                    data: values,
                    backgroundColor: backgroundColors,
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false
            }
        });

        console.log("Top products chart rendered successfully");
    } catch (error) {
        console.error("Error rendering top products chart:", error);
    }
}

function renderBottomProductsChart(data) {
    console.log("Rendering bottom products chart...");
    try {
        var canvas = document.getElementById('bottomProductsChart');
        if (!canvas) {
            console.error('Canvas element bottomProductsChart not found');
            return;
        }

        var ctx = canvas.getContext('2d');
        if (!ctx) {
            console.error('Could not get 2d context for bottomProductsChart');
            return;
        }

        // Destroy existing chart if it exists
        if (bottomProductsChart) {
            bottomProductsChart.destroy();
        }

        // Prepare data for chart
        var labels = data.map(item => item.ProductName);
        var values = data.map(item => item.SoldQuantity);

        console.log("Chart data - labels:", labels, "values:", values);

        // Generate colors
        var backgroundColors = generateRandomColors(data.length);

        // Create a simpler chart first to test
        bottomProductsChart = new Chart(ctx, {
            type: 'bar', // Changed to bar for simplicity
            data: {
                labels: labels,
                datasets: [{
                    label: 'Số lượng bán',
                    data: values,
                    backgroundColor: backgroundColors,
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false
            }
        });

        console.log("Bottom products chart rendered successfully");
    } catch (error) {
        console.error("Error rendering bottom products chart:", error);
    }
}

// Helper function to generate random colors for charts
function generateRandomColors(count) {
    var colors = [];
    var predefinedColors = [
        'rgba(255, 99, 132, 0.7)',
        'rgba(54, 162, 235, 0.7)',
        'rgba(255, 206, 86, 0.7)',
        'rgba(75, 192, 192, 0.7)',
        'rgba(153, 102, 255, 0.7)',
        'rgba(255, 159, 64, 0.7)',
        'rgba(199, 199, 199, 0.7)',
        'rgba(83, 102, 255, 0.7)',
        'rgba(40, 159, 64, 0.7)',
        'rgba(210, 199, 199, 0.7)'
    ];

    for (var i = 0; i < count; i++) {
        colors.push(predefinedColors[i % predefinedColors.length]);
    }

    return colors;
}

