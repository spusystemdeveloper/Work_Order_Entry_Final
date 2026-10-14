import struct

with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe', 'rb') as f:
    data = f.read()

target = struct.pack('<I', 0x86a960)
idx = 0
while True:
    idx = data.find(target, idx)
    if idx == -1:
        break
    print(f"Found pointer to 0x86a960 at raw 0x{idx:x}")
    idx += 4
