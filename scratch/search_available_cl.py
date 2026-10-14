import os

for root, dirs, files in os.walk(r'.\Work Order Entry'):
    for f in files:
        if f.endswith('.vb'):
            p = os.path.join(root, f)
            with open(p, encoding='latin-1') as fp:
                txt = fp.read()
                if 'txtAvailable' in txt or 'available credit' in txt.lower() or 'credit limit' in txt.lower():
                    lines = txt.splitlines()
                    for idx, l in enumerate(lines):
                        if any(k in l.lower() for k in ['txtavailable', 'creditlimit', 'credit limit', 'exceed']):
                            if not l.strip().startswith("'"):
                                print(f"{f}:{idx+1} {l.strip()}")
