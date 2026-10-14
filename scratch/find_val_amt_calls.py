with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe', 'rb') as f:
    data = f.read()

# Search for calls with displacement 0x130
import re
# ff [90-97] 30 01 00 00
matches = [m.start() for m in re.finditer(b'\xff[\x90-\x97]\x30\x01\x00\x00', data)]
print(f"Found {len(matches)} calls to [reg + 0x130]: {[hex(x) for x in matches]}")
