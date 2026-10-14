# Let's inspect the code before 0x46aa40
with open(r'C:\Program Files (x86)\Microsoft Retail Management System\Store Operations\SOPOSUSER.exe', 'rb') as f:
    data = f.read()

# Let's find the function prologue before 0x46aa40
# Typical prologue: 55 8b ec (push ebp; mov ebp, esp)
offset = 0x46aa40
prologue_offset = -1
for i in range(offset - 1, max(0, offset - 4000), -1):
    if data[i:i+3] == b'\x55\x8b\xec':
        prologue_offset = i
        break

print(f"Function prologue at 0x{prologue_offset:x} (len {offset - prologue_offset})")

# Let's disassemble instructions between prologue and 0x46aa40 + 200
# We can write a tiny disassembler or inspect calls/jumps
