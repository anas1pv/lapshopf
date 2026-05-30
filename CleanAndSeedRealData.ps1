# CleanAndSeedRealData.ps1
# Script to clear all database catalog tables, reset identities, and seed premium, real-world laptop data, images, settings, sliders, evaluations, and mock orders.

$connString = "Server=localhost;Database=LapShop;Integrated Security=True;TrustServerCertificate=True"

# Directories for uploads
$projectDir = "C:\Users\anasa\source\repos\lapshop\lapshop"
$itemsUploadDir = "$projectDir\wwwroot\Uploads\Items"
$categoriesUploadDir = "$projectDir\wwwroot\Uploads\Categories"
$slidersUploadDir = "$projectDir\wwwroot\Uploads\Sliders"

# Ensure directories exist
if (-not (Test-Path $itemsUploadDir)) { New-Item -ItemType Directory -Path $itemsUploadDir -Force }
if (-not (Test-Path $categoriesUploadDir)) { New-Item -ItemType Directory -Path $categoriesUploadDir -Force }
if (-not (Test-Path $slidersUploadDir)) { New-Item -ItemType Directory -Path $slidersUploadDir -Force }

# Source image files from brain folder
$brainDir = "C:\Users\anasa\.gemini\antigravity\brain\afdf9e4c-085b-4c02-9045-5d3347752d22"
$sourceApple = "$brainDir\apple_macbook_1779826382060.png"
$sourceGaming = "$brainDir\gaming_laptop_1779826399525.png"
$sourceSilver = "$brainDir\silver_ultrabook_1779826416164.png"
$sourceDark = "$brainDir\dark_laptop_1779826433561.png"
$sourceSlider = "$brainDir\slider_banner_1779826449245.png"
$sourceDell = "$brainDir\dell_xps_1780018652726.png"
$sourceLenovo = "$brainDir\lenovo_thinkpad_1780018669520.png"
$sourceRazer = "$brainDir\razer_blade_1780018686295.png"

# Copy image assets to uploads directories
Write-Host "Copying premium laptop image assets to uploads folders..."
Copy-Item $sourceApple -Destination "$itemsUploadDir\apple_macbook.png" -Force
Copy-Item $sourceGaming -Destination "$itemsUploadDir\gaming_laptop.png" -Force
Copy-Item $sourceSilver -Destination "$itemsUploadDir\silver_ultrabook.png" -Force
Copy-Item $sourceDark -Destination "$itemsUploadDir\dark_laptop.png" -Force
Copy-Item $sourceDell -Destination "$itemsUploadDir\dell_xps.png" -Force
Copy-Item $sourceLenovo -Destination "$itemsUploadDir\lenovo_thinkpad.png" -Force
Copy-Item $sourceRazer -Destination "$itemsUploadDir\razer_blade.png" -Force

# Category Images (reuse laptop images)
Copy-Item "$itemsUploadDir\apple_macbook.png" -Destination "$categoriesUploadDir\apple_macbook.png" -Force
Copy-Item "$itemsUploadDir\dell_xps.png" -Destination "$categoriesUploadDir\dell_xps.png" -Force
Copy-Item "$itemsUploadDir\lenovo_thinkpad.png" -Destination "$categoriesUploadDir\lenovo_thinkpad.png" -Force
Copy-Item "$itemsUploadDir\silver_ultrabook.png" -Destination "$categoriesUploadDir\silver_ultrabook.png" -Force
Copy-Item "$itemsUploadDir\gaming_laptop.png" -Destination "$categoriesUploadDir\gaming_laptop.png" -Force
Copy-Item "$itemsUploadDir\razer_blade.png" -Destination "$categoriesUploadDir\razer_blade.png" -Force

# Slider
Copy-Item $sourceSlider -Destination "$slidersUploadDir\slider1.png" -Force

Write-Host "Images copied successfully!"

# SQL Database Truncation & Seeding
$connection = New-Object System.Data.SqlClient.SqlConnection($connString)

