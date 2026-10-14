# Let's inspect what is compared before 0x329f49
with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    data = f.read()

# Let's look at bytes from 0x329d00 to 0x32a000
chunk = data[0x329d00:0x32a000]
print(f"Len: {len(chunk)}")

# We can search for floating point comparisons or currency comparisons:
# fcomp, fcompp, fcom, fld, fstp, or integer cmp
# Let's print out what methods are called (ff 51, ff 52, ff 90, ff 91, ff 92, ff 15)
import re
for i in range(0x329d00, 0x329f60):
    b = data[i]
    if b == 0xd8 or b == 0xd9 or b == 0xda or b == 0xdb or b == 0xdc or b == 0xdd or b == 0xde or b == 0xdf:
        print(f"FPU at 0x{i:x}: {data[i:i+6].hex()}")
    elif data[i:i+2] == b'\x0f\x84' or data[i:i+2] == b'\x0f\x85' or data[i:i+2] == b'\x0f\x8c' or data[i:i+2] == b'\x0f\x8d' or data[i:i+2] == b'\x0f\x8e' or data[i:i+2] == b'\x0f\x8f':
        print(f"Jcc at 0x{i:x}: {data[i:i+6].hex()}")
