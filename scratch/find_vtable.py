# Let's find the vtable of Transaction in QSRules.dll
with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    data = f.read()

# Let's search for statuscodeenum or find the vtable pointer
# We know at 0x11053b2c there is an IID or class descriptor
# Let's check what is at 0x11053b2c - ImageBase = 0x53b2c in raw data
rva = 0x53b2c
print("Data at 0x53b2c:", data[rva:rva+32].hex())

# Let's also check 0x1104fee0 - ImageBase = 0x4fee0
print("Data at 0x4fee0:", data[0x4fee0:0x4fee0+32].hex())

# Let's search for method names or GUIDs in QSRules.dll
# 0x170 = 368 decimal -> 368 / 4 = 92nd method!
