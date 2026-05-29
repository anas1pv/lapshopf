import re

# Read categories, OS, and item types inserted in the script
os_ids = {1, 2, 3, 4, 5}
type_ids = {1, 2, 3, 4}
cat_ids = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}

with open("c:/Users/anasa/source/repos/lapshop/seed_1000_items.sql", "r", encoding="utf-8") as f:
    content = f.read()

# Let's find all lines containing INSERT INTO TbItems
# and check the lines after it until a semicolon
lines = content.splitlines()
in_items = False
item_lines = []

for line in lines:
    if "INSERT INTO TbItems" in line:
        in_items = True
        continue
    if in_items:
        if ";" in line:
            item_lines.append(line.split(";")[0])
            in_items = False
        else:
            item_lines.append(line)

print("Collected", len(item_lines), "item lines.")

# Each line looks like: (1, 'name', salesPrice, purchasePrice, categoryId, 'image', currentState, GETDATE(), 'createdBy', 'desc', 'gpu', 'disk', itemTypeId, 'processor', ramSize, 'screenRes', 'screenSize', 'weight', osId),
errors = []
item_count = 0

for line in item_lines:
    line = line.strip()
    if not line:
        continue
    if line.endswith(","):
        line = line[:-1]
    
    # Strip opening and closing parentheses
    if line.startswith("("):
        line = line[1:]
    if line.endswith(")"):
        line = line[:-1]
        
    # We want to split by comma, but commas inside strings should be ignored.
    # A simple parser to split by comma outside quotes
    parts = []
    current = []
    in_quote = False
    for char in line:
        if char == "'":
            in_quote = not in_quote
            current.append(char)
        elif char == "," and not in_quote:
            parts.append("".join(current).strip())
            current = []
        else:
            current.append(char)
    parts.append("".join(current).strip())
    
    if len(parts) < 19:
        # Some lines might be split or wrapped, let's skip them or print warning
        # print("Skipping short line:", len(parts), line[:50])
        continue
        
    item_count += 1
    
    try:
        item_id = int(parts[0])
        item_name = parts[1]
        cat_id = int(parts[4])
        type_id = int(parts[12])  # Part 12 is ItemTypeId
        os_id = int(parts[18])    # Part 18 is OsId
        
        if cat_id not in cat_ids:
            errors.append(f"Item {item_id} ({item_name}) has invalid CategoryId: {cat_id}")
        if type_id not in type_ids:
            errors.append(f"Item {item_id} ({item_name}) has invalid ItemTypeId: {type_id}")
        if os_id not in os_ids:
            errors.append(f"Item {item_id} ({item_name}) has invalid OsId: {os_id}")
    except Exception as e:
        print("Error parsing line:", e, parts[:6])

print(f"Total items analyzed: {item_count}")
print(f"Total foreign key errors: {len(errors)}")
for e in errors[:20]:
    print(e)
