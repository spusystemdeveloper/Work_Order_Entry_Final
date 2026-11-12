# Connected project folders

This is the folder map for the Codex desktop project
`spusystemdeveloper/Work_Order_Entry_Final`, updated on November 12, 2026.
It records the five connected workspace folders and the current related V3
source location.

| Connected folder | Role in this workspace |
| --- | --- |
| `C:\Users\ADMIN\source\repos\WORK ORDER ENTRY_ASOF_15SEPT2026` | Main Work Order Entry V8 repository. Contains the Work Order Entry solution, Order Processing Queueing, Database Updater, SQL components, tests and project guidance. |
| `C:\Users\ADMIN\source\repos\WORK_ORDER_ENTRY_3` | Current Work Order Entry V3 source location, supplied by the user on November 12, 2026. Contains `ItemLookUp`, `Database_Component` and `TEMPLATE_CHANGES.md`. Use this location for V3 work in place of the earlier Documents folder. |
| `C:\Users\ADMIN\source\repos\Recon` | Related RMS inventory reconciliation work and branch investigation references. Consult it when tracing transfers and Work Order reconciliation. |
| `C:\Program Files (x86)\Microsoft Retail Management System\Store Operations` | Local RMS installation. Contains installed application components, add-ins, receipt templates and reports used when checking RMS integration and printing. |
| `C:\Users\ADMIN\source\repos\RMS_SYSTEMDOCS` | Shared RMS documentation, report inventory, incident records and troubleshooting handoffs. |
| `C:\Users\ADMIN\source\repos\BIR-WITHHOLDINGTAX` | Withholding-tax analysis, RMS/Invoice Manager implementation notes and supporting SQL references. |

## How the folders relate

For V8 work, start in the main Work Order repository. For V3 work, use
`C:\Users\ADMIN\source\repos\WORK_ORDER_ENTRY_3`. Use the related
folders to trace reconciliation, inspect installed RMS components, review
incident knowledge and understand withholding-tax integration as the task needs.
Read the owning folder's contributor guidance before changing its files.

Each Git checkout retains its own repository, branch, commits and remote.
Connected-folder access provides shared context; build dependencies are defined
by the actual solution and project files. Commits and pushes apply to the
selected repository. The installed RMS folder is an installation location and
should be treated accordingly when inspecting or deploying files.

The paths above describe this PC. When setting up another PC or Codex project,
attach the corresponding local folders and update this map if their locations
change. Branch database settings and deployment requirements remain documented
in the owning projects and installation configuration.

## Main repository references

- [Project overview](README.md)
- [Shared contributor instructions](AGENTS.md)
- [Retained RMS incident knowledge](GEMINI.md)
- [Validation and test instructions](UNIT_TEST_GUIDE.md)

Documentation check: all six listed paths existed on November 12, 2026; the local
folder roles were checked against their documentation or directory contents.
The V3 path records its source location; desktop folder attachments are managed
separately in the project settings.
