# Work Order UI preview

Open `bin/Release/ModernBlue/Work Order Modern Preview.exe` to try the blue
design adapted from `DVO_WORK_ORDER_V8_WinForms`. It uses a compact customer
header, white panels with rounded borders, neutral grids, blue selection/Save
and compact withholding tabs. This is an independent VB.NET
WinForms visual demo, not an alternative version of the production addin.

- Search the sample items, select rows, and click recent searches.
- Switch the four withholding tabs. Every enabled example rate stays visible
  in its tab title; the order totals stay visible beside the tabs.
- Refresh clears the sample search. Close exits the preview.
- Other buttons only display a preview notice in the bottom status bar.
  Fields and selected item quantities are read-only sample values.

No database, RMS, registry, production configuration or ReportViewer is used.
No save, approval, stock, tax or printing workflow is implemented or changed.
The preview does not reproduce production keyboard shortcuts; labels show their
proposed appearance only. The main solution does not reference this project.
RMS investigation remains paused. Do not deploy this demo as the real addin.
The supplied C# sample is kept intact. Only its visual approach was adapted;
its mock save/total code, .NET 8 target and Guna package were not copied.
This preview needs neither Guna nor Windows 11 Mica. The earlier teal EXE,
if already built, is left in its original folder for comparison.

Requires Windows with .NET Framework 4.7.2. Build independently:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' '.\UI Preview\WorkOrderUiPreview.vbproj' /t:Build /p:Configuration=Release /p:Platform=x86 /v:minimal
```

For hidden sample-data checks and screenshots, run the EXE with
`--check <absolute-output-directory>`. Checks cover sample searching, empty
results, switching four tabs without changing the fixed sample totals, visible
actions and tab space at 1366 and 1920 window widths. Visual acceptance on the
user's display and Windows scaling is still required.

October 21 validation: preview and both production Release/x86 solutions built;
all 106 existing unit tests passed. Preview checks/rendering passed at both
widths. Debug and Release store configurations were preserved byte-for-byte.
Only this new preview folder and documentation changed in the repository.
