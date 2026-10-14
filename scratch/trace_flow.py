import capstone

with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    data = f.read()

# Let's inspect the entire function 0x11329530 to 0x1132a400
# and extract all method calls and logic
md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_32)

start = 0x329530
end = 0x32a400
base = 0x11000000 + start

instructions = list(md.disasm(data[start:end], base))

# Let's filter key parts
for ins in instructions:
    # print calls, jcc, and comparisons
    if ins.mnemonic.startswith('j') or ins.mnemonic.startswith('call') or ins.mnemonic.startswith('cmp') or ins.mnemonic.startswith('fld') or '0x13d' in ins.op_str:
        print(f"0x{ins.address:x}: {ins.mnemonic:8} {ins.op_str}")
