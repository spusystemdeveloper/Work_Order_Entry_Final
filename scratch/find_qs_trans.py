with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', 'rb') as f:
    data = f.read()

import pefile
pe = pefile.PE(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll')
image_base = pe.OPTIONAL_HEADER.ImageBase

# Let's search for "Transaction" class GUID or CLSID or string in QSRules.dll
# CLSID for QSRules.Transaction
# Let's find "QSRules.Transaction" in data
idx = data.find(b'QSRules.Transaction\x00')
print(f"'QSRules.Transaction' at 0x{idx:x}")

# Let's search for statusValidationTenderOverTendered in QSRules.dll:
# Earlier we found: 0x4253d4, 0x43a2da
# Let's see what is referencing statusValidationTenderOverTendered or where the validation logic is.
# In QSRules, what function checks over-tendering?
