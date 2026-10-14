import zipfile
import xml.etree.ElementTree as ET

with zipfile.ZipFile(r'C:\Users\ADMIN\source\repos\Queries\ENLIVO_AND_AMBER_CONSOLIDATED.xlsx', 'r') as z:
    print("Files in zip:", z.namelist()[:10])
    # Read workbook.xml to get sheet names
    wb_xml = z.read('xl/workbook.xml')
    root = ET.fromstring(wb_xml)
    sheets = [s.attrib['name'] for s in root.iter('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}sheet')]
    print("Sheets:", sheets)
    
    # Read shared strings
    shared_strings = []
    if 'xl/sharedStrings.xml' in z.namelist():
        ss_xml = z.read('xl/sharedStrings.xml')
        ss_root = ET.fromstring(ss_xml)
        for si in ss_root.iter('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}si'):
            t = "".join(node.text for node in si.iter('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}t') if node.text)
            shared_strings.append(t)
    print(f"Total shared strings: {len(shared_strings)}")
    print("Sample shared strings:", shared_strings[:15])
