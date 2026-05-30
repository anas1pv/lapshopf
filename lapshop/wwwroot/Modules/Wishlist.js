var WishlistHelper = {
    storageKey: 'lapshop_wishlist',

    getAll: function () {
        var data = localStorage.getItem(this.storageKey);
        return data ? JSON.parse(data) : [];
    },

    save: function (items) {
        localStorage.setItem(this.storageKey, JSON.stringify(items));
        this.updateBadge();
    },

    add: function (item) {
        var items = this.getAll();
        // Check if already exists
        var exists = items.find(function (i) { return i.itemId === item.itemId; });
        if (exists) {
            NotificationHelper.ShowError('This item is already in your wishlist!');
            return false;
        }
        items.push({
            itemId: item.itemId,
            itemName: item.itemName,
            salesPrice: item.salesPrice,
            imageName: item.imageName,
            processor: item.processor || 'N/A',
            ramSize: item.ramSize || 0,
            addedDate: new Date().toISOString()
        });
        this.save(items);
        NotificationHelper.ShowSuccess('Added to wishlist! ❤️');
        return true;
    },

    remove: function (itemId) {
        var items = this.getAll();
        items = items.filter(function (i) { return i.itemId !== itemId; });
        this.save(items);
    },

    toggle: function (itemId, itemData) {
        var items = this.getAll();
        var exists = items.find(function (i) { return i.itemId === itemId; });
        if (exists) {
            this.remove(itemId);
            NotificationHelper.ShowSuccess('Removed from wishlist');
            return false;
        } else {
            return this.add(itemData);
        }
    },

    isInWishlist: function (itemId) {
        var items = this.getAll();
        return items.some(function (i) { return i.itemId === itemId; });
    },

    getCount: function () {
        return this.getAll().length;
    },

    updateBadge: function () {
        var count = this.getCount();
        var badge = $('#wishlistCountBadge');
        if (badge.length) {
            badge.text(count);
            if (count > 0) {
                badge.show();
            } else {
                badge.hide();
            }
        }
    },

    updateHeartIcons: function () {
        var items = this.getAll();
        var ids = items.map(function (i) { return i.itemId; });
        $('.wishlist-btn').each(function () {
            var id = parseInt($(this).data('item-id'));
            if (ids.indexOf(id) !== -1) {
                $(this).find('i').removeClass('fa-heart-o').addClass('fa-heart');
                $(this).addClass('wishlisted');
            } else {
                $(this).find('i').removeClass('fa-heart').addClass('fa-heart-o');
                $(this).removeClass('wishlisted');
            }
        });
    },

    moveToCart: function (itemId) {
        if (typeof AddToCartAjax === 'function') {
            AddToCartAjax(itemId, null);
        } else {
            window.location.href = '/Order/AddToCart?itemId=' + itemId;
        }
        this.remove(itemId);
    }
};

// Initialize badge on page load
$(document).ready(function () {
    WishlistHelper.updateBadge();
    WishlistHelper.updateHeartIcons();
});

// Global toggle helper for product card click
function toggleWishlist(itemId, itemName, salesPrice, imageName, processor, ramSize) {
    var itemData = {
        itemId: itemId,
        itemName: itemName,
        salesPrice: salesPrice,
        imageName: imageName,
        processor: processor,
        ramSize: ramSize
    };
    WishlistHelper.toggle(itemId, itemData);
    WishlistHelper.updateHeartIcons();
}

