with open(r'c:\Users\ADMIN\source\repos\WORK ORDER ENTRY_ASOF_15SEPT2026\scratch\session_members.txt', 'r', encoding='utf-16le') as f:
    lines = f.readlines()

for line in lines:
    if any(k in line.lower() for k in ['trans', 'tender', 'over', 'valid', 'pay']):
        print(line.strip())
