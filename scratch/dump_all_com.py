import ctypes
from ctypes import wintypes

oleaut32 = ctypes.oledll.oleaut32

pTypeLib = ctypes.c_void_p()
hr = oleaut32.LoadTypeLib(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll\1', ctypes.byref(pTypeLib))

vtable = ctypes.cast(pTypeLib, ctypes.POINTER(ctypes.POINTER(ctypes.c_void_p))).contents
GetTypeInfoCount = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.POINTER(ctypes.c_uint))(vtable[3])
GetTypeInfo = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.c_uint, ctypes.POINTER(ctypes.c_void_p))(vtable[4])
GetDocumentation = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.c_int, ctypes.POINTER(ctypes.c_wchar_p), ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p)(vtable[9])

count = ctypes.c_uint()
GetTypeInfoCount(pTypeLib, ctypes.byref(count))

class GUID(ctypes.Structure):
    _fields_ = [("Data1", wintypes.DWORD), ("Data2", wintypes.WORD), ("Data3", wintypes.WORD), ("Data4", ctypes.c_byte * 8)]

class TYPEATTR(ctypes.Structure):
    _fields_ = [
        ("guid", GUID), ("lcid", wintypes.DWORD), ("dwReserved", wintypes.DWORD),
        ("memidConstructor", wintypes.LONG), ("memidDestructor", wintypes.LONG),
        ("lpstrSchema", ctypes.c_void_p), ("cbSizeInstance", wintypes.ULONG),
        ("typekind", ctypes.c_int), ("cFuncs", wintypes.WORD), ("cVars", wintypes.WORD),
        ("cInterfaces", wintypes.WORD), ("cbSizeVft", wintypes.WORD), ("cbAlignment", wintypes.WORD),
        ("wTypeFlags", wintypes.WORD), ("wMajorVerNum", wintypes.WORD), ("wMinorVerNum", wintypes.WORD),
        ("tdescAlias", ctypes.c_byte * 8), ("idldescType", ctypes.c_byte * 8),
    ]

class FUNCDESC(ctypes.Structure):
    _fields_ = [
        ("memid", wintypes.LONG), ("lprgscode", ctypes.c_void_p), ("lprgelemdescParam", ctypes.c_void_p),
        ("funckind", ctypes.c_int), ("invkind", ctypes.c_int), ("callconv", ctypes.c_int),
        ("cParams", ctypes.c_short), ("cParamsOpt", ctypes.c_short), ("oVft", ctypes.c_short),
        ("wFlags", ctypes.c_short), ("elemdescFunc", ctypes.c_byte * 16)
    ]

results = []
for i in range(count.value):
    pTypeInfo = ctypes.c_void_p()
    GetTypeInfo(pTypeLib, i, ctypes.byref(pTypeInfo))
    tinfo_vtable = ctypes.cast(pTypeInfo, ctypes.POINTER(ctypes.POINTER(ctypes.c_void_p))).contents
    
    GetTypeAttr = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.POINTER(ctypes.POINTER(TYPEATTR)))(tinfo_vtable[3])
    GetFuncDesc = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.c_uint, ctypes.POINTER(ctypes.POINTER(FUNCDESC)))(tinfo_vtable[5])
    GetDoc = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, wintypes.LONG, ctypes.POINTER(ctypes.c_wchar_p), ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p)(tinfo_vtable[12])
    
    bstrName = ctypes.c_wchar_p()
    GetDocumentation(pTypeLib, i, ctypes.byref(bstrName), None, None, None)
    ifname = bstrName.value
    
    pTypeAttr = ctypes.POINTER(TYPEATTR)()
    GetTypeAttr(pTypeInfo, ctypes.byref(pTypeAttr))
    attr = pTypeAttr.contents
    
    for fi in range(attr.cFuncs):
        pFuncDesc = ctypes.POINTER(FUNCDESC)()
        GetFuncDesc(pTypeInfo, fi, ctypes.byref(pFuncDesc))
        fd = pFuncDesc.contents
        fnName = ctypes.c_wchar_p()
        GetDoc(pTypeInfo, fd.memid, ctypes.byref(fnName), None, None, None)
        results.append((ifname, fnName.value, fd.oVft, fd.invkind))

with open(r'c:\Users\ADMIN\source\repos\WORK ORDER ENTRY_ASOF_15SEPT2026\scratch\all_com_methods.txt', 'w', encoding='utf-8') as f:
    for ifname, fnName, oVft, invkind in sorted(results, key=lambda x: (x[0], x[2])):
        f.write(f"{ifname}::0x{oVft:x} ({oVft}): {fnName} (inv={invkind})\n")

print(f"Dumped {len(results)} methods to all_com_methods.txt")
