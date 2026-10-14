import subprocess

def run_query(sql):
    cmd = ['sqlcmd', '-S', 'localhost', '-U', 'sa', '-P', 'admin123', '-d', 'DVO_STORE_DB', '-y', '0', '-Q', sql]
    res = subprocess.run(cmd, capture_output=True, text=True)
    return res.stdout

q = """
SELECT o.name, o.type_desc, m.definition
FROM sys.sql_modules m
JOIN sys.objects o ON m.object_id = o.object_id
WHERE m.definition LIKE '%CreditLimit%' 
   OR m.definition LIKE '%AccountBalance%'
   OR m.definition LIKE '%exceed%'
   OR m.definition LIKE '%MAA%'
   OR m.definition LIKE '%VINZON%'
ORDER BY o.name;
"""

print(run_query(q))
