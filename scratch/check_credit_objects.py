import pyodbc

# Or use sqlcmd or subprocess
import subprocess

def run_query(sql):
    cmd = ['sqlcmd', '-S', 'localhost', '-U', 'sa', '-P', 'admin123', '-d', 'DVO_STORE_DB', '-y', '0', '-Q', sql]
    res = subprocess.run(cmd, capture_output=True, text=True)
    return res.stdout

print("=== SOD_fn_GetCL ===")
print(run_query("sp_helptext 'SOD_fn_GetCL'"))

print("=== SOD_fn_GetCreditLimit ===")
print(run_query("sp_helptext 'SOD_fn_GetCreditLimit'"))

print("=== SOD_sp_Reclashing ===")
print(run_query("sp_helptext 'SOD_sp_Reclashing'"))

print("=== UpdateCustomerAccountBalance ===")
print(run_query("sp_helptext 'UpdateCustomerAccountBalance'"))
