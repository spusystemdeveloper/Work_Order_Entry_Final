import capstone

with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe', 'rb') as f:
    data = f.read()

start_raw = 0x200330
end_raw = 0x200500
code = data[start_raw:end_raw]

base_va = 0x400000 + start_raw

md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_32)
for ins in md.disasm(code, base_va):
    print(f"0x{ins.address:x}: {ins.mnemonic:8} {ins.op_str}")
