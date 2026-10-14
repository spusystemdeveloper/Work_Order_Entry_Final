# Let's search all interfaces in typelib for oVft == 0x7f0
import ctypes
from ctypes import wintypes

oleaut32 = ctypes.oledll.oleaut32
pTypeLib = ctypes.c_void_p()
oleaut32.LoadTypeLib(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll\1', ctypes.byref(pTypeLib))

vtable = ctypes.cast(pTypeLib, ctypes.POINTER(ctypes.POINTER(ctypes.c_void_p))).contents
GetTypeInfoCount = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.POINTER(ctypes.c_uint))(vtable[3])
GetTypeInfo = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.c_uint, ctypes.POINTER(ctypes.c_void_p))(vtable[4])
GetDocumentation = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.c_int, ctypes.POINTER(ctypes.c_wchar_p), ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p)(vtable[9])

class TYPEATTR(ctypes.Structure):
    _fields_ = [
        ("guid", ctypes.c_byte * 16), ("lcid", wintypes.DWORD), ("dwReserved", wintypes.DWORD),
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

# In COM, count is number of typeinfos
count = ctypes.c_uint()
GetTypeInfoCount(pTypeLib, ctypes.byref(count))
print(f"Total TypeInfos: {count.value}")

found = []
for i in range(count.value):
    pTypeInfo = ctypes.c_void_p()
    if GetTypeInfo(pTypeLib, i, ctypes.byref(pTypeInfo)) == 0:
        tinfo_vtable = ctypes.cast(pTypeInfo, ctypes.POINTER(ctypes.POINTER(ctypes.c_void_p))).contents
        GetTypeAttr = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.POINTER(ctypes.POINTER(TYPEATTR)))(tinfo_vtable[3])
        GetFuncDesc = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.c_uint, ctypes.POINTER(ctypes.POINTER(FUNCDESC)))(tinfo_vtable[5])
        GetDoc = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, wintypes.LONG, ctypes.POINTER(ctypes.c_wchar_p), ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p)(tinfo_vtable[12])
        
        bstrName = ctypes.c_wchar_p()
        GetDocumentation(pTypeLib, i, ctypes.byref(bstrName), None, None, None)
        ifname = bstrName.value
        
        pTypeAttr = ctypes.POINTER(TYPEATTR)()
        if GetTypeAttr(pTypeInfo, ctypes.byref(pTypeAttr)) == 0:
            attr = pTypeAttr.contents
            for fi in range(attr.cFuncs):
                pFuncDesc = ctypes.POINTER(FUNCDESC)()
                if GetFuncDesc(pTypeInfo, fi, ctypes.byref(pFuncDesc)) == 0:
                    fd = pFuncDesc.contents
                    fnName = ctypes.c_wchar_p()
                    GetDoc(pTypeInfo, fd.memid, ctypes.byref(fnName), None, None, None)
                    if fd.oVft == 0x7f0:
                        print(f"FOUND 0x7f0! Interface {ifname}, Method {fnName.value}")
                    if 'overtender' in str(fnName.value).lower():
                        print(f"Tender method: {ifname}::{fnName.value}, oVft=0x{fd.oVft:x}")
