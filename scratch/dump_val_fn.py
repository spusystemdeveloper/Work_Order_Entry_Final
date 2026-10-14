with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    data = f.read()

offset = 0x329f49
prologues = []
for i in range(offset - 1, max(0, offset - 10000), -1):
    if data[i:i+3] == b'\x55\x8b\xec':
        prologues.append(i)

print(f"Prologues found: {[hex(x) for x in prologues[:5]]}")
