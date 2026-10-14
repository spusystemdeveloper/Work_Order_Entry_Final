import os

keywords = ['SOD_fn_GetCreditLimit', 'SOD_fn_GetCL', 'CreditLimit', 'AccountBalance', 'AvailableCL', 'UpdateCustomerAccountBalance']

for root, dirs, files in os.walk('.'):
    for f in files:
        if f.endswith('.vb') and not f.endswith('.Designer.vb'):
            p = os.path.join(root, f)
            try:
                with open(p, encoding='utf-8') as fp:
                    txt = fp.read()
            except:
                with open(p, encoding='latin-1') as fp:
                    txt = fp.read()
            for kw in keywords:
                if kw.lower() in txt.lower():
                    print(f"Found '{kw}' in {p}")
