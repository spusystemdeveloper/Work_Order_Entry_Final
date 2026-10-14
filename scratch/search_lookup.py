with open(r'.\Work Order Entry\Forms\frmItemLookUp.vb', encoding='latin-1') as f:
    lines = f.readlines()

for i, line in enumerate(lines):
    if any(k in line for k in ['CreditLimit', 'AccountBalance', 'getCreditLimit', 'getCustomerCreditLimit', 'getOpenWO', 'exceed']):
        print(f"Line {i+1}: {line.strip()}")
