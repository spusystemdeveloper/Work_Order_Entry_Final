# Portable required-object baseline

These seven definitions support a full Work Order Entry installation in an
RMS store database that does not already have branch-specific versions.

The inventory, receivables, and open Work Order calculations use only the
currently selected database. They contain no Davao, Gensan, Cebu, warehouse,
or StoreID constants.

For a branch where credit exposure or open Work Orders must be consolidated
across multiple RMS databases, replace `SOD_fn_GetCreditLimit` and
`SOD_fn_GetTotalOpenWorkOrder` with reviewed branch overrides. SQL scalar
functions cannot safely discover arbitrary sibling database names at runtime.

The GSC deployment driver includes these files because all seven objects are
missing from the audited GSC database. DVO retains its existing definitions
to preserve its current cross-warehouse behavior.
