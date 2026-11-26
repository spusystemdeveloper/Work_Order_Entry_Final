"""Verify compact half-sheet layout, fonts, totals and pagination using local PDFs."""
import re,sys
from pathlib import Path
import pdfplumber
base=Path(sys.argv[1]);checks=0
def check(ok,label):
 global checks
 assert ok,label
 checks+=1
for version in ('V4','V8'):
 folder=base/(version+'Render')
 for name in ('NoRates','Quotation'):
  with pdfplumber.open(folder/(name+'.pdf')) as pdf:
   text='\n'.join(p.extract_text() or '' for p in pdf.pages)
   check('Total WTax:' not in text and 'Est.' not in text and 'WTax 1%' not in text,version+' '+name+' reference hidden')
   check('1,100.00' in text,version+' '+name+' original total retained')
 for name,rates in [('AllRates',(1,5)),('Only15',(15,)),('AllRatesPreview',(1,5)),('Charge',(1,5)),('ChargeOnly15',(15,)),('ChargeLargeValues',(1,5)),('ChargeMultiPage',(1,5)),('LargeValues',(1,5)),('MultiPage',(1,5)),('Empty',(1,5)),('SixItems',(1,5)),('SixLongItems',(1,5)),('SixFourRates',(1,2,5,15))]:
  with pdfplumber.open(folder/(name+'.pdf')) as pdf:
   text='\n'.join(p.extract_text() or '' for p in pdf.pages)
   check('#Error' not in text and '<b>' not in text and 'Less:' not in text,version+' '+name+' no errors or markup')
   check(tuple(int(r) for r in re.findall(r'WTax (1|2|5|15)%(?:_Charge)?\s*:',text))==rates,version+' '+name+' only nonzero enabled rates')
   check(not re.search(r'WTax (?:1|2|5|15)%(?:_Charge)?\s*:\s*0\.00',text),version+' '+name+' zeros hidden')
   page=next(p for p in pdf.pages if re.search(r'Total WTax\s*:',p.extract_text() or ''));ww=page.extract_words()
   rows=[w for w in ww if w['text']=='WTax' and any(abs(x['top']-w['top'])<.5 and re.match(r'(1|2|5|15)%',x['text']) for x in ww)]
   total=next(w for w in ww if w['text']=='Total' and any(x['text']=='WTax' and abs(x['top']-w['top'])<.5 for x in ww));est=next(w for w in ww if w['text']=='Est.')
   check(len(rows)==len(rates) and len({round(w['x0'],1) for w in rows})==1,version+' '+name+' one column')
   check(max(w['bottom'] for w in rows)<total['top']<est['top'],version+' '+name+' summary/payment below rates')
   for w in rows:
    chars=[c for c in page.chars if abs(c['top']-w['top'])<.5 and w['x0']-.1<=c['x0']<w['x1']]
    check(chars and all('Bold' not in c['fontname'] for c in chars),version+' '+name+' individual WTax regular')
    nums=[x for x in ww if abs(x['top']-w['top'])<.5 and re.match(r'^-?[\d,]+\.\d{2}$',x['text']) and x['x0']<360]
    check(nums and all('Bold' not in c['fontname'] for x in nums for c in page.chars if abs(c['top']-x['top'])<.5 and x['x0']-.1<=c['x0']<x['x1']),version+' '+name+' individual amounts regular')
   for w in (total,est):check(any('Bold' in c['fontname'] and abs(c['top']-w['top'])<.5 and abs(c['x0']-w['x0'])<.5 for c in page.chars),version+' '+name+' summary/payment bold')
   rightcolons=[x for x in ww if x['text']==':' and 440<x['x0']<475 and any(y['x0']>460 and abs(y['top']-x['top'])<1 and re.match(r'^-?[\d,]+\.\d{2}$',y['text']) for y in ww)]
   check(len(rightcolons)==3 and max(x['x0'] for x in rightcolons)-min(x['x0'] for x in rightcolons)<.5,version+' '+name+' Sub Total/Sales Tax/Order Total colons aligned')
   blockrows=rows+[total,est]
   values=[];colons=[]
   for row in blockrows:
    same=[x for x in ww if abs(x['top']-row['top'])<.5 and x['x0']<350]
    value=next(x for x in same if re.match(r'^-?[\d,]+\.\d{2}$',x['text']));values.append(value)
    colons.append(next(x for x in same if x['text']==':'))
    bold=row is total or row is est
    check(all(('Bold' in c['fontname'])==bold for c in page.chars if abs(c['top']-value['top'])<.5 and value['x0']-.1<=c['x0']<value['x1']),version+' '+name+' amount weight matches label')
   check(max(x['x1'] for x in values)-min(x['x1'] for x in values)<.5,version+' '+name+' all amounts right aligned')
   check(max(x['x0'] for x in colons)-min(x['x0'] for x in colons)<.5,version+' '+name+' all colons aligned')
   signs=[w for w in ww if w['text'] in ('Picked','Checked','Packed')]
   check(len(signs)==3 and max(w['top'] for w in signs)-min(w['top'] for w in signs)<.5,version+' '+name+' signatures in one row')
   check(est['bottom']<min(w['top'] for w in signs),version+' '+name+' no signature overlap')
   check('123,456,789.00' in text if name.endswith('LargeValues') else '1,100.00' in text,version+' '+name+' original total retained')
   if name in ('SixItems','SixFourRates'):
    check(len(pdf.pages)==1,version+' '+name+' six rows fit half-sheet')
    check(all('TEST-'+str(i) in text for i in range(1,7)),version+' '+name+' all six lines retained')
   if name=='SixLongItems':check(all('TEST-'+str(i) in text for i in range(1,7)),version+' long descriptions retained')
   if name in ('MultiPage','ChargeMultiPage'):check('TEST-60' in text,version+' '+name+' all sixty items retained')
print(f'PASS: {checks} compact half-sheet PDF checks across V4/V8; local synthetic rendering only.')
