import os

search_terms = [b"following tender types", b"Overtendering", b"Adjust the payment amounts"]

root_dir = r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations'
for root, dirs, files in os.walk(root_dir):
    for file in files:
        filepath = os.path.join(root, file)
        try:
            with open(filepath, 'rb') as f:
                content = f.read()
                for term in search_terms:
                    # check ascii
                    if term.lower() in content.lower():
                        print(f"Match '{term.decode()}' in {filepath}")
                    # check utf-16le
                    term_u16 = term.decode().encode('utf-16le')
                    # case-insensitive utf-16 is harder, let's check exact or case-insensitive:
                    if term_u16 in content:
                        print(f"Match utf-16 '{term.decode()}' in {filepath}")
        except Exception as e:
            pass
