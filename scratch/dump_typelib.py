import ctypes
from ctypes import wintypes

oleaut32 = ctypes.oledll.oleaut32

pTypeLib = ctypes.c_void_p()
hr = oleaut32.LoadTypeLib(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\QSRules.dll', ctypes.byref(pTypeLib))
print("LoadTypeLib hr:", hex(hr))

vtable = ctypes.cast(pTypeLib, ctypes.POINTER(ctypes.POINTER(ctypes.c_void_p))).contents

GetTypeInfoCount_proto = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.POINTER(ctypes.c_uint))
GetTypeInfoCount = GetTypeInfoCount_proto(vtable[3])

GetTypeInfo_proto = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.c_uint, ctypes.POINTER(ctypes.c_void_p))
GetTypeInfo = GetTypeInfo_proto(vtable[4])

GetDocumentation_proto = ctypes.WINFUNCTYPE(ctypes.HRESULT, ctypes.c_void_p, ctypes.c_int, ctypes.POINTER(ctypes.c_wchar_p), ctypes.POINTER(ctypes.c_wchar_p), ctypes.POINTER(wintypes.DWORD), ctypes.POINTER(ctypes.c_wchar_p))
GetDocumentation = GetDocumentation_proto(vtable[9])

count = ctypes.c_uint()
GetTypeInfoCount(pTypeLib, ctypes.byref(count))
print("TypeInfo count:", count.value)

for i in range(count.value):
    bstrName = ctypes.c_wchar_p()
    GetDocumentation(pTypeLib, i, ctypes.byref(bstrName), None, None, None)
    name = bstrName.value
    # oleaut32.SysFreeString(bstrName)
    print(f"Index {i}: {name}")
