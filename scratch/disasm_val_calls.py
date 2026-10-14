import capstone

with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe', 'rb') as f:
    data = f.read()

calls = [0x13fc8b, 0x1ee09e, 0x1fa1c9, 0x1faa89, 0x20034d, 0x40679b, 0x486f39, 0x4d6407]
md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_32)

for c in calls:
    start = max(0, c - 30)
    end = min(len(data), c + 50)
    print(f"\n--- Call at raw 0x{c:x} (VA 0x{c + 0x400000:x}) ---")
    for ins in md.disasm(data[start:end], start + 0x400000):
        print(f"0x{ins.address:x}: {ins.mnemonic:8} {ins.op_str}")
