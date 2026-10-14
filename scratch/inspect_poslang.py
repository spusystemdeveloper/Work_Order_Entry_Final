with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\POSLanguage.DAT', 'rb') as f:
    text = f.read().decode('latin-1')

for line in text.splitlines():
    if 'Other:' in line or 'Other' in line:
        print(line)
