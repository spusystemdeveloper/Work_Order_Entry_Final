import re
from collections import Counter
from pathlib import Path
import sys
import pdfplumber
base=Path(sys.argv[1])
checks=0
def check(value, label):
    global checks
    assert value, label
    checks+=1

def words(path):
    with pdfplumber.open(path) as pdf:
        return [[(w['text'],round(w['x0'],2),round(w['top'],2),round(w['x1'],2),round(w['bottom'],2)) for w in p.extract_words()] for p in pdf.pages]
for version in ('V4','V8'):
    folder=base/(version+'Render')
    for after,before in [('NoRates','BaselineNoRates'),('Quotation','BaselineQuotation')]:
        check(words(folder/(after+'.pdf')) == words(folder/(before+'.pdf')), version+' '+after+' every existing word/position/page unchanged')
    baseline_words=Counter(w[0] for w in sum(words(folder/'BaselineNoRates.pdf'),[]))
    for name,rates in [('AllRates',(1,2,5,15)),('Only15',(15,)),('AllRatesPreview',(1,2,5,15)),('LargeValues',(1,2,5,15)),('MultiPage',(1,2,5,15)),('Empty',(1,2,5,15))]:
        with pdfplumber.open(folder/(name+'.pdf')) as pdf:
            texts=[p.extract_text() or '' for p in pdf.pages]
            text='\n'.join(texts)
            check(text.count('Withholding Tax (Reference)')==1,version+' '+name+' heading appears once')
            check(tuple(int(r) for r in re.findall(r'\b(1|2|5|15)%:',text))==rates,version+' '+name+' all and only enabled rates')
            page=next(p for p,t in zip(pdf.pages,texts) if 'Withholding Tax (Reference)' in t)
            pagewords=page.extract_words()
            heading=next(w for w in pagewords if w['text']=='Withholding')
            picked=next((w for w in pagewords if w['text']=='Picked'),None)
            ratewords=[w for w in pagewords if re.fullmatch('(1|2|5|15)%:',w['text'])]
            check(all(w['top']>=heading['bottom'] for w in ratewords),version+' '+name+' amounts below heading')
            if picked:
                check(max(w['bottom'] for w in ratewords)<picked['top'],version+' '+name+' reference clear of signatures')
            # V4's existing narrow total box wraps very large numbers. Use PDF
            # character order to verify the full value across the wrapped lines.
            characters=''.join(c['text'] for p in pdf.pages for c in p.chars)
            check('123,456,789.00' in characters if name=='LargeValues' else '1,100.00' in characters,version+' '+name+' unchanged total rendered')
            if name in ('AllRates','Only15'):
                rendered=Counter(w[0] for w in sum(words(folder/(name+'.pdf')),[]))
                check(not (baseline_words-rendered),version+' '+name+' existing report text and total values retained')
    with pdfplumber.open(folder/'BaselineLargeValues.pdf') as pdf:
        baseline_characters=''.join(c['text'] for p in pdf.pages for c in p.chars)
    with pdfplumber.open(folder/'LargeValues.pdf') as pdf:
        current_characters=''.join(c['text'] for p in pdf.pages for c in p.chars)
    check(baseline_characters.count('123,456,789.00') == current_characters.count('123,456,789.00') == 2, version+' large existing subtotal/total preserve every digit despite legacy wrapping')
    with pdfplumber.open(folder/'AllRates.pdf') as pdf:
        text='\n'.join(p.extract_text() or '' for p in pdf.pages)
        check(all(s in text for s in ('1%: 9.82','2%: 19.64','5%: 49.11','15%: 147.32')), version+' rendered exact amounts')
print(f'PASS: {checks} PDF text/layout comparisons across both versions')
