# cshell-api
Coconut Shell charcoal manufacture and sales.

## Quotation invoices

Before deploying the invoice/quotation conversion changes, run
`Erp.Repository\DBScripts\InvoiceQuotationConversion.sql` against the existing SQL Server database.
The script adds the master fields and permits invoice details without outward references.

Use `Invoice_Source: "Quotation"` and a non-empty `Quotation_Id` when creating an invoice
from a quotation; manual invoices use `Invoice_Source: "Manual"` and a null `Quotation_Id`.
Quotation invoice details are saved with null `Outward_ID` and `OutwardDetail_Id`, regardless
of the values sent. For manual invoices, supplied outward IDs must exist, and when both are
supplied the detail must belong to the specified outward.
Only one invoice can be linked to a quotation. The source and quotation cannot be changed
after invoice creation. Deleting the invoice clears the quotation's conversion fields;
a converted quotation cannot be deleted while its invoice exists. Conversion fields in
quotation save requests are server-managed and are not used to change the link.

`Valid_Until` is calculated as the calendar date of `Quotation_Date` plus `Validity_Days`
(default 30). It is null when `Quotation_Date` is null; supplied `Valid_Until` values are
not used. Existing quotations receive the default number of days but their existing
`Valid_Until` remains null until saved.