try {
    $connection.Open()
    Write-Host "Connected to SQL Server. Truncating existing tables..."
    
    # Run SQL script to truncate tables
    $truncateSql = @"
DELETE FROM TbItemEvaluations;
DELETE FROM TbSalesInvoiceItems;
DELETE FROM TbSalesInvoices;
DELETE FROM TbItemImages;
DELETE FROM TbItemDiscount;
DELETE FROM TbCustomerItems;
DELETE FROM TbItems;
DELETE FROM TbCategories;
DELETE FROM TbItemTypes;
DELETE FROM TbOs;
DELETE FROM TbSlider;
DELETE FROM TbCoupons;
DELETE FROM TbPages;

DBCC CHECKIDENT ('TbItemEvaluations', RESEED, 0);
DBCC CHECKIDENT ('TbSalesInvoiceItems', RESEED, 0);
DBCC CHECKIDENT ('TbSalesInvoices', RESEED, 0);
DBCC CHECKIDENT ('TbItemImages', RESEED, 0);
DBCC CHECKIDENT ('TbItemDiscount', RESEED, 0);
DBCC CHECKIDENT ('TbItems', RESEED, 0);
DBCC CHECKIDENT ('TbCategories', RESEED, 0);
DBCC CHECKIDENT ('TbItemTypes', RESEED, 0);
DBCC CHECKIDENT ('TbOs', RESEED, 0);
DBCC CHECKIDENT ('TbSlider', RESEED, 0);
DBCC CHECKIDENT ('TbCoupons', RESEED, 0);
DBCC CHECKIDENT ('TbPages', RESEED, 0);
"@
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = $truncateSql
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "Database catalog tables cleared and identities reseeded."

    # Seed OS
    $seedOsSql = @"
SET IDENTITY_INSERT TbOs ON;
INSERT INTO TbOs (OsId, OsName, ImageName, ShowInHomePage, CurrentState, CreatedDate, CreatedBy) VALUES
(1, 'Windows 11', 'win11.png', 1, 1, GETDATE(), 'Admin'),
(2, 'Windows 10', 'win10.png', 1, 1, GETDATE(), 'Admin'),
(3, 'macOS', 'macos.png', 1, 1, GETDATE(), 'Admin'),
(4, 'Linux', 'linux.png', 0, 1, GETDATE(), 'Admin'),
(5, 'ChromeOS', 'chrome.png', 0, 1, GETDATE(), 'Admin');
SET IDENTITY_INSERT TbOs OFF;
"@
    $cmd.CommandText = $seedOsSql
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "Seeded Operating Systems (TbOs)."

    # Seed Item Types
    $seedTypesSql = @"
SET IDENTITY_INSERT TbItemTypes ON;
INSERT INTO TbItemTypes (ItemTypeId, ItemTypeName, ImageName, CurrentState, CreatedDate, CreatedBy) VALUES
(1, 'Ultrabook', 'ultrabook.png', 1, GETDATE(), 'Admin'),
(2, 'Gaming', 'gaming.png', 1, GETDATE(), 'Admin'),
(3, 'Business', 'business.png', 1, GETDATE(), 'Admin'),
(4, 'Workstation', 'workstation.png', 1, GETDATE(), 'Admin');
SET IDENTITY_INSERT TbItemTypes OFF;
"@
    $cmd.CommandText = $seedTypesSql
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "Seeded Item Types (TbItemTypes)."

    # Seed Categories
    $seedCatsSql = @"
SET IDENTITY_INSERT TbCategories ON;
INSERT INTO TbCategories (CategoryId, CategoryName, ImageName, CurrentState, CreatedDate, CreatedBy) VALUES
(1, 'Apple', 'apple_macbook.png', 1, GETDATE(), 'Admin'),
(2, 'Dell', 'dell_xps.png', 1, GETDATE(), 'Admin'),
(3, 'Lenovo', 'lenovo_thinkpad.png', 1, GETDATE(), 'Admin'),
(4, 'HP', 'silver_ultrabook.png', 1, GETDATE(), 'Admin'),
(5, 'Asus', 'gaming_laptop.png', 1, GETDATE(), 'Admin'),
(6, 'Razer', 'razer_blade.png', 1, GETDATE(), 'Admin'),
(7, 'MSI', 'gaming_laptop.png', 1, GETDATE(), 'Admin');
SET IDENTITY_INSERT TbCategories OFF;
"@
    $cmd.CommandText = $seedCatsSql
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "Seeded Brands (TbCategories)."

    # Seed Items
    $seedItemsSql = @"
SET IDENTITY_INSERT TbItems ON;
INSERT INTO TbItems (ItemId, ItemName, SalesPrice, PurchasePrice, CategoryId, ImageName, CurrentState, CreatedDate, CreatedBy, Description, Gpu, HardDisk, ItemTypeId, Processor, RamSize, ScreenReslution, ScreenSize, Weight, OsId) VALUES
(1, 'Apple MacBook Pro 16\" (M3 Max)', 3499.00, 2800.00, 1, 'apple_macbook.png', 1, GETDATE(), 'Admin', 
'The Apple MacBook Pro 16 is the ultimate mobile workstation for power users. Featuring the groundbreaking Apple M3 Max chip with a 16-core CPU and 40-core GPU, it delivers desktop-class performance in a portable form factor. Its stunning Liquid Retina XDR display offers peak brightness and extreme contrast ratio, perfect for HDR video editing and photo grading. Designed with an aluminum unibody, long battery life, and high-fidelity sound system.', 
'Apple M3 Max 40-Core GPU', '1TB NVMe SSD', 4, 'Apple M3 Max', 48, '3456 x 2234', '16.2', '2.16 kg', 3),

(2, 'Apple MacBook Air 13\" (M3)', 1099.00, 850.00, 1, 'apple_macbook.png', 1, GETDATE(), 'Admin', 
'The incredibly thin and fast Apple MacBook Air 13 with M3 chip is designed for work, play, and everything in between. Powered by the M3 processor, it handles heavy multitasking with ease while remaining completely silent due to its fanless thermal design. With up to 18 hours of battery life and a striking Liquid Retina display, it is the perfect lightweight ultrabook for students and mobile professionals.', 
'Apple M3 10-Core GPU', '512GB SSD', 1, 'Apple M3', 16, '2560 x 1664', '13.6', '1.24 kg', 3),

(3, 'Dell XPS 15 (9530)', 1999.00, 1600.00, 2, 'dell_xps.png', 1, GETDATE(), 'Admin', 
'Crafted with premium materials including CNC machined aluminum and carbon fiber, the Dell XPS 15 offers a premium computing experience. Featuring a 13th Gen Intel Core i7 processor and NVIDIA RTX 4060 graphics, it delivers extreme performance for content creators and casual gamers alike. The gorgeous 15.6-inch OLED touch screen features infinity-edge borders for an immersive visual experience.', 
'NVIDIA GeForce RTX 4060', '1TB NVMe SSD', 1, 'Intel Core i7-13700H', 32, '3456 x 2160', '15.6', '1.92 kg', 1),

(4, 'Dell Precision 5680 Workstation', 2899.00, 2300.00, 2, 'dell_xps.png', 1, GETDATE(), 'Admin', 
'The Dell Precision 5680 is a premium 16-inch mobile workstation designed for professional design, simulation, and engineering workloads. Driven by an Intel Core i9 processor and enterprise-class NVIDIA RTX professional graphics, it is ISV-certified to run industry-standard applications smoothly. Features a stunning UHD+ OLED touchscreen and advanced dual-fan thermal cooling.', 
'NVIDIA RTX A2000 8GB', '2TB NVMe SSD', 4, 'Intel Core i9-13900H', 64, '3840 x 2400', '16.0', '2.03 kg', 1),

(5, 'Lenovo ThinkPad X1 Carbon Gen 11', 1749.00, 1400.00, 3, 'lenovo_thinkpad.png', 1, GETDATE(), 'Admin', 
'The Lenovo ThinkPad X1 Carbon is the gold standard of business laptops. Built with lightweight carbon fiber, it weighs just 1.12 kg while passing military-grade durability tests. Features the legendary ThinkPad keyboard, robust security features like biometric authentication and dTPM, and all-day battery life, making it the top choice for executives and business travelers.', 
'Intel Iris Xe Graphics', '1TB NVMe SSD', 3, 'Intel Core i7-1365U', 32, '1920 x 1200', '14.0', '1.12 kg', 1),

(6, 'Lenovo Legion Pro 7i Gaming Laptop', 2299.00, 1850.00, 3, 'gaming_laptop.png', 1, GETDATE(), 'Admin', 
'Dominating the virtual battlefield, the Lenovo Legion Pro 7i features an Intel Core i9 processor and a powerful NVIDIA RTX 4080 GPU. With Lenovo Legion Coldfront 5.0 cooling, it sustains maximum frame rates under heavy loads. The 16-inch WQXGA IPS display runs at a blistering 240Hz refresh rate, providing fluid and responsive gaming visuals.', 
'NVIDIA GeForce RTX 4080 12GB', '2TB NVMe SSD', 2, 'Intel Core i9-13900HX', 32, '2560 x 1600', '16.0', '2.80 kg', 1),

(7, 'HP Spectre x360 2-in-1 Laptop', 1449.00, 1150.00, 4, 'silver_ultrabook.png', 1, GETDATE(), 'Admin', 
'The HP Spectre x360 is a versatile 2-in-1 convertible laptop that seamlessly flips between laptop and tablet modes. Powered by the latest Intel Core Ultra 7 processor with AI acceleration capabilities, it delivers high efficiency and performance. Featuring a stunning 2.8K OLED touchscreen and long battery life, it is ideal for creative professionals on the move.', 
'Intel Arc Graphics', '1TB NVMe SSD', 1, 'Intel Core Ultra 7 155H', 16, '2880 x 1800', '14.0', '1.44 kg', 1),

(8, 'HP EliteBook 840 G10 Business Laptop', 1299.00, 1000.00, 4, 'dark_laptop.png', 1, GETDATE(), 'Admin', 
'The HP EliteBook 840 is an enterprise-grade notebook designed to keep hybrid workers connected and secure. It offers powerful multitasking with an Intel Core i5 processor, a highly crisp 5-megapixel webcam for business meetings, and built-in HP Wolf Security. Crafted with a premium recycled aluminum chassis.', 
'Intel Iris Xe Graphics', '512GB NVMe SSD', 3, 'Intel Core i5-1335U', 16, '1920 x 1200', '14.0', '1.36 kg', 1),

(9, 'Asus ROG Zephyrus G14 (2024)', 1599.00, 1300.00, 5, 'gaming_laptop.png', 1, GETDATE(), 'Admin', 
'Compact, lightweight, and incredibly powerful, the Asus ROG Zephyrus G14 delivers a premium thin-and-light gaming experience. Equipped with an AMD Ryzen 9 processor and NVIDIA RTX 4070 graphics, it handles the latest AAA titles easily. Featuring a 165Hz ROG Nebula QHD+ display, it renders games with stunning clarity.', 
'NVIDIA GeForce RTX 4070 8GB', '1TB NVMe SSD', 2, 'AMD Ryzen 9 7940HS', 16, '2560 x 1600', '14.0', '1.72 kg', 1),

(10, 'Asus Zenbook 14 OLED', 999.00, 800.00, 5, 'silver_ultrabook.png', 1, GETDATE(), 'Admin', 
'Designed for sub-kilogram portability and premium style, the Asus Zenbook 14 features a beautiful 2.8K OLED display. Powered by an AMD Ryzen 7 processor, it offers exceptional performance for web browsing, office tasks, and multimedia consumption. Features a sleek aluminum chassis and dual stereo speakers tuned by Harman Kardon.', 
'AMD Radeon Graphics', '1TB NVMe SSD', 1, 'AMD Ryzen 7 7730U', 16, '2880 x 1800', '14.0', '1.35 kg', 1),

(11, 'Razer Blade 16 Gaming Laptop', 2999.00, 2400.00, 6, 'razer_blade.png', 1, GETDATE(), 'Admin', 
'The Razer Blade 16 sets a new standard for portable gaming power. Meticulously engineered with a solid aluminum chassis, it packs an Intel Core i9 processor and an NVIDIA RTX 4090 GPU. The dual-mode Mini-LED display allows you to toggle between UHD+ 120Hz for creators and FHD+ 240Hz for competitive gaming.', 
'NVIDIA GeForce RTX 4090 16GB', '2TB NVMe SSD', 2, 'Intel Core i9-13950HX', 32, '3840 x 2400', '16.0', '2.45 kg', 1),

(12, 'MSI Raider GE78 HX Gaming Laptop', 2699.00, 2150.00, 7, 'gaming_laptop.png', 1, GETDATE(), 'Admin', 
'The MSI Raider GE78 HX is a heavy-duty gaming desktop replacement. Driven by Intel''s top-tier i9 processor and NVIDIA RTX 4080 graphics, it is optimized to run games at competitive frame rates. Features the iconic MSI Mystic Light bar on the front chassis and an advanced SteelSeries mechanical keyboard with per-key RGB control.', 
'NVIDIA GeForce RTX 4080 12GB', '2TB NVMe SSD', 2, 'Intel Core i9-13980HX', 32, '2560 x 1600', '17.0', '3.10 kg', 1);
SET IDENTITY_INSERT TbItems OFF;
"@
    $cmd.CommandText = $seedItemsSql
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "Seeded 12 Laptops (TbItems)."

    # Seed Sliders
    $seedSlidersSql = @"
INSERT INTO TbSlider (Title, Description, ImageName, CreatedBy, CreatedDate, CurrentState) VALUES
('Ultimate Gaming Laptops', 'Unleash extreme power with the latest RTX graphics & high-refresh rate displays. Up to 30% Off.', 'slider_gaming.png', 'Admin', GETDATE(), 1),
('Sleek Business Workstations', 'Supercharge your productivity with Intel Core Ultra & Apple M-series chips. Free Delivery.', 'slider_business.png', 'Admin', GETDATE(), 1),
('Lightweight Premium Ultrabooks', 'All-day battery life and gorgeous displays, designed for creators on the go.', 'slider_ultrabook.png', 'Admin', GETDATE(), 1);
"@
    $cmd.CommandText = $seedSlidersSql
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "Seeded Homepage Sliders (TbSlider)."

    # Seed Coupons
    $seedCouponsSql = @"
INSERT INTO TbCoupons (CouponCode, DiscountPercent, ExpiryDate, IsActive) VALUES
('WELCOME2026', 10.00, '2028-12-31', 1),
('LUNAR25', 25.00, '2028-12-31', 1),
('SUPERDEAL', 50.00, '2028-12-31', 1);
"@
    $cmd.CommandText = $seedCouponsSql
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "Seeded Discount Coupons (TbCoupons)."

    # Seed Pages
    $seedPagesSql = @"
SET IDENTITY_INSERT TbPages ON;
INSERT INTO TbPages (PageId, Title, Description, MetaKeyWord, MetaDescriptiuon, ImageName, CurrentState, CreatedDate, CreatedBy) VALUES
(3, 'About Us', '<h3>Welcome to LapShop</h3><p>Founded in 2026, LapShop is the leading provider of high-performance laptops and workstations. We specialize in bringing cutting-edge personal computing directly to creators, engineers, and gamers.</p><p>We partner with top-tier international brands including Apple, HP, Acer, Dell, and Razer to offer only the most certified and high-quality machines. Every device is backed by our official warranty and premium support services.</p>', 'about, lapshop, technology', 'About LapShop premium laptop store', '', 1, GETDATE(), 'Admin'),
(4, 'Terms Of Use', '<h3>Terms of Service</h3><p>By using the LapShop portal, you agree to comply with our purchasing agreements, refund policies, and official usage policies.</p><h4>1. Shipping & Warranties</h4><p>We provide free shipping across major cities and offer official local brand warranties for up to 2 years.</p><h4>2. Refunds & Returns</h4><p>You can return or exchange any purchased laptop within 14 days of delivery if the seal is unbroken.</p>', 'terms, legal, refund', 'Terms of Use for LapShop purchase system', '', 1, GETDATE(), 'Admin'),
(5, 'Contact Us', '<h3>We are Here to Help!</h3><p>Have questions about specs or orders? Contact our sales and technical support departments.</p><div style=\"margin: 20px 0;\"><p><strong>Address:</strong> Cairo, Egypt</p><p><strong>Phone:</strong> +20 123 456 789</p><p><strong>Email:</strong> support@lapshop.com</p></div><p>Our response window is typically under 12 hours.</p>', 'contact, support, sales', 'Contact information and support channels at LapShop', '', 1, GETDATE(), 'Admin');
SET IDENTITY_INSERT TbPages OFF;
"@
    $cmd.CommandText = $seedPagesSql
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "Seeded Text Pages (TbPages)."

    # Get user list for seeding orders
    $getUsersCmd = $connection.CreateCommand()
    $getUsersCmd.CommandText = "SELECT Id FROM AspNetUsers"
    $reader = $getUsersCmd.ExecuteReader()
    $userIds = New-Object System.Collections.Generic.List[string]
    while ($reader.Read()) {
        $userIds.Add($reader.GetString(0))
    }
    $reader.Close()

    if ($userIds.Count -gt 0) {
        Write-Host "Found $($userIds.Count) users. Seeding mockup customer orders..."
        
        # We will seed 5 orders using the available user IDs
        $u1 = $userIds[0]
        $u2 = if ($userIds.Count -gt 1) { $userIds[1] } else { $u1 }
        $u3 = if ($userIds.Count -gt 2) { $userIds[2] } else { $u1 }
        $u4 = if ($userIds.Count -gt 3) { $userIds[3] } else { $u1 }

        # Order 1 (Delivered, 6 days ago)
        $orderSql1 = @"
INSERT INTO TbSalesInvoices (InvoiceDate, DelivryDate, CustomerId, Notes, CreatedBy, CreatedDate, CurrentState, UpdatedDate, UpdatedBy) VALUES
(DATEADD(day, -6, GETDATE()), DATEADD(day, -1, GETDATE()), '$u1', 'Address: 123 Market St, San Francisco | Phone: 555-1234', '$u1', DATEADD(day, -6, GETDATE()), 3, DATEADD(day, -1, GETDATE()), '1');
SELECT SCOPE_IDENTITY() AS OrderId;
"@
        $cmd.CommandText = $orderSql1
        $orderId1 = $cmd.ExecuteScalar()
        $cmd.CommandText = "INSERT INTO TbSalesInvoiceItems (ItemId, InvoiceId, Qty, InvoicePrice) VALUES (1, $orderId1, 1, 3499.00);"
        $cmd.ExecuteNonQuery() | Out-Null

        # Order 2 (Processing, 4 days ago)
        $orderSql2 = @"
INSERT INTO TbSalesInvoices (InvoiceDate, DelivryDate, CustomerId, Notes, CreatedBy, CreatedDate, CurrentState, UpdatedDate, UpdatedBy) VALUES
(DATEADD(day, -4, GETDATE()), DATEADD(day, 1, GETDATE()), '$u2', 'Address: 456 Tech Park, San Jose | Phone: 555-5678', '$u2', DATEADD(day, -4, GETDATE()), 2, DATEADD(day, -2, GETDATE()), '1');
SELECT SCOPE_IDENTITY() AS OrderId;
"@
        $cmd.CommandText = $orderSql2
        $orderId2 = $cmd.ExecuteScalar()
        $cmd.CommandText = "INSERT INTO TbSalesInvoiceItems (ItemId, InvoiceId, Qty, InvoicePrice) VALUES (3, $orderId2, 1, 1999.00);"
        $cmd.ExecuteNonQuery() | Out-Null

        # Order 3 (Pending, 2 days ago)
        $orderSql3 = @"
INSERT INTO TbSalesInvoices (InvoiceDate, DelivryDate, CustomerId, Notes, CreatedBy, CreatedDate, CurrentState) VALUES
(DATEADD(day, -2, GETDATE()), DATEADD(day, 3, GETDATE()), '$u3', 'Address: 789 Cloud Ave, Oakland | Phone: 555-9012', '$u3', DATEADD(day, -2, GETDATE()), 1);
SELECT SCOPE_IDENTITY() AS OrderId;
"@
        $cmd.CommandText = $orderSql3
        $orderId3 = $cmd.ExecuteScalar()
        $cmd.CommandText = "INSERT INTO TbSalesInvoiceItems (ItemId, InvoiceId, Qty, InvoicePrice) VALUES (5, $orderId3, 1, 1749.00);"
        $cmd.ExecuteNonQuery() | Out-Null

        # Order 4 (Pending, Today)
        $orderSql4 = @"
INSERT INTO TbSalesInvoices (InvoiceDate, DelivryDate, CustomerId, Notes, CreatedBy, CreatedDate, CurrentState) VALUES
(GETDATE(), DATEADD(day, 5, GETDATE()), '$u1', 'Address: 123 Market St, San Francisco | Phone: 555-1234', '$u1', GETDATE(), 1);
SELECT SCOPE_IDENTITY() AS OrderId;
"@
        $cmd.CommandText = $orderSql4
        $orderId4 = $cmd.ExecuteScalar()
        $cmd.CommandText = "INSERT INTO TbSalesInvoiceItems (ItemId, InvoiceId, Qty, InvoicePrice) VALUES (11, $orderId4, 1, 2999.00);"
        $cmd.ExecuteNonQuery() | Out-Null

        # Order 5 (Delivered, 15 days ago)
        $orderSql5 = @"
INSERT INTO TbSalesInvoices (InvoiceDate, DelivryDate, CustomerId, Notes, CreatedBy, CreatedDate, CurrentState, UpdatedDate, UpdatedBy) VALUES
(DATEADD(day, -15, GETDATE()), DATEADD(day, -10, GETDATE()), '$u4', 'Address: 99 Broadway, New York | Phone: 555-3344 | Coupon: LUNAR25 (25% off)', '$u4', DATEADD(day, -15, GETDATE()), 3, DATEADD(day, -10, GETDATE()), '1');
SELECT SCOPE_IDENTITY() AS OrderId;
"@
        $cmd.CommandText = $orderSql5
        $orderId5 = $cmd.ExecuteScalar()
        $cmd.CommandText = "INSERT INTO TbSalesInvoiceItems (ItemId, InvoiceId, Qty, InvoicePrice) VALUES (2, $orderId5, 1, 824.25);"
        $cmd.ExecuteNonQuery() | Out-Null

        Write-Host "Seeded 5 mock orders in database."
    }

    # Seed Item Reviews (Evaluations)
    $evalSql = @"
INSERT INTO TbItemEvaluations (ItemId, CustomerName, CustomerEmail, Rating, ReviewText, CreatedDate) VALUES
(1, 'John Doe', 'john@example.com', 5, 'Absolutely incredible laptop. The M3 Max chip compiles code instantly and the screen is beautiful.', DATEADD(day, -5, GETDATE())),
(3, 'Jane Smith', 'jane@example.com', 4, 'Very solid laptop. The OLED screen is breathtaking, but it runs a bit warm when multitasking.', DATEADD(day, -3, GETDATE())),
(6, 'Alex Mercer', 'alex@example.com', 5, 'Absolute gaming monster! High FPS on Ultra settings. Highly recommended.', DATEADD(day, -1, GETDATE())),
(7, 'Sarah Connor', 'sarah@example.com', 5, 'Best ultrabook I have owned. The keyboard is crisp and battery life lasts a full workday.', DATEADD(day, -2, GETDATE()));
"@
    $cmd.CommandText = $evalSql
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "Seeded sample reviews (TbItemEvaluations)."

    Write-Host "Database seeded successfully!"
    $connection.Close()
} catch {
    Write-Error $_.Exception.Message
    if ($connection.State -eq [System.Data.ConnectionState]::Open) { $connection.Close() }
}
