import pefile
import struct

pe = pefile.PE(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe')
data = open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe', 'rb').read()

target_va = 0x45edd6
image_base = pe.OPTIONAL_HEADER.ImageBase

for s in pe.sections:
    if b'.text' in s.Name:
        text_sec = s
        break

text_data = text_sec.get_data()
text_va = text_sec.VirtualAddress + image_base

for i in range(len(text_data) - 5):
    b = text_data[i]
    if b in [0xe8, 0xe9]: # call or jmp
        rel = struct.unpack('<i', text_data[i+1:i+5])[0]
        cur_va = text_va + i
        dest_va = cur_va + 5 + rel
        if dest_va == target_va:
            print(f"Found call at VA: 0x{cur_va:x} (raw: 0x{text_sec.PointerToRawData + i:x})")
