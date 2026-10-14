import ctypes
from ctypes import wintypes

oleaut32 = ctypes.oledll.oleaut32

# Load typelib for QSRules
pTypeLib = ctypes.c_void_p()
# The typelib is often at index 1: QSRules.dll\\1
hr = oleaut32.LoadTypeLib(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll\1', ctypes.byref(pTypeLib))
if hr != 0:
    hr = oleaut32.LoadTypeLib(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', ctypes.byref(pTypeLib))

print("hr:", hex(hr))

# Let's find _Transaction interface
# GUID: {D223ABDC-2506-4964-A15F-9A1A19D46EDB}
# In ITypeLib: GetTypeInfoOfGuid(REFGUID guid, ITypeInfo **ppTInfo)
# REFGUID is ctypes struct:
class GUID(ctypes.Structure):
    _fields_ = [
        ("Data1", wintypes.DWORD),
        ("Data2", wintypes.WORD),
        ("Data3", wintypes.WORD),
        ("Data4", ctypes.c_byte * 8)
    ]

# d223abdc-2506-4964-a15f-9a1a19d46edb
guid_trans = GUID(0xd223abdc, 0x2506, 0x4964, (ctypes.c_byte * 8)(0xa1, 0x5f, 0x9a, 0x1a, 0x19, 0xd4, 0x6e, 0xdb))

vtable = ctypes.cast(pTypeLib, ctypes.POINTER(ctypes.POINTER(ctypes.c_void_p))).contents
# GetTypeInfoOfGuid is index 6
GetTypeInfoOfGuid_proto = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.POINTER(GUID), ctypes.POINTER(ctypes.c_void_p))
GetTypeInfoOfGuid = GetTypeInfoOfGuid_proto(vtable[6])

pTypeInfo = ctypes.c_void_p()
hr2 = GetTypeInfoOfGuid(pTypeLib, ctypes.byref(guid_trans), ctypes.byref(pTypeInfo))
print("GetTypeInfoOfGuid hr:", hex(hr2))

if hr2 == 0:
    # ITypeInfo vtable:
    # 3: GetTypeAttr
    # 4: GetTypeComp
    # 5: GetFuncDesc
    # 6: GetVarDesc
    # 7: GetNames
    # 12: GetDocumentation(MEMBERID memid, BSTR *pBstrName, ...)
    tinfo_vtable = ctypes.cast(pTypeInfo, ctypes.POINTER(ctypes.POINTER(ctypes.c_void_p))).contents
    
    # Let's inspect methods!
    # GetTypeAttr(TYPEATTR **ppTypeAttr)
    class TYPEATTR(ctypes.Structure):
        _fields_ = [
            ("guid", GUID),
            ("lcid", wintypes.DWORD),
            ("dwReserved", wintypes.DWORD),
            ("memidConstructor", wintypes.LONG),
            ("memidDestructor", wintypes.LONG),
            ("lpstrSchema", ctypes.c_void_p),
            ("cbSizeInstance", wintypes.ULONG),
            ("typekind", ctypes.c_int),
            ("cFuncs", wintypes.WORD),
            ("cVars", wintypes.WORD),
            ("cInterfaces", wintypes.WORD),
            ("cbSizeVft", wintypes.WORD),
            ("cbAlignment", wintypes.WORD),
            ("wTypeFlags", wintypes.WORD),
            ("wMajorVerNum", wintypes.WORD),
            ("wMinorVerNum", wintypes.WORD),
            ("tdescAlias", ctypes.c_byte * 8),
            ("idldescType", ctypes.c_byte * 8),
        ]
    
    GetTypeAttr_proto = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.POINTER(ctypes.POINTER(TYPEATTR)))
    GetTypeAttr = GetTypeAttr_proto(tinfo_vtable[3])
    
    pTypeAttr = ctypes.POINTER(TYPEATTR)()
    GetTypeAttr(pTypeInfo, ctypes.byref(pTypeAttr))
    attr = pTypeAttr.contents
    print(f"cFuncs: {attr.cFuncs}, cbSizeVft: {attr.cbSizeVft}")
    
    # GetFuncDesc(UINT index, FUNCDESC **ppFuncDesc)
    class FUNCDESC(ctypes.Structure):
        _fields_ = [
            ("memid", wintypes.LONG),
            ("lprgscode", ctypes.c_void_p),
            ("lprgelemdescParam", ctypes.c_void_p),
            ("funckind", ctypes.c_int),
            ("invkind", ctypes.c_int),
            ("callconv", ctypes.c_int),
            ("cParams", ctypes.c_short),
            ("cParamsOpt", ctypes.c_short),
            ("oVft", ctypes.c_short), # VTABLE OFFSET!
            ("wFlags", ctypes.c_short),
            ("elemdescFunc", ctypes.c_byte * 16)
        ]
    
    GetFuncDesc_proto = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.c_uint, ctypes.POINTER(ctypes.POINTER(FUNCDESC)))
    GetFuncDesc = GetFuncDesc_proto(tinfo_vtable[5])
    
    GetDocumentation_proto = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, wintypes.LONG, ctypes.POINTER(ctypes.c_wchar_p), ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p)
    GetDocumentation = GetDocumentation_proto(tinfo_vtable[12])
    
    for fi in range(attr.cFuncs):
        pFuncDesc = ctypes.POINTER(FUNCDESC)()
        GetFuncDesc(pTypeInfo, fi, ctypes.byref(pFuncDesc))
        fd = pFuncDesc.contents
        bstrName = ctypes.c_wchar_p()
        GetDocumentation(pTypeInfo, fd.memid, ctypes.byref(bstrName), None, None, None)
        if fd.oVft == 0x170 or fd.oVft == 0xa4 or fd.oVft == 0xa0:
            print(f"Func {fi}: name={bstrName.value}, oVft=0x{fd.oVft:x} ({fd.oVft}), invkind={fd.invkind}")
        elif 'total' in str(bstrName.value).lower() or 'due' in str(bstrName.value).lower() or 'purchase' in str(bstrName.value).lower() or 'tender' in str(bstrName.value).lower():
            if fd.oVft in [0x170, 0x168, 0x16c, 0x174, 0x178]:
                print(f"  Match: name={bstrName.value}, oVft=0x{fd.oVft:x}")
