# generate_catalog.py
# Python script to procedurally generate 1000 unique, high-quality laptops and seed SQL data.

import random
from datetime import datetime

# Database metadata definitions
BRANDS = [
    {"id": 1, "name": "Apple", "img": "apple_macbook.png"},
    {"id": 2, "name": "Dell", "img": "dell_xps.png"},
    {"id": 3, "name": "Lenovo", "img": "lenovo_thinkpad.png"},
    {"id": 4, "name": "HP", "img": "silver_ultrabook.png"},
    {"id": 5, "name": "Asus", "img": "gaming_laptop.png"},
    {"id": 6, "name": "Razer", "img": "razer_blade.png"},
    {"id": 7, "name": "MSI", "img": "gaming_laptop.png"},
    {"id": 8, "name": "Acer", "img": "silver_ultrabook.png"},
    {"id": 9, "name": "Microsoft", "img": "silver_ultrabook.png"},
    {"id": 10, "name": "Samsung", "img": "dark_laptop.png"}
]

ITEM_TYPES = [
    {"id": 1, "name": "Ultrabook", "img": "ultrabook.png"},
    {"id": 2, "name": "Gaming", "img": "gaming.png"},
    {"id": 3, "name": "Business", "img": "business.png"},
    {"id": 4, "name": "Workstation", "img": "workstation.png"}
]

OS_TYPES = [
    {"id": 1, "name": "Windows 11", "img": "win11.png", "home": 1},
    {"id": 2, "name": "Windows 10", "img": "win10.png", "home": 1},
    {"id": 3, "name": "macOS", "img": "macos.png", "home": 1},
    {"id": 4, "name": "Linux", "img": "linux.png", "home": 0},
    {"id": 5, "name": "ChromeOS", "img": "chrome.png", "home": 0}
]

# Brand-specific model series
SERIES = {
    "Apple": ["MacBook Pro 14", "MacBook Pro 16", "MacBook Air 13", "MacBook Air 15"],
    "Dell": ["XPS 13", "XPS 15", "XPS 17", "Latitude 5440", "Latitude 7440", "Precision 5480", "Precision 5680", "Inspiron 15", "Inspiron 16", "G15 Gaming"],
    "Lenovo": ["ThinkPad X1 Carbon", "ThinkPad T14", "ThinkPad P16", "Yoga Book 9i", "Yoga Slim 7", "Legion Pro 5", "Legion Pro 7", "IdeaPad Slim 3", "IdeaPad 5 Pro", "LOQ 15"],
    "HP": ["Spectre x360 14", "Spectre x360 16", "Envy x360 15", "Pavilion 15", "Pavilion Plus 14", "Omen 16", "Omen Transcend 16", "EliteBook 840 G10", "EliteBook 1040 G10", "Victus 16"],
    "Asus": ["ROG Zephyrus G14", "ROG Zephyrus G16", "ROG Strix SCAR 16", "ROG Strix SCAR 18", "Zenbook 14 OLED", "Zenbook Pro 16X", "Vivobook 15", "Vivobook Pro 16", "TUF Gaming A15", "TUF Gaming F15"],
    "Razer": ["Blade 14", "Blade 15", "Blade 16", "Blade 18"],
    "MSI": ["Raider GE78 HX", "Titan GT77 HX", "Stealth 16 Studio", "Pulse 15", "Katana 15", "Cyborg 15", "Prestige 14", "Prestige 16", "Modern 15", "Summit E16 Flip"],
    "Acer": ["Swift Go 14", "Swift Edge 16", "Aspire 3", "Aspire 5", "Predator Helios 16", "Predator Helios 18", "Nitro 5", "Nitro 16", "TravelMate P4", "Spin 5"],
    "Microsoft": ["Surface Laptop 5 13.5", "Surface Laptop 5 15", "Surface Pro 9", "Surface Laptop Studio 2", "Surface Go 3"],
    "Samsung": ["Galaxy Book3 Pro", "Galaxy Book3 Pro 360", "Galaxy Book3 Ultra", "Galaxy Book3 360", "Galaxy Book Go"]
}

