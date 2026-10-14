# Let's find the vtable of Transaction and read the function pointer at 0x170
with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    data = f.read()

# Earlier we found references to Transaction GUID (0x1104fee0)
# In COM, class factory or CoCreateInstance initializes the vtable.
# Let's find the vtable of CTransaction (or Transaction class)
# We can find where the vtable pointer is assigned in the constructor of Transaction:
# mov dword ptr [esi], offset ??_7CTransaction...
# Let's search for "mov dword ptr [reg], offset vtable" where vtable is in .rdata or .text
# Or search for references to offset 0x11329530 (ValidateAmount is in Tender vtable!)

# In Tender vtable:
# ValidateAmount is at 0x130 (or similar)
# Let's find references to 0x11329530 in QSRules.dll!
import struct
target = struct.pack('<I', 0x11329530)
idx = 0
while True:
    idx = data.find(target, idx)
    if idx == -1:
        break
    print(f"Found ref to ValidateAmount (0x11329530) at 0x{idx:x}")
    idx += 4
