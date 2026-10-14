with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    data = f.read()

# Let's find all occurrences of "PreventOverTendering"
idx = 0
while True:
    idx = data.find(b'PreventOverTendering', idx)
    if idx == -1:
        break
    print(f"Found at 0x{idx:x}")
    # print context
    start = max(0, idx - 50)
    end = min(len(data), idx + 100)
    print(data[start:end])
    idx += 20
