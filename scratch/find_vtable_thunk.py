import pefile
import struct

pe = pefile.PE(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe')
data = open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe', 'rb').read()

# Let's find the start of the thunk
# sub dword ptr [esp+4], ... ; jmp ...
# The sub instruction was around 0x5edd0 - 0x5edd6
thunk_va = 0x45edce # let's check exact bytes
idx = data.find(b'\x81\x6c\x24\x04\x5f\x00\x00\x00\xe9')
print(f"Thunk at raw offset 0x{idx:x}, VA: 0x{idx + 0x400000:x}")
thunk_va = idx + 0x400000

# Search for thunk_va in entire binary:
target = struct.pack('<I', thunk_va)
matches = []
for i in range(len(data) - 4):
    if data[i:i+4] == target:
        matches.append(i)

print(f"Matches for thunk_va in binary: {[hex(x) for x in matches]}")
