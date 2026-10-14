import pefile
import re

with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    data = f.read()

print(f"File size: {len(data)}")

# Let's find occurrences of statusValidationTenderOverTendered
idx = 0
while True:
    idx = data.find(b'statusValidationTenderOverTendered', idx)
    if idx == -1:
        break
    print(f"Found statusValidationTenderOverTendered at offset 0x{idx:x}")
    idx += len(b'statusValidationTenderOverTendered')
