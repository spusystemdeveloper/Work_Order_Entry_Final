# Main project: ReportViewer 2010 and latest EXE

The user confirmed that the earlier isolated EXE printed on the affected PC
after Microsoft ReportViewer 2010 Redistributable was installed, then explicitly
authorized adopting that runtime in the main project with the latest withholding
display. All checks below were local; no company share was updated, no live SQL
was accessed and no physical printer job was sent for this build. RMS remains
paused. The earlier report records the small version-10 versus version-15 title
edge differences; this build was compared to the accepted version-10 candidate.

## Changes and delivery

- Main project keeps .NET Framework 4.7.2 / x86. Replaces the version-15 NuGet
  ReportViewer reference with exact Common/WinForms 10.0.0.0 references from the
  installed GAC, SpecificVersion=True and Copy Local=False. A build prerequisite
  check fails clearly if the installed runtime references are unavailable.
- Costura continues to exclude Microsoft.ReportViewer.*. Other dependencies
  retain their existing packaging. No DLL embedding workaround, runtime trust
  switch, printing routine or business-rule change was introduced.
- The two exact local build folders' obsolete generated version-15 DLLs were
  archived by hash under the ignored test run and removed from those outputs.
  No shared/deployed files were removed or overwritten.
- All **139** application VB, RDLC and RESX files matched their pre-switch
  hashes, including current withholding code, report forms and resources.
- Debug and Release store EXE configurations were preserved byte-for-byte
  through restore, builds and tests. Source App.config was not edited.

Delivery folder (exactly one file):
`Work Order Entry/bin/Release/EXE-Only/Work Order Entry_V8.exe`

SHA256: `2C110DD4167C4B7C43B17CF4C965247A80459C2C661990DC5CE8B3B35C6504BD`

Size: 5,017,088 bytes. It is byte-identical to the latest Release EXE and includes
the Suggested W/Tax display. Keep its filename/config pairing on the target PC.
Install ReportViewer 2010 once per PC, then update the EXE while retaining that
unit's configuration. This is EXE-only delivery using an installed prerequisite,
not a dependency-free application. The supplied installer was inspected earlier
as Microsoft ReportViewer 2010 Redistributable 10.0.30319.01 with a valid Microsoft
signature; it was not run by the assistant. Version 2005 alone is insufficient.

## Validation

| Check | Result |
| --- | --- |
| Restore, main Release/x86, queue Release/x86 and main Debug | Passed |
| Unit tests | 106 passed |
| Withholding customer/actual totals/UI checks in an owned disposable localhost fixture | 30 passed; fixture removed |
| Recall checkbox formatting | 28 passed; zero DataErrors |
| EXE-only clean folder; zero private DLLs/config | Passed |
| Actual Import entry-point JIT and embedded CSV/XLSX readers | Passed; dependency identity preserved |
| Actual WO/quotation and Pick List preview forms, existing embedded resources and PrintSetting | All six sample cases rendered PDF/TIFF for both templates |
| Normal, quotation, empty, long text, large values and multipage output versus accepted ReportViewer 2010 candidate | All 32 TIFF pages matched dimensions and pixels exactly |
| Existing preview settings | 850x650 paper and existing zero integer margins unchanged |
| Runtime loading | Exact version-10 references; Common/WinForms loaded from installed GAC, not private/embedded DLLs |
| Synthetic probe with localhost UNC application base | Passed with the previously validated local harness launch pattern |

The localhost UNC check used a locally launched synthetic harness with
`\\localhost\C$\...\Packaging-7dcc9c05` as its child AppDomain application base.
An initial direct UNC ExecuteAssembly launch of the probe failed to locate the
probe itself before application checks; the local-harness pattern passed all
renders/imports. No trust/config changes were applied. This is not a full app
startup from a store share or a test of another PC's permissions/printer driver.

The packaging check was updated for the explicitly authorized dependency change:
it now rejects all private dependency DLLs/config, stale ReportViewer output,
wrong runtime versions and non-GAC loading, and exercises actual preview forms
across more cases. The former five-DLL check was the version-15 deployment
contract; it was not merely suppressed to make this build pass.

Owned ignored evidence:

- `TestResults/ReportViewer2010Main-d8a4f51d/`: previous version-15 baseline,
  source hashes, restore/build/unit logs, exact rendering comparison,
  localhost UNC log and Delivery.json.
- `TestResults/Packaging-7dcc9c05/`: latest EXE, synthetic harness and 12 paired
  PDF/TIFF files (32 rendered pages). This folder is a test harness, not delivery.
- `TestResults/Withholding-f5d1cefc/`: latest withholding totals-panel renders.

For subsequent builds, run both Release/x86 solutions, Run-UnitTests.ps1,
Packaging_CleanFolder_Test.ps1, WithholdingSuggestions_LocalDB_UI_Test.ps1 and
RecallFormatting_UI_Test.ps1 while preserving store configs. Physical output,
printer selection/copies and full startup from the intended store location
remain operator acceptance checks for the latest EXE; the earlier candidate's
successful physical printing is user-reported evidence, not a test executed here.
