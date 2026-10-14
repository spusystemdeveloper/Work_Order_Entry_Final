with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    data = f.read()

start = 0x5f000
end = 0x60000
chunk = data[start:end]
# Extract all unicode strings
import re
u_strings = re.findall(b'(?:[\x20-\x7e]\x00){4,}', chunk)
for s in u_strings:
    print(s.decode('utf-16le'))
