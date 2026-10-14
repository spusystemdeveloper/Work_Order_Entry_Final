import pefile
pe = pefile.PE(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe')
for s in pe.sections:
    print(s.Name.decode().strip('\x00'), hex(s.VirtualAddress), hex(s.SizeOfRawData), hex(s.PointerToRawData))
    if s.PointerToRawData <= 0x46a960 < s.PointerToRawData + s.SizeOfRawData:
        va = pe.OPTIONAL_HEADER.ImageBase + s.VirtualAddress + (0x46a960 - s.PointerToRawData)
        print("MATCHED VA:", hex(va))
