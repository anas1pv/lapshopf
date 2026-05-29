# SeedData.ps1
# Script to copy laptop image assets and seed the LapShop SQL database.

$projectDir = "C:\Users\anasa\source\repos\lapshop\lapshop"
$itemsUploadDir = "$projectDir\wwwroot\Uploads\Items"
$slidersUploadDir = "$projectDir\wwwroot\Uploads\Sliders"

# Ensure directories exist
if (-not (Test-Path $itemsUploadDir)) { New-Item -ItemType Directory -Path $itemsUploadDir -Force }
if (-not (Test-Path $slidersUploadDir)) { New-Item -ItemType Directory -Path $slidersUploadDir -Force }

# Source files (hardcoded to generated artifacts)
$sourceApple = "C:\Users\anasa\.gemini\antigravity\brain\afdf9e4c-085b-4c02-9045-5d3347752d22\apple_macbook_1779826382060.png"
$sourceGaming = "C:\Users\anasa\.gemini\antigravity\brain\afdf9e4c-085b-4c02-9045-5d3347752d22\gaming_laptop_1779826399525.png"
$sourceSilver = "C:\Users\anasa\.gemini\antigravity\brain\afdf9e4c-085b-4c02-9045-5d3347752d22\silver_ultrabook_1779826416164.png"
$sourceDark = "C:\Users\anasa\.gemini\antigravity\brain\afdf9e4c-085b-4c02-9045-5d3347752d22\dark_laptop_1779826433561.png"
$sourceSlider = "C:\Users\anasa\.gemini\antigravity\brain\afdf9e4c-085b-4c02-9045-5d3347752d22\slider_banner_1779826449245.png"

# Copy image assets
Copy-Item $sourceApple -Destination "$itemsUploadDir\apple_macbook.png" -Force
Copy-Item $sourceGaming -Destination "$itemsUploadDir\gaming_laptop.png" -Force
Copy-Item $sourceSilver -Destination "$itemsUploadDir\silver_ultrabook.png" -Force
Copy-Item $sourceDark -Destination "$itemsUploadDir\dark_laptop.png" -Force
Copy-Item $sourceSlider -Destination "$slidersUploadDir\slider1.png" -Force

Write-Output "Image assets copied successfully!"

# SQL Database Seeding
$connString = "Server=localhost;Database=LapShop;Integrated Security=True;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connString)

