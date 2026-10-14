import zipfile, xml.etree.ElementTree as ET

with zipfile.ZipFile(r'C:\Users\ADMIN\source\repos\Queries\ENLIVO_AND_AMBER_CONSOLIDATED.xlsx', 'r') as z:
    wb = ET.fromstring(z.read('xl/workbook.xml'))
    sheet_map = {s.attrib['name']: s.attrib['{http://schemas.openxmlformats.org/officeDocument/2006/relationships}id'] 
                 for s in wb.iter('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}sheet')}
    rels = ET.fromstring(z.read('xl/_rels/workbook.xml.rels'))
    rel_map = {r.attrib['Id']: r.attrib['Target'] for r in rels.iter('{http://schemas.openxmlformats.org/package/2006/relationships}Relationship')}
    shared_strings = []
    if 'xl/sharedStrings.xml' in z.namelist():
        ss_root = ET.fromstring(z.read('xl/sharedStrings.xml'))
        for si in ss_root.iter('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}si'):
            t = ''.join(node.text for node in si.iter('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}t') if node.text)
            shared_strings.append(t)

    target_sheet = 'AMBER'
    sheet_root = ET.fromstring(z.read('xl/' + rel_map[sheet_map[target_sheet]]))
    
    rows = []
    for r in sheet_root.iter('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}row'):
        row_vals = {}
        for c in r.iter('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}c'):
            ref = c.attrib.get('r')
            col = ''.join(filter(str.isalpha, ref))
            v = c.find('{http://schemas.openxmlformats.org/spreadsheetml/2006/main}v')
            t = c.attrib.get('t')
            val = v.text if v is not None else ''
            if t == 's' and val:
                val = shared_strings[int(val)]
            row_vals[col] = val
        if 'A' in row_vals and row_vals['A'] != 'Customer':
            rows.append(row_vals)

    rows.sort(key=lambda x: float(x.get('J', 0) or 0), reverse=True)
    print('Top 10 highest sales rows in Excel AMBER:')
    for r in rows[:10]:
        print(f"Customer: {r.get('A')}, Item: {r.get('B')}, Qty: {r.get('I')}, Sales: {float(r.get('J', 0)):,.2f}, Rep: {r.get('K')}")
