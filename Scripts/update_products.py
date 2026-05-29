import pyodbc
import os

def main():
    print("LapShop Product Data Populator starting...")
    
    # Connection string to local SQL Server using Windows Authentication
    # Tries standard drivers
    conn_str = ""
    drivers = [
        "{ODBC Driver 17 for SQL Server}",
        "{SQL Server}",
        "{ODBC Driver 18 for SQL Server}"
    ]
    
    conn = None
    for driver in drivers:
        try:
            conn_str = f"Driver={driver};Server=localhost;Database=LapShop;Trusted_Connection=yes;TrustServerCertificate=yes;"
            conn = pyodbc.connect(conn_str)
            print(f"Connected successfully using driver: {driver}")
            break
        except Exception as e:
            continue
            
    if not conn:
        print("Failed to connect to local SQL Server. Please check that SQL Server is running on 'localhost' and database 'LapShop' exists.")
        return
        
    cursor = conn.cursor()
    
    try:
        # Get category names
        cursor.execute("SELECT CategoryId, CategoryName FROM TbCategories")
        categories = {row[0]: row[1] for row in cursor.fetchall()}
        
        # Get all products
        cursor.execute("SELECT ItemId, ItemName, CategoryId, Processor, RamSize, HardDisk, Gpu, ScreenSize, Description, ImageName FROM TbItems")
        items = cursor.fetchall()
        
        print(f"Loaded {len(items)} products from database.")
        
        updated_count = 0
        for item in items:
            item_id = item[0]
            item_name = item[1]
            category_id = item[2]
            processor = item[3] or "Intel/AMD"
            ram_size = item[4] or 8
            hard_disk = item[5] or "high-speed SSD"
            gpu = item[6] or "Intel Iris Xe Graphics"
            screen_size = item[7] or "15.6-inch"
            desc = item[8]
            image_name = item[9]
            
            modified = False
            new_image = image_name
            new_desc = desc
            
            # 1. Assign correct image filename
            if not image_name or image_name == "silver_ultrabook.png" or image_name.lower().endswith(".jpg"):
                if category_id == 1: # Apple
                    new_image = "apple_macbook.png"
                elif category_id in [4, 8, 14]: # ASUS, MSI, Razer
                    new_image = "gaming_laptop.png"
                elif category_id == 5: # Dell
                    new_image = "dark_laptop.png"
                else: # HP, Lenovo, etc.
                    new_image = "silver_ultrabook.png" if item_id % 2 == 0 else "dark_laptop.png"
                modified = True
                
            # 2. Generate professional description if empty
            if not desc or len(desc) < 10:
                brand = categories.get(category_id, "Premium")
                new_desc = f"This premium {brand} {item_name} is a powerful laptop engineered for modern computing. It is powered by a high-performance {processor} processor, equipped with {ram_size}GB of RAM, and features {hard_disk} storage. Graphics are handled by {gpu}, displaying beautiful visuals on its {screen_size} screen. Perfect for developers, creators, and business professionals seeking reliability and speed on the go."
                modified = True
                
            if modified:
                cursor.execute(
                    "UPDATE TbItems SET Description = ?, ImageName = ? WHERE ItemId = ?",
                    (new_desc, new_image, item_id)
                )
                updated_count += 1
                
        conn.commit()
        print(f"Successfully updated {updated_count} products with real images and descriptions in database.")
        
    except Exception as e:
        print(f"An error occurred: {e}")
        conn.rollback()
    finally:
        conn.close()

if __name__ == "__main__":
    main()
