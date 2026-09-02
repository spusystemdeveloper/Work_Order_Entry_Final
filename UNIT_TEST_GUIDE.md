# Work Order Entry unit tests

These tests check small business rules without opening the application,
connecting to DVO/GSC, or changing database records.

## What is tested

- Price labels map to the correct RMS price-level number.
- The Price Level form initially highlights its top row.
- Multiple customer-change prices are counted and processed one row at a time.
- Matching branch database names are accepted and mismatches are rejected.
- Work order type `2` selects the Work Order print template.
- Quotation type `3` selects the Sales Quotation print template.
- Unknown order types are rejected instead of using the wrong template.
- Quotation/Sale, Transfer/Purchase, and WEBSITE filenames select the correct import workflow.
- Unrecognized import filenames are rejected.
- A numeric senior-citizen ID adds the expected description suffix.
- VAT-inclusive pricing separates VAT correctly.
- Tax-exempt pricing removes VAT.
- Non-taxable items do not add VAT.

The test names describe the scenario in **Given / When / Then** form:

- **Given** describes the starting information.
- **When** describes the action.
- **Then** describes the expected result.

Example:

> Given two taxable items priced at 112.00 each, when their price is
> calculated, then the result is 200.00 VATable sales plus 24.00 VAT.

## Run the tests

From the `Work_Order_Entry_Final` directory:

```powershell
.\Run-UnitTests.ps1
```

A successful run displays each test as `Passed` and ends with a summary such
as:

```text
Passed: 27
Failed: 0
```

If a future change breaks a rule, the relevant test displays `Failed`, its
readable test name, and the expected and actual values.

## What these tests do not do

They do not test SQL functions, stored procedures, saving, recall, inventory
commitment, concurrency, or queue processing. Those workflows require
integration tests against a disposable RMS database restore—never a live
production database.
