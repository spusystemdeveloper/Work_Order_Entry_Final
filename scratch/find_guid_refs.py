with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    data = f.read()

# Let's find the vtable pointer for Transaction
# In QSRules.dll, where is the Transaction class vtable?
# We know 0x1104fee0 is Transaction GUID
# Let's search for references to 0x1104fee0 in QSRules.dll
import struct
target = struct.pack('<I', 0x1104fee0)
idx = 0
while True:
    idx = data.find(target, idx)
    if idx == -1:
        break
    print(f"Found Transaction GUID ref at 0x{idx:x}")
    idx += 4
