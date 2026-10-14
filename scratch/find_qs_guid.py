import winreg

k = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r'SOFTWARE\Classes\TypeLib', 0, winreg.KEY_READ | winreg.KEY_WOW64_32KEY)
for i in range(winreg.QueryInfoKey(k)[0]):
    sub = winreg.EnumKey(k, i)
    try:
        sk = winreg.OpenKey(k, sub, 0, winreg.KEY_READ | winreg.KEY_WOW64_32KEY)
        for j in range(winreg.QueryInfoKey(sk)[0]):
            ver = winreg.EnumKey(sk, j)
            vk = winreg.OpenKey(sk, ver, 0, winreg.KEY_READ | winreg.KEY_WOW64_32KEY)
            name = winreg.QueryValue(vk, None)
            if name and 'qsrules' in name.lower():
                print(f"Found QSRules TypeLib: {sub} - {name}")
                # print path:
                pk = winreg.OpenKey(vk, r'0\win32', 0, winreg.KEY_READ | winreg.KEY_WOW64_32KEY)
                print("Path:", winreg.QueryValue(pk, None))
    except:
        pass