try {
    $connection.Open()
    Write-Output "Connected to database. Starting SQL seeding..."

    # 1. Seed Settings table (if empty)
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM TbSettings"
    $count = $cmd.ExecuteScalar()
    if ($count -eq 0) {
        $cmd.CommandText = @"
INSERT INTO TbSettings (WebsiteName, Logo, WebsiteDescription, FacebookLink, TwitterLink, InstgramLink, YoutubeLink, Address, ContactNumber, MiddlePanner, LastPanner)
VALUES (
    'LapShop', 
    'logo.png', 
    'Your ultimate destination for premium laptops, gaming rigs, and enterprise business workstations.', 
    'https://facebook.com/lapshop', 
    'https://twitter.com/lapshop', 
    'https://instagram.com/lapshop', 
    'https://youtube.com/lapshop', 
    'Cairo, Egypt', 
    '+20 123 456 789', 
    'middle_banner.png', 
    'last_banner.png'
)
"@
        $cmd.ExecuteNonQuery()
        Write-Output "Seeded TbSettings."
    } else {
        Write-Output "TbSettings already has data."
    }

    # 2. Seed Slider table (if empty)
    $cmd.CommandText = "SELECT COUNT(*) FROM TbSlider"
    $count = $cmd.ExecuteScalar()
    if ($count -eq 0) {
        $cmd.CommandText = @"
INSERT INTO TbSlider (Title, Description, ImageName, CreatedBy, CreatedDate, CurrentState)
VALUES 
('Ultimate Gaming Laptops', 'Unleash extreme power with the latest RTX graphics & high-refresh rate displays. Up to 30% Off.', 'slider1.png', 'Admin', GETDATE(), 1),
('Sleek Business Workstations', 'Supercharge your productivity with Intel Core Ultra & Apple M-series chips. Free Delivery.', 'slider1.png', 'Admin', GETDATE(), 1),
('Lightweight Premium Ultrabooks', 'All-day battery life and gorgeous displays, designed for creators on the go.', 'slider1.png', 'Admin', GETDATE(), 1)
"@
        $cmd.ExecuteNonQuery()
        Write-Output "Seeded TbSlider."
    } else {
        Write-Output "TbSlider already has data."
    }

    # 4. Seed TbPages table (if empty)
    $cmd.CommandText = "SELECT COUNT(*) FROM TbPages"
    $count = $cmd.ExecuteScalar()
    if ($count -eq 0) {
        $cmd.CommandText = "SET IDENTITY_INSERT TbPages ON"
        $cmd.ExecuteNonQuery()

        $cmd.CommandText = @"
INSERT INTO TbPages (PageId, Title, Description, MetaKeyWord, MetaDescriptiuon, ImageName, CurrentState, CreatedDate, CreatedBy)
VALUES 
(3, 'About Us', '<h3>Welcome to LapShop</h3><p>Founded in 2026, LapShop is the leading provider of high-performance laptops and workstations. We specialize in bringing cutting-edge personal computing directly to creators, engineers, and gamers.</p><p>We partner with top-tier international brands including Apple, HP, Acer, Dell, and Razer to offer only the most certified and high-quality machines. Every device is backed by our official warranty and premium support services.</p>', 'about, lapshop, technology', 'About LapShop premium laptop store', '', 1, GETDATE(), 'Admin'),
(4, 'Terms Of Use', '<h3>Terms of Service</h3><p>By using the LapShop portal, you agree to comply with our purchasing agreements, refund policies, and official usage policies.</p><h4>1. Shipping & Warranties</h4><p>We provide free shipping across major cities and offer official local brand warranties for up to 2 years.</p><h4>2. Refunds & Returns</h4><p>You can return or exchange any purchased laptop within 14 days of delivery if the seal is unbroken.</p>', 'terms, legal, refund', 'Terms of Use for LapShop purchase system', '', 1, GETDATE(), 'Admin'),
(5, 'Contact Us', '<h3>We are Here to Help!</h3><p>Have questions about specs or orders? Contact our sales and technical support departments.</p><div style=\"margin: 20px 0;\"><p><strong>Address:</strong> Cairo, Egypt</p><p><strong>Phone:</strong> +20 123 456 789</p><p><strong>Email:</strong> support@lapshop.com</p></div><p>Our response window is typically under 12 hours.</p>', 'contact, support, sales', 'Contact information and support channels at LapShop', '', 1, GETDATE(), 'Admin')
"@
        $cmd.ExecuteNonQuery()

        $cmd.CommandText = "SET IDENTITY_INSERT TbPages OFF"
        $cmd.ExecuteNonQuery()
        Write-Output "Seeded TbPages."
    } else {
        Write-Output "TbPages already has data."
    }

    # 3. Update all Items in TbItems (Seeding descriptions and mapping brands to correct images)
    $cmd.CommandText = "SELECT ItemId, ItemName, Processor, RamSize, HardDisk, Gpu, CategoryId FROM TbItems"
    $reader = $cmd.ExecuteReader()
    
    $items = New-Object System.Collections.Generic.List[Object]
    while ($reader.Read()) {
        $item = [PSCustomObject]@{
            ItemId = $reader.GetInt32(0)
            ItemName = $reader.GetString(1)
            Processor = if ($reader.IsDBNull(2)) { "Intel Core" } else { $reader.GetString(2) }
            RamSize = if ($reader.IsDBNull(3)) { 8 } else { $reader.GetInt32(3) }
            HardDisk = if ($reader.IsDBNull(4)) { "256GB SSD" } else { $reader.GetString(4) }
            Gpu = if ($reader.IsDBNull(5)) { "Integrated Graphics" } else { $reader.GetString(5) }
            CategoryId = $reader.GetInt32(6)
        }
        $items.Add($item)
    }
    $reader.Close()

    Write-Output "Found $($items.Count) items to update. Writing descriptions & mapping images..."

    $updateCmd = $connection.CreateCommand()
    $updateCmd.CommandText = "UPDATE TbItems SET Description = @desc, ImageName = @img WHERE ItemId = @id"
    $updateCmd.Parameters.Add("@desc", [System.Data.SqlDbType]::NVarChar, -1) | Out-Null
    $updateCmd.Parameters.Add("@img", [System.Data.SqlDbType]::NVarChar, 200) | Out-Null
    $updateCmd.Parameters.Add("@id", [System.Data.SqlDbType]::Int) | Out-Null

    $i = 0
    foreach ($item in $items) {
        # Determine image by Brand category:
        # CategoryId: 1=Apple, 8=MSI, 14=Razer -> Apple/Gaming
        # Other CategoryIds -> Silver or Dark laptop
        $img = "silver_ultrabook.png"
        if ($item.CategoryId -eq 1) {
            $img = "apple_macbook.png"
        } elseif ($item.CategoryId -eq 8 -or $item.CategoryId -eq 14 -or $item.ItemName -like "*gaming*" -or $item.Gpu -like "*RTX*" -or $item.Gpu -like "*Nvidia*") {
            $img = "gaming_laptop.png"
        } elseif ($item.CategoryId -eq 6 -or $item.CategoryId -eq 5 -or $item.Processor -like "*i7*" -or $item.RamSize -ge 16) {
            $img = "dark_laptop.png"
        }

        # Generate a premium, technical description
        $desc = "The premium $($item.ItemName) represents the peak of modern portable computing. Specifically configured with a powerful $($item.Processor) processor, $($item.RamSize)GB high-speed RAM, and spacious $($item.HardDisk) storage. Graphics rendering is powered by $($item.Gpu), producing stunning visuals for engineering, content creation, and high-performance gaming. Designed with premium materials for maximum durability and lightweight portability."

        $updateCmd.Parameters["@desc"].Value = $desc
        $updateCmd.Parameters["@img"].Value = $img
        $updateCmd.Parameters["@id"].Value = $item.ItemId
        $updateCmd.ExecuteNonQuery()
        $i++
    }

    Write-Output "Successfully updated $i database items with specs-based descriptions and premium laptop images!"
    $connection.Close()
} catch {
    Write-Error $_.Exception.Message
    if ($connection.State -eq [System.Data.ConnectionState]::Open) { $connection.Close() }
}
