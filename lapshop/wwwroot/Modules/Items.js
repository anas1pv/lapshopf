var ClsItems = {
    currentSearch: "",
    currentCategoryId: 0,
    GetAll: function (search, categoryId) {
        ClsItems.currentSearch = search || "";
        
        var catId = parseInt(categoryId) || 0;
        if (catId > 0) {
            var checkbox = $('.brand-filter[data-val="' + catId + '"]');
            if (checkbox.length) {
                checkbox.prop('checked', true);
                ClsItems.currentCategoryId = 0; // Checkboxes handle the brand filtering now
            } else {
                ClsItems.currentCategoryId = catId; // Fallback if no matching checkbox
            }
        } else {
            ClsItems.currentCategoryId = 0;
        }

        // Initialize Pagination using AJAX dataSource
        ClsItems.InitPagination();

        // Attach event handlers to checkboxes to trigger refresh
        $('.filter-checkbox').off('change').on('change', function() {
            ClsItems.RefreshPagination();
        });

        // Attach event handlers to page-size and sort-order dropdowns to trigger refresh
        $('#select-page-size, #select-sort-order').off('change').on('change', function() {
            ClsItems.RefreshPagination();
        });

        // Intercept search form submit if on product list page
        var searchForm = $('#search-overlay form');
        if (searchForm.length) {
            searchForm.off('submit').on('submit', function(e) {
                e.preventDefault();
                var searchVal = $(this).find('input[name="search"]').val();
                ClsItems.currentSearch = searchVal || "";
                if (typeof closeSearch === 'function') {
                    closeSearch();
                }
                ClsItems.RefreshPagination();
            });
        }
    },
    InitPagination: function() {
        // Collect active filters to send with AJAX
        var selectedBrands = [];
        $('.brand-filter:checked').each(function() {
            selectedBrands.push($(this).data('val'));
        });

        var selectedRam = [];
        $('.ram-filter:checked').each(function() {
            selectedRam.push($(this).data('val'));
        });

        var selectedProcessors = [];
        $('.processor-filter:checked').each(function() {
            selectedProcessors.push($(this).data('val'));
        });

        var selectedOs = [];
        $('.os-filter:checked').each(function() {
            selectedOs.push($(this).data('val'));
        });

        var priceSlider = $('#price-slider');
        var minPrice = 0;
        var maxPrice = 0;
        if (priceSlider.length && priceSlider.hasClass('ui-slider')) {
            minPrice = priceSlider.slider('values', 0);
            maxPrice = priceSlider.slider('values', 1);
        }

        $('#ItemPagination').pagination({
            dataSource: '/api/Items/GetPaged',
            locator: 'data.items',
            totalNumberLocator: function(response) {
                if (response && response.data) {
                    return response.data.totalCount;
                }
                return 0;
            },
            pageSize: parseInt($('#select-page-size').val()) || 24,
            alias: {
                pageNumber: 'page',
                pageSize: 'pageSize'
            },
            ajax: {
                data: {
                    categoryId: ClsItems.currentCategoryId,
                    search: ClsItems.currentSearch,
                    brands: selectedBrands.join(','),
                    ramSizes: selectedRam.join(','),
                    processors: selectedProcessors.join(','),
                    osIds: selectedOs.join(','),
                    minPrice: minPrice,
                    maxPrice: maxPrice,
                    sortOrder: $('#select-sort-order').val() || "default"
                },
                beforeSend: function() {
                    $('#ItemArea').html('<div class="col-12 text-center" style="padding: 80px 0;"><div class="spinner-border text-primary" role="status" style="border-color: #00f3ff; border-right-color: transparent;"><span class="sr-only">Loading...</span></div></div>');
                }
            },
            callback: function (data, pagination) {
                // Update showing text
                var pageNum = pagination.pageNumber;
                var pageSize = pagination.pageSize;
                var totalCount = pagination.totalNumber;
                var start = totalCount > 0 ? (pageNum - 1) * pageSize + 1 : 0;
                var end = Math.min(pageNum * pageSize, totalCount);
                
                var resultsText = "Showing Products " + start + "-" + end + " of " + totalCount + " Results";
                if (ClsItems.currentSearch) {
                    resultsText += " for \"" + ClsItems.currentSearch + "\"";
                }
                $('.search-count h5').text(resultsText);

                var htmlData = "";
                if (data && data.length > 0) {
                    for (var i = 0; i < data.length; i++) {
                        htmlData += ClsItems.DrawItem(data[i]);
                    }
                } else {
                    htmlData = '<div class="col-12 text-center" style="padding: 80px 20px;"><i class="ti-search" style="font-size: 48px; color: #64748B; display:block; margin-bottom:15px;"></i><h5 style="color:#fff;">No laptops found matching your filters</h5></div>';
                }
                
                var d1 = document.getElementById('ItemArea');
                if (d1) {
                    d1.innerHTML = htmlData;

                    // Sync Wishlist icons state for newly drawn items
                    if (typeof WishlistHelper !== 'undefined') {
                        WishlistHelper.updateHeartIcons();
                    }
                }
            }
        });
    },
    RefreshPagination: function() {
        ClsItems.InitPagination();
    },
    DrawItem: function (item) {
        var data = "<div class='col-xl-3 col-6 col-grid-box'>";
        data += "<div class='product-box'>";
        var img = item.imageName || 'silver_ultrabook.png';
        data += "<div class='img-wrapper' style='border-radius: 12px; overflow: hidden; background: rgba(0, 0, 0, 0.2) !important; display: flex; align-items: center; justify-content: center; height: 180px; position: relative;'>";
        data += "<div class='front'><a href='/Items/ItemDetails/" + item.itemId + "'><img src='/Uploads/Items/" + img + "' class='img-fluid' alt='' style='max-height: 100%; width: auto; object-fit: contain;'></a></div>";
        
        var escapedName = item.itemName.replace(/'/g, "\\'");
        data += "<div class='cart-info cart-wrap'>";
        data += "<a href='/Items/ItemDetails/" + item.itemId + "' title='View details'><i class='ti-search' aria-hidden='true'></i></a>";
        data += "<a href='javascript:void(0)' class='wishlist-btn' data-item-id='" + item.itemId + "' onclick=\"toggleWishlist(" + item.itemId + ", '" + escapedName + "', " + item.salesPrice + ", '" + img + "', '" + (item.processor || 'Core i7') + "', " + (item.ramSize || 16) + ")\" title='Add to Wishlist'><i class='fa fa-heart-o' aria-hidden='true' style='color: #ef4444;'></i></a>";
        data += "</div>";
        data += "</div>";
        
        var avg = item.averageRating || 4.8;
        
        data += "<div class='product-detail' style='padding-top: 14px; text-align: left;'>";
        data += "<div style='display: flex; justify-content: space-between; align-items: center; margin-bottom: 4px;'>";
        data += "<a href='/Items/ItemDetails/" + item.itemId + "' style='flex: 1; min-width: 0; padding-right: 8px;'>";
        data += "<h6 style='margin: 0; font-size: 15px; font-weight: 700; color: #ffffff; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; font-family: \"Outfit\", sans-serif !important;'>" + item.itemName + "</h6>";
        data += "</a>";
        data += "<span style='color: #ff9f0a; font-size: 12px; font-weight: 600; white-space: nowrap; font-family: \"Inter\", sans-serif !important;'>★ " + avg.toFixed(1) + "/5</span>";
        data += "</div>";
        data += "<div style='font-size: 12px; color: #64748b; margin-bottom: 12px; font-family: \"Inter\", sans-serif !important; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;' title='" + (item.processor || 'Core i7') + " | " + (item.ramSize || 16) + "GB RAM'>";
        data += (item.processor || 'Core i7') + " | " + (item.ramSize || 16) + "GB RAM";
        data += "</div>";
        data += "<div class='product-card-footer'>";
        if (item.discountPrice && item.discountPrice > 0) {
            data += "<h4 class='product-card-price' style='display: flex; gap: 8px; align-items: center; flex-wrap: wrap; margin: 0;'>";
            data += "<del style='color: #64748b; font-size: 12px; font-weight: 400; font-family: \"Inter\", sans-serif !important;'>$" + parseFloat(item.salesPrice).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + "</del>";
            data += "<span style='color: #00f3ff; font-weight: 800; font-family: \"Inter\", sans-serif !important;'>$" + parseFloat(item.discountPrice).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + "</span>";
            data += "</h4>";
        } else {
            data += "<h4 class='product-card-price'>$" + parseFloat(item.salesPrice).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + "</h4>";
        }
        data += "<a href='javascript:void(0)' onclick='ClsItems.AddToCart(" + item.itemId + ", this)' class='product-card-btn'>ADD TO CART</a>";
        data += "</div>";
        data += "</div></div></div>";
        return data;
    },
    AddToCart: function(itemId, btn) {
        if (typeof AddToCartAjax === 'function') {
            AddToCartAjax(itemId, btn);
        } else {
            Helper.AjaxCallGet("/Order/AddToCartAjax?itemId=" + itemId, {}, "json", function(response) {
                if (response && response.success) {
                    // Update badge in header
                    var badge = $('#cartCountBadge');
                    if (badge.length) {
                        badge.text(response.cartCount);
                        badge.show();
                    }
                    NotificationHelper.ShowSuccess("Laptop added to cart successfully!");
                } else {
                    NotificationHelper.ShowError("Could not add item to cart.");
                }
            }, function() {
                NotificationHelper.ShowError("Connection error while adding to cart.");
            });
        }
    }
}
