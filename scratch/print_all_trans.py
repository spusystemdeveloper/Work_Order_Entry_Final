import ctypes
from ctypes import wintypes

oleaut32 = ctypes.oledll.oleaut32

pTypeLib = ctypes.c_void_p()
hr = oleaut32.LoadTypeLib(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll\1', ctypes.byref(pTypeLib))

class GUID(ctypes.Structure):
    _fields_ = [("Data1", wintypes.DWORD), ("Data2", wintypes.WORD), ("Data3", wintypes.WORD), ("Data4", ctypes.c_byte * 8)]

guid_trans = GUID(0xd223abdc, 0x2506, 0x4964, (ctypes.c_byte * 8)(0xa1, 0x5f, 0x9a, 0x1a, 0x19, 0xd4, 0x6e, 0xdb))

vtable = ctypes.cast(pTypeLib, ctypes.POINTER(ctypes.POINTER(ctypes.c_void_p))).contents
GetTypeInfoOfGuid = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.POINTER(GUID), ctypes.POINTER(ctypes.c_void_p))(vtable[6])

pTypeInfo = ctypes.c_void_p()
GetTypeInfoOfGuid(pTypeLib, ctypes.byref(guid_trans), ctypes.byref(pTypeInfo))
tinfo_vtable = ctypes.cast(pTypeInfo, ctypes.POINTER(ctypes.POINTER(ctypes.c_void_p))).contents

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

GetTypeAttr = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.POINTER(ctypes.POINTER(TYPEATTR)))(tinfo_vtable[3])
GetFuncDesc = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.c_uint, ctypes.POINTER(ctypes.POINTER(FUNCDESC)))(tinfo_vtable[5])
GetDocumentation = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, wintypes.LONG, ctypes.POINTER(ctypes.c_wchar_p), ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p)(tinfo_vtable[12])

pTypeAttr = ctypes.POINTER(TYPEATTR)()
GetTypeAttr(pTypeInfo, ctypes.byref(pTypeAttr))

for fi in range(pTypeAttr.contents.cFuncs):
    pFuncDesc = ctypes.POINTER(FUNCDESC)()
    GetFuncDesc(pTypeInfo, fi, ctypes.byref(pFuncDesc))
    fd = pFuncDesc.contents
    bstrName = ctypes.c_wchar_p()
    GetDocumentation(pTypeInfo, fd.memid, ctypes.byref(bstrName), None, None, None)
    print(f"oVft=0x{fd.oVft:x} ({fd.oVft}): {bstrName.value} (invkind={fd.invkind})")
