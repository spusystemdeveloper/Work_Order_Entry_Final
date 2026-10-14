import zipfile, xml.etree.ElementTree as ET
from collections import defaultdict
NS = '{http://schemas.openxmlformats.org/spreadsheetml/2006/main}'
R = '{http://schemas.openxmlformats.org/officeDocument/2006/relationships}id'
with zipfile.ZipFile(r'C:\Users\ADMIN\source\repos\Queries\ENLIVO_AND_AMBER_CONSOLIDATED.xlsx') as z:
    wb = ET.fromstring(z.read('xl/workbook.xml'))
    smap = {s.attrib['name']: s.attrib[R] for s in wb.iter(NS + 'sheet')}
    rels = ET.fromstring(z.read('xl/_rels/workbook.xml.rels'))
    rmap = {r.attrib['Id']: r.attrib['Target'] for r in rels}
    ss = [''.join(t.text or '' for t in si.iter(NS + 't')) for si in ET.fromstring(z.read('xl/sharedStrings.xml')).iter(NS + 'si')]
    for sheet in ['AMBER', 'ENLIVO']:
        root = ET.fromstring(z.read('xl/' + rmap[smap[sheet]]))
        groups = defaultdict(list)
        for row in root.iter(NS + 'row'):
            vals = {}
            for c in row.iter(NS + 'c'):
                col = ''.join(ch for ch in c.attrib['r'] if ch.isalpha())
                v = c.find(NS + 'v')
                val = v.text if v is not None else ''
                if c.attrib.get('t') == 's' and val:
                    val = ss[int(val)]
                vals[col] = val
            if vals.get('A') and vals['A'] != 'Customer':
                groups[(vals['A'], vals.get('B'))].append(vals.get('K'))
        d = {k: v for k, v in groups.items() if len(v) > 1}
        nullrep = sum(1 for v in groups.values() for x in v if x in ('NULL', '', None))
        print(f'{sheet}: dup customer+item groups={len(d)}, rows with NULL SalesRep={nullrep}')
        for k, v in list(d.items())[:5]:
            print('  ', k, '->', [x.encode('ascii', 'replace').decode() for x in v])
