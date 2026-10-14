import pefile
pe = pefile.PE(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll')
print("QSRules ImageBase:", hex(pe.OPTIONAL_HEADER.ImageBase))
for s in pe.sections:
    print(s.Name.decode().strip('\x00'), hex(s.VirtualAddress), hex(s.SizeOfRawData), hex(s.PointerToRawData))
