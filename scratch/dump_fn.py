with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe', 'rb') as f:
    data = f.read()

fn_data = data[0x46a960:0x46ad00]

# Let's inspect calls and virtual calls
# In PE, image base is usually 0x400000 or from PE header
import pefile
pe = pefile.PE(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe')
image_base = pe.OPTIONAL_HEADER.ImageBase

# Map file offset to RVA / VA
for section in pe.sections:
    start_raw = section.PointerToRawData
    end_raw = start_raw + section.SizeOfRawData
    if start_raw <= 0x46a960 < end_raw:
        rva = 0x46a960 - start_raw + section.VirtualAddress
        va = rva + image_base
        print(f"Function VA: 0x{va:x}, ImageBase: 0x{image_base:x}")
        break

# Let's see what imports or calls are made
# We can decode the bytes around 0x46a960:
print("Hexdump:")
for i in range(0, len(fn_data), 16):
    chunk = fn_data[i:i+16]
    print(f"{0x46a960 + i:06x}: " + " ".join(f"{b:02x}" for b in chunk))
