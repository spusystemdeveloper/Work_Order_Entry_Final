import struct

with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    data = f.read()

target = struct.pack('<I', 317)
idx = 0
found = []
while True:
    idx = data.find(target, idx)
    if idx == -1:
        break
    found.append(idx)
    idx += 4

print(f"Found 317 (0x13d) at raw offsets: {[hex(x) for x in found]}")
