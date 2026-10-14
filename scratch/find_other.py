with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    dll_data = f.read()

# Let's search for "Other:" in QSRules.dll or SOPOSUSER.exe
idx = 0
while True:
    idx = dll_data.find(b'Other:', idx)
    if idx == -1:
        break
    print(f"Found 'Other:' in QSRules.dll at 0x{idx:x}")
    idx += 6

with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe', 'rb') as f:
    exe_data = f.read()

idx = 0
while True:
    idx = exe_data.find(b'Other:', idx)
    if idx == -1:
        break
    print(f"Found 'Other:' in SOPOSUSER.exe at 0x{idx:x}")
    idx += 6
