with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    data = f.read()

# Let's search for "FROM Tender" or "[PreventOverTendering]"
for s in [b"FROM Tender", b"FROM [Tender]", b"PreventOverTendering"]:
    idx = 0
    while True:
        idx = data.lower().find(s.lower(), idx)
        if idx == -1:
            break
        print(f"Found {s} at 0x{idx:x}")
        start = max(0, idx - 50)
        end = min(len(data), idx + 100)
        print(data[start:end])
        idx += len(s)
