with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe', 'rb') as f:
    data = f.read()

offset = 0x46aa40
start = max(0, offset - 100)
end = min(len(data), offset + 100)
chunk = data[start:end]
print(f"Bytes around {hex(offset)}:")
print(" ".join(f"{b:02x}" for b in chunk))
