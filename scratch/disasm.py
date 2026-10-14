# Let's inspect the exact instructions from 0x46a960 to 0x46aa50
# We can inspect the bytes line by line

bytes_hex = """
46a960: 55 
46a961: 8b ec 
46a963: 83 ec 0c 
46a966: 68 56 ba 41 00 
46a96b: 64 a1 00 00 00 00 
46a971: 50 
46a972: 64 89 25 00 00 00 00 
46a979: 81 ec d0 00 00 00 
46a97f: 53 
46a980: 56 
46a981: 57 
46a982: 89 65 f4 
46a985: c7 45 f8 08 45 41 00 
46a98c: 8b 75 08 
46a98f: 8b c6 
46a991: 83 e0 01 
46a994: 89 45 fc 
46a997: 83 e6 fe 
46a99a: 56 
46a99b: 89 75 08 
46a99e: 8b 0e 
46a9a0: ff 51 04 
46a9a3: 8b 16 
46a9a5: 8d 45 e4 
46a9a8: 33 db 
46a9aa: 50 
46a9ab: 56 
46a9ac: 89 5d e8 
46a9af: 89 5d e4 
46a9b2: 89 5d e0 
46a9b5: 89 5d dc 
46a9b8: 89 5d d8 
46a9bb: 89 5d d4 
46a9be: 89 5d d0 
46a9c1: 89 5d cc 
46a9c4: 89 5d c8 
46a9c7: 89 5d c4 
46a9ca: 89 5d c0 
46a9cd: 89 5d bc 
46a9d0: 89 5d ac 
46a9d3: 89 5d 9c 
46a9d6: 89 5d 8c 
46a9d9: 89 9d 7c ff ff ff 
46a9df: 89 9d 6c ff ff ff 
46a9e5: 89 9d 58 ff ff ff 
46a9eb: 89 9d 54 ff ff ff 
46a9f1: 89 9d 50 ff ff ff 
46a9f7: 89 9d 4c ff ff ff 
46a9fd: 89 9d 48 ff ff ff 
46aa03: ff 92 f0 07 00 00 
46aa09: 8b 4d e4 
46aa0c: 51 
46aa0d: 68 58 f2 49 00 
46aa12: ff 15 3c 12 40 00 
46aa18: 8b f8 
46aa1a: 8d 4d e4 
46aa1d: f7 df 
46aa1f: 1b ff 
46aa21: f7 df 
46aa23: f7 df 
46aa25: ff 15 20 15 40 00 
46aa2b: 66 3b fb 
46aa2e: 0f 84 ab 01 00 00 
"""
print("Disassembly:")
# 8b 75 08 -> mov esi, [ebp+8] (the 'this' or transaction parameter)
# 8b c6 -> mov eax, esi
# 83 e0 01 -> and eax, 1
# 89 45 fc -> mov [ebp-4], eax
# 83 e6 fe -> and esi, 0xFFFFFFFE (strip bottom bit)
# 56 -> push esi
# 8b 0e -> mov ecx, [esi] (ecx = vtable)
# ff 51 04 -> call [ecx+4] (AddRef)
# 8b 16 -> mov edx, [esi] (edx = vtable)
# 8d 45 e4 -> lea eax, [ebp-1Ch] (out string parameter!)
# 33 db -> xor ebx, ebx
# 50 -> push eax (pointer to out BSTR)
# 56 -> push esi (this pointer)
# ... initializing local variables to 0 (ebx) ...
# ff 92 f0 07 00 00 -> call [edx + 0x7f0]
# 8b 4d e4 -> mov ecx, [ebp-1Ch] (the returned BSTR!)
# 51 -> push ecx
# 68 58 f2 49 00 -> push pointer to empty string / comparison string
# ff 15 3c 12 40 00 -> call __vbaStrCmp / StrCmp / wcscmp
# 66 3b fb -> cmp di, bx
# 0f 84 ab 01 00 00 -> jz 0x46abd5 (if strings match or length == 0, SKIP ERROR!)