# Specification options by laptop types
SPEC_TEMPLATES = {
    "Apple": {
        "processors": ["M3", "M3 Pro", "M3 Max", "M2", "M2 Pro", "M2 Max", "M1 Pro"],
        "gpus": ["Apple 10-Core GPU", "Apple 14-Core GPU", "Apple 18-Core GPU", "Apple 30-Core GPU", "Apple 40-Core GPU"],
        "ram": [8, 16, 24, 32, 48, 64, 96, 128],
        "harddisk": ["256GB SSD", "512GB SSD", "1TB SSD", "2TB SSD", "4TB SSD"],
        "screens": ["13.6", "14.2", "15.3", "16.2"],
        "resolutions": ["2560x1664", "3024x1964", "2880x1864", "3456x2234"],
        "weights": ["1.24 kg", "1.61 kg", "1.51 kg", "2.16 kg"]
    },
    "Gaming": {
        "processors": ["Intel Core i9-13980HX", "Intel Core i9-13900HX", "Intel Core i7-13700HX", "AMD Ryzen 9 7945HX", "AMD Ryzen 7 7840HS", "Intel Core i9-14900HX", "AMD Ryzen 9 8945HS"],
        "gpus": ["NVIDIA GeForce RTX 4090 16GB", "NVIDIA GeForce RTX 4080 12GB", "NVIDIA GeForce RTX 4070 8GB", "NVIDIA GeForce RTX 4060 8GB", "NVIDIA GeForce RTX 4050 6GB"],
        "ram": [16, 32, 64, 96],
        "harddisk": ["512GB NVMe SSD", "1TB NVMe SSD", "2TB NVMe SSD", "4TB NVMe SSD"],
        "screens": ["14.0", "15.6", "16.0", "17.3", "18.0"],
        "resolutions": ["2560x1600", "1920x1080", "2560x1440", "3840x2400"],
        "weights": ["1.72 kg", "2.30 kg", "2.50 kg", "2.80 kg", "3.20 kg"]
    },
    "Workstation": {
        "processors": ["Intel Core i9-13900H", "Intel Core i7-13800H", "AMD Ryzen 9 7940HS", "Intel Core i9-14900H", "Intel Xeon W-11955M"],
        "gpus": ["NVIDIA RTX A2000 8GB", "NVIDIA RTX A3000 12GB", "NVIDIA RTX 4000 Ada 12GB", "NVIDIA RTX 5000 Ada 16GB", "NVIDIA GeForce RTX 4070"],
        "ram": [32, 64, 128],
        "harddisk": ["1TB NVMe SSD", "2TB NVMe SSD", "4TB NVMe SSD"],
        "screens": ["15.6", "16.0", "17.0", "17.3"],
        "resolutions": ["3840x2400", "2560x1600", "3840x2160"],
        "weights": ["1.95 kg", "2.10 kg", "2.40 kg", "2.90 kg"]
    },
    "Ultrabook": {
        "processors": ["Intel Core Ultra 7 155H", "Intel Core Ultra 5 125H", "Intel Core i7-1355U", "Intel Core i5-1335U", "AMD Ryzen 7 7735U", "AMD Ryzen 5 7530U"],
        "gpus": ["Intel Arc Graphics", "Intel Iris Xe Graphics", "AMD Radeon Graphics"],
        "ram": [8, 16, 32],
        "harddisk": ["256GB SSD", "512GB NVMe SSD", "1TB NVMe SSD", "2TB NVMe SSD"],
        "screens": ["13.3", "13.4", "14.0", "15.6"],
        "resolutions": ["1920x1200", "2880x1800", "2560x1600", "3456x2160"],
        "weights": ["0.99 kg", "1.15 kg", "1.25 kg", "1.40 kg"]
    },
    "Business": {
        "processors": ["Intel Core i7-1365U", "Intel Core i5-1345U", "Intel Core i7-1355U", "AMD Ryzen 7 PRO 7730U", "AMD Ryzen 5 PRO 7530U"],
        "gpus": ["Intel Iris Xe Graphics", "Intel UHD Graphics", "AMD Radeon Graphics"],
        "ram": [8, 16, 32, 64],
        "harddisk": ["256GB NVMe SSD", "512GB NVMe SSD", "1TB NVMe SSD"],
        "screens": ["14.0", "15.6", "16.0"],
        "resolutions": ["1920x1080", "1920x1200", "2560x1600"],
        "weights": ["1.12 kg", "1.36 kg", "1.50 kg", "1.70 kg"]
    }
}

SUFFIXES = [
    "Edition", "Pro Edition", "Developer Edition", "Performance Pack", 
    "Extreme Config", "Max Config", "Carbon Edition", "Creator Edition", 
    "Signature Edition", "Enterprise Setup", "Essential Plus", "Elite Selection",
    "Special Edition", "Core Version", "Studio Config", "Ultra Edition"
]

