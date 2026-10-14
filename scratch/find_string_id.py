import struct

def search_id(filename, string_id):
    with open(filename, 'rb') as f:
        data = f.read()
    
    # 3970 as 16-bit little-endian: \x82\x0f
    target16 = struct.pack('<H', string_id)
    # 3970 as 32-bit little-endian: \x82\x0f\x00\x00
    target32 = struct.pack('<I', string_id)
    
    print(f"Searching in {filename}...")
    for target, name in [(target16, '16-bit'), (target32, '32-bit')]:
        idx = 0
        while True:
            idx = data.find(target, idx)
            if idx == -1:
                break
            print(f"  Found {name} at 0x{idx:x}")
            idx += 1

search_id(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe', 3970)
search_id(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 3970)
