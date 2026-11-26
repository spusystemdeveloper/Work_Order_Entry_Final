# V8 first Work Order account

Open V8 against the intended store database and click Login. If the existing
`dbo.SOD_WO_User` table is empty, V8 opens **Create first Work Order account**.
The dialog shows the selected server/database. Choose a username, enter and
confirm a password, and enter the correct register number. Click Create account,
then sign in through the normal Login screen. The name defaults to `admin`, but
there is no built-in password or special login bypass. The full name initially
matches the username; existing user maintenance remains available afterward.

Cancel makes no account changes. Existing accounts disable first-account
creation; a second client cannot create another initial account concurrently.
This is not an account-reset feature. Connection errors or a missing user table
stop setup instead of creating tables or treating a failure as an empty store.
The existing user table must already be installed by the reviewed branch setup.

This feature is V8 only. Existing password matching, register validation, pricing,
tax, stock, saving and approval behavior continue unchanged. No SQL migration or
new dependency is required. No live setup was executed during development.

Validation: 35 owned LocalDB checks exercised actual Login/setup dialogs,
cancellation, input validation, existing-account authentication rejection,
concurrent first creation and missing-schema errors. Both Release/x86 solutions
and all 119 unit tests passed. Full normal login and RMS register UI were not
automated by the fixture; verify those on localhost with your intended register.
Clean-folder EXE packaging/import/report checks also passed. Retain the target
unit's existing EXE configuration and installed ReportViewer 2010 runtime.