DESC_TEMPLATES = [
    "The premium {name} is specifically configured for professionals, developers, and creators. It is powered by a high-performance {cpu} processor, {ram}GB of high-speed memory, and a fast {ssd} storage drive. Visuals are rendered on a gorgeous {screen_size}-inch {res} display powered by {gpu} graphics. Designed with high-quality components, providing an excellent blend of battery life, durability, and extreme performance.",
    "Engineered for high efficiency and productivity, the {name} stands as a powerhouse in portable computing. Equipped with the advanced {cpu} CPU, {ram}GB RAM, and {ssd} SSD storage, this laptop handles multi-threaded applications and virtualization without breaking a sweat. It also features a stunning {screen_size}\" {res} screen with {gpu} graphics for rich and crisp display output.",
    "Experience next-generation computing with the all-new {name}. Configured with {cpu} CPU, {ram}GB RAM, and a spacious {ssd} solid state drive, it is built to deliver fast load times and responsive multitasking. The lighweight chassis hosts a beautiful {screen_size}-inch screen at {res} resolution, rendering vivid colors and deep blacks via its {gpu} graphics processor.",
    "Uncompromising speed and gorgeous design define the {name}. Perfect for creative workloads, software engineering, and high-performance computing, it packs a {cpu} processor, {ram}GB of RAM, and {ssd} of storage. Enjoy immersive graphics on the {screen_size}\" display at {res} resolution powered by {gpu} graphics. Ideal for power users who demand the best."
]

def get_hd_gb(hd_str):
    parts = hd_str.split()
    size_part = parts[0]
    if "TB" in size_part:
        return int(size_part.replace("TB", "")) * 1024
    elif "GB" in size_part:
        return int(size_part.replace("GB", ""))
    return 512

