with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    data = f.read()

offsets = [0x14dc3b, 0x14df7b, 0x14f91e, 0x151f90, 0x17c807, 0x25de05, 0x2d71b1, 0x329f49, 0x32a27e, 0x38a71a]

for off in offsets:
    chunk = data[off-40:off+40]
    print(f"\nOffset 0x{off:x}:")
    print(" ".join(f"{b:02x}" for b in chunk))
