import os
import re

def dump_html(rel_path):
    path = os.path.join(r'C:\Users\ADMIN\source\repos\PROJECT\REFERENCES\rms-customization-guide', rel_path)
    with open(path, encoding='latin-1') as f:
        text = f.read()
    links = re.findall(r'<a href="([^"]+)">([^<]+)</a>', text)
    for href, text_link in links:
        print(f"  {text_link} -> {href}")

print("Tender methods/properties:")
dump_html('Tender/Tender_Class.htm')
print("\nTransaction methods/properties:")
dump_html('Transaction/Transaction_Class.htm')