def generate_laptops():
    items = []
    used_names = set()
    
    # 1. Generate Apple Laptops (enforce strict Apple rules)
    apple_series = SERIES["Apple"]
    apple_specs = SPEC_TEMPLATES["Apple"]
    for i in range(120): # Generate ~120 Apple laptops
        series_name = random.choice(apple_series)
        suffix = random.choice(SUFFIXES)
        name = f"Apple {series_name} ({suffix})"
        
        # Ensure name uniqueness
        idx = 2
        while name in used_names:
            name = f"Apple {series_name} ({suffix} v{idx})"
            idx += 1
        used_names.add(name)
        
        cpu = random.choice(apple_specs["processors"])
        gpu = random.choice(apple_specs["gpus"])
        ram = random.choice(apple_specs["ram"])
        hd = random.choice(apple_specs["harddisk"])
        screen = random.choice(apple_specs["screens"])
        res = random.choice(apple_specs["resolutions"])
        weight = random.choice(apple_specs["weights"])
        
        # Apple is Ultrabook (1) or Workstation (4)
        item_type_id = 4 if "Pro" in series_name and ("Max" in cpu or ram >= 64) else 1
        os_id = 3 # macOS
        
        # Price calculation based on specs
        base_price = 999.00
        if "Pro" in series_name: base_price += 500.00
        if "16" in series_name or "15" in series_name: base_price += 200.00
        if "Max" in cpu: base_price += 800.00
        elif "Pro" in cpu: base_price += 300.00
        base_price += (ram * 10) + (get_hd_gb(hd) * 0.15)
        
        purchase_price = round(base_price, 2)
        sales_price = round(purchase_price * random.choice([1.18, 1.20, 1.22, 1.25]), 2)
        
        desc = random.choice(DESC_TEMPLATES).format(
            name=name, cpu=cpu, ram=ram, ssd=hd, screen_size=screen, res=res, gpu=gpu
        ).replace("'", "''")
        
        items.append({
            "name": name,
            "sales_price": sales_price,
            "purchase_price": purchase_price,
            "category_id": 1, # Apple
            "image": "apple_macbook.png",
            "desc": desc,
            "gpu": gpu,
            "hd": hd,
            "type_id": item_type_id,
            "cpu": cpu,
            "ram": ram,
            "res": res,
            "screen": screen,
            "weight": weight,
            "os_id": os_id
        })
        
    # 2. Generate Non-Apple Laptops (Dell, Lenovo, HP, Asus, Razer, MSI, Acer, Microsoft, Samsung)
    non_apple_brands = BRANDS[1:] # Exclude Apple
    
    # We need 880 more laptops to reach 1000
    for idx_item in range(880):
        brand = random.choice(non_apple_brands)
        brand_name = brand["name"]
        series_list = SERIES[brand_name]
        series_name = random.choice(series_list)
        suffix = random.choice(SUFFIXES)
        
        name = f"{brand_name} {series_name} ({suffix})"
        idx = 2
        while name in used_names:
            name = f"{brand_name} {series_name} ({suffix} v{idx})"
            idx += 1
        used_names.add(name)
        
        # Determine Item Type based on series name keyword
        is_gaming = any(k in series_name.lower() for k in ["gaming", "legion", "rog", "omen", "predator", "nitro", "victus", "loq", "blade", "raider", "titan", "katana", "cyborg"])
        is_workstation = any(k in series_name.lower() for k in ["precision", "thinkpad p", "pro 16x", "summit", "studio"])
        is_business = any(k in series_name.lower() for k in ["thinkpad t", "latitude", "elitebook", "travelmate"])
        
        if is_gaming:
            item_type_id = 2
            spec_type = "Gaming"
        elif is_workstation:
            item_type_id = 4
            spec_type = "Workstation"
        elif is_business:
            item_type_id = 3
            spec_type = "Business"
        else:
            item_type_id = random.choice([1, 3]) # Ultrabook or Business
            spec_type = "Ultrabook" if item_type_id == 1 else "Business"
            
        specs = SPEC_TEMPLATES[spec_type]
        
        cpu = random.choice(specs["processors"])
        gpu = random.choice(specs["gpus"])
        ram = random.choice(specs["ram"])
        hd = random.choice(specs["harddisk"])
        screen = random.choice(specs["screens"])
        res = random.choice(specs["resolutions"])
        weight = random.choice(specs["weights"])
        
        # Enforce OS
        if is_workstation or is_business:
            os_id = random.choice([1, 4]) # Windows 11 or Linux
        else:
            os_id = random.choice([1, 2]) # Windows 11 or Windows 10
            
        # Select matching premium mockup image based on brand/type
        img = brand["img"]
        if item_type_id == 2: # Gaming
            img = "razer_blade.png" if brand_name == "Razer" else "gaming_laptop.png"
        elif brand_name == "Dell":
            img = "dell_xps.png"
        elif brand_name == "Lenovo":
            img = "lenovo_thinkpad.png"
        elif brand_name == "Apple":
            img = "apple_macbook.png"
            
        # Price calculation based on specs
        base_price = 450.00
        if item_type_id == 2: base_price += 600.00 # Gaming premium
        if item_type_id == 4: base_price += 800.00 # Workstation premium
        if "i9" in cpu or "Ryzen 9" in cpu: base_price += 400.00
        if "4090" in gpu or "4080" in gpu: base_price += 700.00
        elif "4070" in gpu or "4060" in gpu: base_price += 300.00
        
        base_price += (ram * 6) + (get_hd_gb(hd) * 0.12)
        
        purchase_price = round(base_price, 2)
        sales_price = round(purchase_price * random.choice([1.15, 1.18, 1.20, 1.24]), 2)
        
        desc = random.choice(DESC_TEMPLATES).format(
            name=name, cpu=cpu, ram=ram, ssd=hd, screen_size=screen, res=res, gpu=gpu
        ).replace("'", "''")
        
        items.append({
            "name": name,
            "sales_price": sales_price,
            "purchase_price": purchase_price,
            "category_id": brand["id"],
            "image": img,
            "desc": desc,
            "gpu": gpu,
            "hd": hd,
            "type_id": item_type_id,
            "cpu": cpu,
            "ram": ram,
            "res": res,
            "screen": screen,
            "weight": weight,
            "os_id": os_id
        })
        
    return items

