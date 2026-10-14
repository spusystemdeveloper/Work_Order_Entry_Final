import re

with open(r'C:\Users\ADMIN\source\repos\PROJECT\REFERENCES\rms-customization-guide\QSRules_Classes.htm', encoding='latin-1') as f:
    text = f.read()

links = re.findall(r'<a href="([^"]+)">([^<]+)</a>', text)
for link, name in links:
    print(f'{name}: {link}')
