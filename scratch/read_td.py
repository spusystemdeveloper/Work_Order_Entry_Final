import re

path = r'C:\Users\ADMIN\source\repos\PROJECT\REFERENCES\rms-customization-guide\TenderDescriptor\TenderDescriptor_Class.htm'
with open(path, encoding='latin-1') as f:
    text = f.read()

links = re.findall(r'<a href="([^"]+)">([^<]+)</a>', text)
for link, name in links:
    print(f'{name}: {link}')