def write_sql_script(items):
    sql_file = "seed_1000_items.sql"
    print(f"Writing {len(items)} generated items to {sql_file}...")
    
    with open(sql_file, "w", encoding="utf-8") as f:
        # 1. Truncate tables and reseed
        f.write("-- SQL Seed Script: 1000 Unique Real Laptops\n")
        f.write("DELETE FROM TbItemEvaluations;\n")
        f.write("DELETE FROM TbSalesInvoiceItems;\n")
        f.write("DELETE FROM TbSalesInvoices;\n")
        f.write("DELETE FROM TbItemImages;\n")
        f.write("DELETE FROM TbItemDiscount;\n")
        f.write("DELETE FROM TbCustomerItems;\n")
        f.write("DELETE FROM TbItems;\n")
        f.write("DELETE FROM TbCategories;\n")
        f.write("DELETE FROM TbItemTypes;\n")
        f.write("DELETE FROM TbOs;\n")
        f.write("DELETE FROM TbSlider;\n")
        f.write("DELETE FROM TbCoupons;\n")
        f.write("DELETE FROM TbPages;\n\n")
        
        f.write("DBCC CHECKIDENT ('TbItemEvaluations', RESEED, 0);\n")
        f.write("DBCC CHECKIDENT ('TbSalesInvoiceItems', RESEED, 0);\n")
        f.write("DBCC CHECKIDENT ('TbSalesInvoices', RESEED, 0);\n")
        f.write("DBCC CHECKIDENT ('TbItemImages', RESEED, 0);\n")
        f.write("DBCC CHECKIDENT ('TbItemDiscount', RESEED, 0);\n")
        f.write("DBCC CHECKIDENT ('TbItems', RESEED, 0);\n")
        f.write("DBCC CHECKIDENT ('TbCategories', RESEED, 0);\n")
        f.write("DBCC CHECKIDENT ('TbItemTypes', RESEED, 0);\n")
        f.write("DBCC CHECKIDENT ('TbOs', RESEED, 0);\n")
        f.write("DBCC CHECKIDENT ('TbSlider', RESEED, 0);\n")
        f.write("DBCC CHECKIDENT ('TbCoupons', RESEED, 0);\n")
        f.write("DBCC CHECKIDENT ('TbPages', RESEED, 0);\n\n")
        
        # 2. Seed Operating Systems
        f.write("SET IDENTITY_INSERT TbOs ON;\n")
        for os in OS_TYPES:
            f.write(f"INSERT INTO TbOs (OsId, OsName, ImageName, ShowInHomePage, CurrentState, CreatedDate, CreatedBy) VALUES ({os['id']}, '{os['name']}', '{os['img']}', {os['home']}, 1, GETDATE(), 'Admin');\n")
        f.write("SET IDENTITY_INSERT TbOs OFF;\n\n")
        
        # 3. Seed Item Types
        f.write("SET IDENTITY_INSERT TbItemTypes ON;\n")
        for t in ITEM_TYPES:
            f.write(f"INSERT INTO TbItemTypes (ItemTypeId, ItemTypeName, ImageName, CurrentState, CreatedDate, CreatedBy) VALUES ({t['id']}, '{t['name']}', '{t['img']}', 1, GETDATE(), 'Admin');\n")
        f.write("SET IDENTITY_INSERT TbItemTypes OFF;\n\n")
        
        # 4. Seed Categories
        f.write("SET IDENTITY_INSERT TbCategories ON;\n")
        for b in BRANDS:
            f.write(f"INSERT INTO TbCategories (CategoryId, CategoryName, ImageName, CurrentState, CreatedDate, CreatedBy) VALUES ({b['id']}, '{b['name']}', '{b['img']}', 1, GETDATE(), 'Admin');\n")
        f.write("SET IDENTITY_INSERT TbCategories OFF;\n\n")
        
        # 5. Seed Items (in batches of 50 to fit nicely in SQL execution buffer)
        f.write("SET IDENTITY_INSERT TbItems ON;\n")
        
        batch_size = 50
        for i in range(0, len(items), batch_size):
            batch = items[i:i+batch_size]
            values_list = []
            for idx, item in enumerate(batch):
                item_id = i + idx + 1
                val_str = f"({item_id}, '{item['name']}', {item['sales_price']}, {item['purchase_price']}, {item['category_id']}, '{item['image']}', 1, GETDATE(), 'Admin', '{item['desc']}', '{item['gpu']}', '{item['hd']}', {item['type_id']}, '{item['cpu']}', {item['ram']}, '{item['res']}', '{item['screen']}', '{item['weight']}', {item['os_id']})"
                values_list.append(val_str)
                
            insert_query = "INSERT INTO TbItems (ItemId, ItemName, SalesPrice, PurchasePrice, CategoryId, ImageName, CurrentState, CreatedDate, CreatedBy, Description, Gpu, HardDisk, ItemTypeId, Processor, RamSize, ScreenReslution, ScreenSize, Weight, OsId) VALUES\n" + ",\n".join(values_list) + ";\n"
            f.write(insert_query)
            
        f.write("SET IDENTITY_INSERT TbItems OFF;\n\n")
        
        # 6. Seed Slider
        f.write("INSERT INTO TbSlider (Title, Description, ImageName, CreatedBy, CreatedDate, CurrentState) VALUES\n")
        f.write("('Ultimate Gaming Laptops', 'Unleash extreme power with the latest RTX graphics & high-refresh rate displays. Up to 30% Off.', 'slider1.png', 'Admin', GETDATE(), 1),\n")
        f.write("('Sleek Business Workstations', 'Supercharge your productivity with Intel Core Ultra & Apple M-series chips. Free Delivery.', 'slider1.png', 'Admin', GETDATE(), 1),\n")
        f.write("('Lightweight Premium Ultrabooks', 'All-day battery life and gorgeous displays, designed for creators on the go.', 'slider1.png', 'Admin', GETDATE(), 1);\n\n")
        
        # 7. Seed Coupons
        f.write("INSERT INTO TbCoupons (CouponCode, DiscountPercent, ExpiryDate, IsActive) VALUES\n")
        f.write("('WELCOME2026', 10.00, '2028-12-31', 1),\n")
        f.write("('LUNAR25', 25.00, '2028-12-31', 1),\n")
        f.write("('SUPERDEAL', 50.00, '2028-12-31', 1);\n\n")
        
        # 8. Seed Pages
        f.write("SET IDENTITY_INSERT TbPages ON;\n")
        f.write("INSERT INTO TbPages (PageId, Title, Description, MetaKeyWord, MetaDescriptiuon, ImageName, CurrentState, CreatedDate, CreatedBy) VALUES\n")
        f.write("(3, 'About Us', '<h3>Welcome to LapShop</h3><p>Founded in 2026, LapShop is the leading provider of high-performance laptops and workstations. We specialize in bringing cutting-edge personal computing directly to creators, engineers, and gamers.</p>', 'about, lapshop', 'About LapShop premium laptop store', '', 1, GETDATE(), 'Admin'),\n")
        f.write("(4, 'Terms Of Use', '<h3>Terms of Service</h3><p>By using the LapShop portal, you agree to comply with our purchasing agreements, refund policies, and official usage policies.</p>', 'terms, legal', 'Terms of Use for LapShop purchase system', '', 1, GETDATE(), 'Admin'),\n")
        f.write("(5, 'Contact Us', '<h3>We are Here to Help!</h3><p>Have questions about specs or orders? Contact our sales and support departments.</p>', 'contact, support', 'Contact information at LapShop', '', 1, GETDATE(), 'Admin');\n")
        f.write("SET IDENTITY_INSERT TbPages OFF;\n\n")
        
        # 9. Seed mockup orders dynamically linking to the first available users
        f.write("DECLARE @u1 NVARCHAR(450);\n")
        f.write("DECLARE @u2 NVARCHAR(450);\n")
        f.write("DECLARE @u3 NVARCHAR(450);\n")
        f.write("SELECT TOP 1 @u1 = Id FROM AspNetUsers;\n")
        f.write("SELECT TOP 1 @u2 = Id FROM AspNetUsers WHERE Id != @u1;\n")
        f.write("SELECT TOP 1 @u3 = Id FROM AspNetUsers WHERE Id != @u1 AND Id != @u2;\n")
        f.write("IF @u2 IS NULL SET @u2 = @u1;\n")
        f.write("IF @u3 IS NULL SET @u3 = @u1;\n\n")
        
        # Create Orders
        f.write("DECLARE @orderId INT;\n\n")
        
        # Order 1
        f.write("INSERT INTO TbSalesInvoices (InvoiceDate, DelivryDate, CustomerId, Notes, CreatedBy, CreatedDate, CurrentState, UpdatedDate, UpdatedBy) VALUES\n")
        f.write("(DATEADD(day, -6, GETDATE()), DATEADD(day, -1, GETDATE()), @u1, 'Address: 123 Market St, San Francisco | Phone: 555-1234', @u1, DATEADD(day, -6, GETDATE()), 3, DATEADD(day, -1, GETDATE()), '1');\n")
        f.write("SET @orderId = SCOPE_IDENTITY();\n")
        f.write("INSERT INTO TbSalesInvoiceItems (ItemId, InvoiceId, Qty, InvoicePrice) VALUES (1, @orderId, 1, 3499.00);\n\n")
        
        # Order 2
        f.write("INSERT INTO TbSalesInvoices (InvoiceDate, DelivryDate, CustomerId, Notes, CreatedBy, CreatedDate, CurrentState, UpdatedDate, UpdatedBy) VALUES\n")
        f.write("(DATEADD(day, -4, GETDATE()), DATEADD(day, 1, GETDATE()), @u2, 'Address: 456 Tech Park, San Jose | Phone: 555-5678', @u2, DATEADD(day, -4, GETDATE()), 2, DATEADD(day, -2, GETDATE()), '1');\n")
        f.write("SET @orderId = SCOPE_IDENTITY();\n")
        f.write("INSERT INTO TbSalesInvoiceItems (ItemId, InvoiceId, Qty, InvoicePrice) VALUES (3, @orderId, 1, 1999.00);\n\n")

        # Order 3
        f.write("INSERT INTO TbSalesInvoices (InvoiceDate, DelivryDate, CustomerId, Notes, CreatedBy, CreatedDate, CurrentState) VALUES\n")
        f.write("(DATEADD(day, -2, GETDATE()), DATEADD(day, 3, GETDATE()), @u3, 'Address: 789 Cloud Ave, Oakland | Phone: 555-9012', @u3, DATEADD(day, -2, GETDATE()), 1);\n")
        f.write("SET @orderId = SCOPE_IDENTITY();\n")
        f.write("INSERT INTO TbSalesInvoiceItems (ItemId, InvoiceId, Qty, InvoicePrice) VALUES (5, @orderId, 1, 1749.00);\n\n")

        # Order 4
        f.write("INSERT INTO TbSalesInvoices (InvoiceDate, DelivryDate, CustomerId, Notes, CreatedBy, CreatedDate, CurrentState) VALUES\n")
        f.write("(GETDATE(), DATEADD(day, 5, GETDATE()), @u1, 'Address: 123 Market St, San Francisco | Phone: 555-1234', @u1, GETDATE(), 1);\n")
        f.write("SET @orderId = SCOPE_IDENTITY();\n")
        f.write("INSERT INTO TbSalesInvoiceItems (ItemId, InvoiceId, Qty, InvoicePrice) VALUES (11, @orderId, 1, 2999.00);\n\n")

        # Order 5
        f.write("INSERT INTO TbSalesInvoices (InvoiceDate, DelivryDate, CustomerId, Notes, CreatedBy, CreatedDate, CurrentState, UpdatedDate, UpdatedBy) VALUES\n")
        f.write("(DATEADD(day, -15, GETDATE()), DATEADD(day, -10, GETDATE()), @u2, 'Address: 99 Broadway, New York | Phone: 555-3344 | Coupon: LUNAR25 (25% off)', @u2, DATEADD(day, -15, GETDATE()), 3, DATEADD(day, -10, GETDATE()), '1');\n")
        f.write("SET @orderId = SCOPE_IDENTITY();\n")
        f.write("INSERT INTO TbSalesInvoiceItems (ItemId, InvoiceId, Qty, InvoicePrice) VALUES (2, @orderId, 1, 824.25);\n\n")
        
        # 10. Seed sample reviews
        f.write("INSERT INTO TbItemEvaluations (ItemId, CustomerName, CustomerEmail, Rating, ReviewText, CreatedDate) VALUES\n")
        f.write("(1, 'John Doe', 'john@example.com', 5, 'Absolutely incredible laptop. The M3 Max chip compiles code instantly and the screen is beautiful.', DATEADD(day, -5, GETDATE())),\n")
        f.write("(3, 'Jane Smith', 'jane@example.com', 4, 'Very solid laptop. The OLED screen is breathtaking, but it runs a bit warm when multitasking.', DATEADD(day, -3, GETDATE())),\n")
        f.write("(6, 'Alex Mercer', 'alex@example.com', 5, 'Absolute gaming monster! High FPS on Ultra settings. Highly recommended.', DATEADD(day, -1, GETDATE())),\n")
        f.write("(7, 'Sarah Connor', 'sarah@example.com', 5, 'Best ultrabook I have owned. The keyboard is crisp and battery life lasts a full workday.', DATEADD(day, -2, GETDATE()));\n")

    print("SQL seed script written successfully.")

if __name__ == "__main__":
    items = generate_laptops()
    write_sql_script(items)
