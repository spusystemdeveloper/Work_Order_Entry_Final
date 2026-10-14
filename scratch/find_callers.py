import struct

with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe', 'rb') as f:
    data = f.read()

target_va = 0x86a960
# In x86, call rel32 is E8 [rel32]
# rel32 = target_va - (call_va + 5)
# Let's search all E8 instructions
import pefile
pe = pefile.PE(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe')
image_base = pe.OPTIONAL_HEADER.ImageBase

calls = []
for sec in pe.sections:
    sec_data = sec.get_data()
    sec_raw = sec.PointerToRawData
    sec_va = sec.VirtualAddress + image_base
    for i in range(len(sec_data) - 5):
        if sec_data[i] == 0xe8:
            rel = struct.unpack('<i', sec_data[i+1:i+5])[0]
            call_site_va = sec_va + i
            dest_va = call_site_va + 5 + rel
            if dest_va == target_va:
                calls.append((call_site_va, sec_raw + i))

print(f"Found {len(calls)} calls to 0x{target_va:x}:")
for va, raw in calls:
    print(f"  VA: 0x{va:x}, raw offset: 0x{raw:x}")
