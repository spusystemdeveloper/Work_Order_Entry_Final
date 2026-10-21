# ReportViewer 2010 isolated compatibility test

**October 21 follow-up:** The user confirmed the printing candidate works on the
affected PC after installing Microsoft ReportViewer 2010 Redistributable. The
user then authorized the main project switch. The experiment below is retained
as historical evidence; statements that the main project remains on version 15
describe October 20, not the current build. See
`REPORTVIEWER_2010_MAIN_DEPLOYMENT_2026-10-21.md` for the latest build and tests.

Requested October 20, 2026: test the runtime used by V4 so the updated addin
could be delivered as an EXE-only update on units with that runtime installed.
No application startup against a store, shared deployment write, SQL operation
or physical printer job was performed. RMS investigation remains paused.

## Candidate and prerequisites

- Isolated copy: `TestResults/ReportViewer2010-a9930a02/App/` (ignored).
- Candidate: `TestResults/ReportViewer2010-a9930a02/ForManualTest/Work Order Entry_V8.exe`.
- SHA256: `C81235593E8EEFD614924AF30E53C306E9681D480D897E6DE93FE9416DFB625A`.
- .NET Framework 4.7.2, Release/x86, existing Fody/Costura settings retained.
- Only the isolated project's ReportViewer 15 package reference was removed;
  it instead references installed ReportViewer 10 Common/WinForms with
  Copy Local=False. No real project references or default output were replaced.
- Installed test runtime: assembly 10.0.0.0, public key b03f5f7f11d50a3a,
  file version 10.0.30319.1. Common, WinForms, ProcessingObjectModel and
  DataVisualization are installed in Windows' GAC on the development PC.
- All 137 original/candidate VB, RESX and RDLC files matched by hash. Business
  code, print/copy/printer-selection code, page setup and report definitions
  were not edited for this experiment.

This is an EXE-only **delivery using installed dependencies**, not an EXE with
no dependencies. ReportViewer 2005 alone does not meet the version-10 reference.
V4 working on the affected PC is encouraging, but its installed GAC version
and the new candidate's behavior on that PC still need manual confirmation.

## Checks and results

| Check | Result |
| --- | --- |
| Isolated Release/x86 build | Passed |
| Unit suite rebuilt against the isolated candidate | 101/101 passed |
| Clean test folder containing app EXE and synthetic harness, no report DLLs or app config | Both report PDF renders passed using installed runtime |
| Actual import entry-point JIT and synthetic CSV/XLSX imports | Passed; embedded dependencies unchanged |
| Actual preview forms, embedded resources and existing PrintSetting | Passed for both Work Order and Pick List |
| Normal Work Order, Sales Quotation, empty, long text, large values and 60-line multipage fixtures | All six cases rendered to PDF and TIFF for both templates and both engines, with real Work Order/Sales Quotation title parameters |
| ReportViewer 2010 versus 15 TIFF pages | All 32 pages matched in count and dimensions; 27 pages were pixel-identical, five Work Order pages each had two differing title-edge pixels |
| Preview page setup | Identical for both templates: 850x650, existing zero integer margins |
| Runtime identity | ReportViewer.WinForms 10.0.0.0 loaded from GAC; no version-15 report reference in candidate |
| Existing localhost administrative UNC path to the owned EXE-only test folder | Both synthetic report renders and import checks passed; no share created |

The comparison intentionally uses unchanged existing report/page settings,
including their current integer-margin behavior. It does not adjust rounding,
quantities, layouts, paper dimensions, print timing, printers or copy counts.
The final exact-pixel assertion returned false (exit 2); it was not weakened or
reported as a pass. Each difference was at x=353, y=126/127 in the Work Order
title on page 2: two light text-edge pixels in version 10 were white in version
15. All remaining pixels matched, including data, amounts and layout. Normal
case render pairs were visually inspected. The earlier 28-page comparison with
a generic fixture title had matched exactly; the expanded real-title comparison
supersedes that preliminary result. Operator acceptance is still required.

TIFF comparison checks rendered report content; it does not certify every older
ReportViewer toolbar/dialog detail or a physical printer driver.

Evidence in the ignored experiment directory:
`CandidateBuild.log`, `CandidateUnitTests.log`, `ExeOnly.log`, `Compare10.log`,
`Compare15.log`, `ImageComparison.log`, `LocalhostUNC.log`, `SourceIdentity.log`,
`CandidateReferences.log`, `Candidate.json`, and normal-case PNG render pairs.
Test harnesses never invoke the preview forms' production Load handlers, SQL,
PrintDialog, PrintDocument.Print or the real application entry point.

## Remaining acceptance and reproducibility

This experiment is not a live deployment, a live-store print test or proof that
the original version-15 failure was diagnosed. The main project remains on
ReportViewer 15; the candidate is provided separately for testing first.

On a controlled test unit/network test location, use the candidate with that
branch's existing EXE config and installed ReportViewer 2010. Preserve the EXE
name/config pairing; do not overwrite the branch config with the build config.
Check a known Work Order, quotation and Pick List, page layout, printer choice
and copy count, then physical output when the operator authorizes printing.
No new order/save is needed to test a known existing report. Keep the production
shared executable unchanged until that acceptance has passed. Confirm the full
version-10 runtime on other units before assuming EXE-only delivery works there.

To repeat the experiment, copy app sources excluding bin/obj into an owned
TestResults directory; replace only that copy's ReportViewer package reference
with version-10 GAC references (Private=False). Restore/build Release/x86.
Compile the existing PackagingProbe.cs against that copy and the installed
version-10 Common/WinForms assemblies; run with only the candidate EXE and the
probe in a clean folder. The extended probe under the recorded experiment
directory exercises actual preview forms, six cases and image comparison.
For candidate unit tests, copy the test project, point its ProjectReference at
the isolated app and its linked QueueOrderAgeRules source at the original
read-only file; build Release/x86 and run VSTest /Platform:x86.
