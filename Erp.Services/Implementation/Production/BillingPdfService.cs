using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Services.Interfaces.Production;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace Erp.Services.Implementation.Production
{
    public class BillingPdfService : IBillingPdfService
    {
        private const string FontName = "Noto Sans";

        // =====================================================
        // A4 PAGE
        // =====================================================

        private const double PageWidthCm = 21.0;
        private const double LeftMarginCm = 0.8;
        private const double RightMarginCm = 0.8;

        private const double ContentWidthCm =
            PageWidthCm - LeftMarginCm - RightMarginCm;

        // =====================================================
        // GENERATE PDF
        // =====================================================

        public byte[] GeneratePdf(
            BillingReportDTO billing)
        {
            var document = new Document();

            document.Info.Title =
                "Billing Receipt";

            document.Info.Subject =
                "Billing Receipt";

            document.Info.Author =
                "Erp";

            ConfigureStyles(document);

            var section =
                document.AddSection();

            ConfigurePage(section);

            ComposeHeader(
                section,
                billing
            );

            ComposeContent(
                section,
                billing
            );

            ComposeFooter(section);

            using var stream =
                new MemoryStream();

            var renderer =
                new PdfDocumentRenderer
                {
                    Document = document
                };

            renderer.RenderDocument();

            renderer.PdfDocument.Save(
                stream,
                false
            );

            return stream.ToArray();
        }

        // =====================================================
        // PAGE
        // =====================================================

        private void ConfigurePage(
            Section section)
        {
            section.PageSetup.PageFormat =
                PageFormat.A4;

            section.PageSetup.Orientation =
                Orientation.Portrait;

            section.PageSetup.TopMargin =
                Unit.FromCentimeter(0.8);

            section.PageSetup.BottomMargin =
                Unit.FromCentimeter(0.8);

            section.PageSetup.LeftMargin =
                Unit.FromCentimeter(0.8);

            section.PageSetup.RightMargin =
                Unit.FromCentimeter(0.8);

            section.PageSetup.HeaderDistance =
                Unit.FromCentimeter(0.3);

            section.PageSetup.FooterDistance =
                Unit.FromCentimeter(0.3);
        }

        // =====================================================
        // STYLES
        // =====================================================

        private void ConfigureStyles(
            Document document)
        {
            var normalStyle =
                document.Styles["Normal"];

            normalStyle.Font.Name =
                FontName;

            normalStyle.Font.Size =
                Unit.FromPoint(9);

            normalStyle.ParagraphFormat.SpaceAfter =
                Unit.FromPoint(0);
        }

        // =====================================================
        // HEADER
        // =====================================================

        private void ComposeHeader(
            Section section,
            BillingReportDTO billing)
        {
            // =================================================
            // COMPANY HEADER
            // =================================================

            var companyTable =
                section.AddTable();

            companyTable.Borders.Visible =
                false;

            companyTable.AddColumn(
                Unit.FromCentimeter(ContentWidthCm)
            );

            var companyRow =
                companyTable.AddRow();

            var companyCell =
                companyRow.Cells[0];

            companyCell.VerticalAlignment =
                VerticalAlignment.Top;

            // =================================================
            // COMPANY NAME
            // =================================================

            var companyName =
                companyCell.AddParagraph();

            companyName.Format.Alignment =
                ParagraphAlignment.Left;

            companyName.Format.SpaceAfter =
                Unit.FromPoint(2);

            var companyNameText =
                companyName.AddFormattedText(
                    billing.CompanyName ?? string.Empty
                );

            companyNameText.Bold =
                true;

            companyNameText.Font.Name =
                FontName;

            companyNameText.Font.Size =
                Unit.FromPoint(14);

            // =================================================
            // COMPANY ADDRESS
            // =================================================

            AddCellText(
                companyCell,
                billing.CompanyAddress
            );

            // =================================================
            // PHONE
            // =================================================

            AddCellText(
                companyCell,
                $"Phone no.: {billing.CompanyPhone}"
            );

            // =================================================
            // GSTIN
            // =================================================

            AddCellText(
                companyCell,
                $"GSTIN: {billing.CompanyGSTIN}"
            );

            // =================================================
            // STATE
            // =================================================

            AddCellText(
                companyCell,
                $"State: {billing.CompanyState}"
            );

            // =================================================
            // TAX INVOICE
            // =================================================

            var title =
                section.AddParagraph();

            title.Format.Alignment =
                ParagraphAlignment.Center;

            title.Format.SpaceBefore =
                Unit.FromPoint(6);

            title.Format.SpaceAfter =
                Unit.FromPoint(6);

            var titleText =
                title.AddFormattedText(
                    "Tax Invoice"
                );

            titleText.Bold =
                true;

            titleText.Font.Name =
                FontName;

            titleText.Font.Size =
                Unit.FromPoint(17);

            // =================================================
            // HORIZONTAL LINE
            // =================================================

            AddHorizontalLine(section);

            // =================================================
            // BILL TO + INVOICE DETAILS
            // =================================================

            var table =
                section.AddTable();

            table.Borders.Visible =
                false;

            table.AddColumn(
                Unit.FromCentimeter(9.7)
            );

            table.AddColumn(
                Unit.FromCentimeter(9.7)
            );

            var row =
                table.AddRow();

            // =================================================
            // BILL TO
            // =================================================

            var supplierCell =
                row.Cells[0];

            supplierCell.VerticalAlignment =
                VerticalAlignment.Top;

            var supplierTitle =
                supplierCell.AddParagraph();

            supplierTitle.Format.SpaceAfter =
                Unit.FromPoint(3);

            AddBoldText(
                supplierTitle,
                "Bill To",
                10
            );

            AddCellText(
                supplierCell,
                billing.SupplierName
            );

            AddCellText(
                supplierCell,
                billing.SupplierAddress
            );

            AddCellText(
                supplierCell,
                $"Contact No.: {billing.SupplierPhone}"
            );

            AddCellText(
                supplierCell,
                $"GSTIN: {billing.SupplierGSTIN}"
            );

            AddCellText(
                supplierCell,
                $"State: {billing.SupplierState}"
            );

            // =================================================
            // INVOICE DETAILS
            // =================================================
   

            var receiptCell =
            row.Cells[1];

            receiptCell.VerticalAlignment =
                VerticalAlignment.Top;

            var receiptTitle =
                receiptCell.AddParagraph();

            receiptTitle.Format.Alignment =
                ParagraphAlignment.Right;

            receiptTitle.Format.SpaceAfter =
                Unit.FromPoint(3);

            AddBoldText(
                receiptTitle,
                "Invoice Details",
                10
            );

            AddRightCellText(
                receiptCell,
                $"Invoice No.: {billing.BillingNumber}"
            );

            AddRightCellText(
                receiptCell,
                $"Date: {billing.ReceiptDate:dd-MM-yyyy}"
            );

            AddRightCellText(
                receiptCell,
                $"Place of Supply: {billing.SupplierState}"
            );

            // =================================================
            // SPACE BEFORE ITEM TABLE
            // =================================================

            AddSpacing(
                section,
                8
            );
        }

        // =====================================================
        // CONTENT
        // =====================================================

        private void ComposeContent(
            Section section,
            BillingReportDTO billing)
        {
            // =================================================
            // CALCULATE TOTALS
            // =================================================

            var totalAmount =
                billing.DebitDetails?
                    .Sum(x => x.Amount) ?? 0m;

            var totalQuantity =
                billing.DebitDetails?
                    .Sum(x => x.Quantity) ?? 0m;

            // =================================================
            // ITEM TABLE
            // =================================================

            var debitTable =
                section.AddTable();

            debitTable.Borders.Width =
                Unit.FromPoint(0.5);

            debitTable.Borders.Color =
                Colors.Gray;

            // =================================================
            // COLUMNS
            // TOTAL = 19.4 CM
            // =================================================

            debitTable.AddColumn(
                Unit.FromCentimeter(0.8)
            );

            debitTable.AddColumn(
                Unit.FromCentimeter(5.5)
            );

            debitTable.AddColumn(
                Unit.FromCentimeter(2.5)
            );

            debitTable.AddColumn(
                Unit.FromCentimeter(2.5)
            );

            debitTable.AddColumn(
                Unit.FromCentimeter(1.5)
            );

            debitTable.AddColumn(
                Unit.FromCentimeter(3.0)
            );

            debitTable.AddColumn(
                Unit.FromCentimeter(3.6)
            );

            // =================================================
            // TABLE HEADER
            // =================================================

            var debitHeader =
                debitTable.AddRow();

            debitHeader.Height =
                Unit.FromCentimeter(0.7);

            SetHeaderCell(
                debitHeader.Cells[0],
                "#",
                ParagraphAlignment.Center
            );

            SetHeaderCell(
                debitHeader.Cells[1],
                "Item Name"
            );

            SetHeaderCell(
                debitHeader.Cells[2],
                "HSN/SAC",
                ParagraphAlignment.Center
            );

            SetHeaderCell(
                debitHeader.Cells[3],
                "Quantity",
                ParagraphAlignment.Right
            );

            SetHeaderCell(
                debitHeader.Cells[4],
                "Unit",
                ParagraphAlignment.Center
            );

            SetHeaderCell(
                debitHeader.Cells[5],
                "Price/Unit",
                ParagraphAlignment.Right
            );

            SetHeaderCell(
                debitHeader.Cells[6],
                "Amount",
                ParagraphAlignment.Right
            );

            // =================================================
            // DEBIT ROWS
            // =================================================

            if (billing.DebitDetails != null)
            {
                foreach (var detail in billing.DebitDetails)
                {
                    var row =
                        debitTable.AddRow();

                    SetDataCell(
                        row.Cells[0],
                        detail.SlNo.ToString(),
                        ParagraphAlignment.Center
                    );

                    SetDataCell(
                        row.Cells[1],
                        detail.Deduction
                    );

                    SetDataCell(
                        row.Cells[2],
                        detail.HSNCode,
                        ParagraphAlignment.Center
                    );

                    SetDataCell(
                        row.Cells[3],
                        detail.Quantity.ToString("N2"),
                        ParagraphAlignment.Right
                    );

                    SetDataCell(
                        row.Cells[4],
                        detail.Uom,
                        ParagraphAlignment.Center
                    );

                    SetDataCell(
                        row.Cells[5],
                        $"₹ {detail.Rate:N2}",
                        ParagraphAlignment.Right
                    );

                    SetDataCell(
                        row.Cells[6],
                        $"₹ {detail.Amount:N2}",
                        ParagraphAlignment.Right
                    );
                }
            }

            // =================================================
            // SUBTOTAL ROW
            // =================================================

            var subtotalRow =
                debitTable.AddRow();

            subtotalRow.Borders.Top.Width =
                Unit.FromPoint(1);

            subtotalRow.Borders.Top.Color =
                Colors.Gray;

            subtotalRow.Borders.Bottom.Width =
                Unit.FromPoint(0.5);

            subtotalRow.Borders.Bottom.Color =
                Colors.Gray;

            SetSubtotalCell(
                subtotalRow.Cells[0],
                string.Empty
            );

            SetSubtotalCell(
                subtotalRow.Cells[1],
                "Total"
            );

            SetSubtotalCell(
                subtotalRow.Cells[2],
                string.Empty
            );

            SetSubtotalCell(
                subtotalRow.Cells[3],
                totalQuantity.ToString("N2"),
                ParagraphAlignment.Right
            );

            SetSubtotalCell(
                subtotalRow.Cells[4],
                string.Empty,
                ParagraphAlignment.Center
            );

            SetSubtotalCell(
                subtotalRow.Cells[5],
                string.Empty,
                ParagraphAlignment.Right
            );

            SetSubtotalCell(
                subtotalRow.Cells[6],
                $"₹ {totalAmount:N2}",
                ParagraphAlignment.Right
            );

            // =================================================
            // SUMMARY
            // =================================================

            AddSpacing(
                section,
                8
            );

            var summaryTable =
                section.AddTable();

            summaryTable.Borders.Visible =
                false;

            // =================================================
            // LEFT = 11.5 CM
            // RIGHT = 7.9 CM
            // TOTAL = 19.4 CM
            // =================================================

            summaryTable.AddColumn(
                Unit.FromCentimeter(11.5)
            );

            summaryTable.AddColumn(
                Unit.FromCentimeter(7.9)
            );

            var summaryRow =
                summaryTable.AddRow();

            // =================================================
            // LEFT CELL
            // =================================================

            var amountWordsCell =
                summaryRow.Cells[0];

            amountWordsCell.VerticalAlignment =
                VerticalAlignment.Top;

            // =================================================
            // AMOUNT IN WORDS
            // =================================================

            var amountWordsTitle =
                amountWordsCell.AddParagraph();

            amountWordsTitle.Format.Alignment =
                ParagraphAlignment.Left;

            amountWordsTitle.Format.SpaceAfter =
                Unit.FromPoint(4);

            AddBoldText(
                amountWordsTitle,
                "Invoice Amount In Words",
                10
            );

            var amountWords =
                amountWordsCell.AddParagraph();

            amountWords.Format.Alignment =
                ParagraphAlignment.Left;

            amountWords.Format.SpaceAfter =
                Unit.FromPoint(10);

            var amountText =
                amountWords.AddFormattedText(
                    billing.AmountInWords ?? string.Empty
                );

            amountText.Bold =
                false;

            amountText.Font.Name =
                FontName;

            amountText.Font.Size =
                Unit.FromPoint(9);

            // =================================================
            // TERMS AND CONDITIONS
            // =================================================

            var termsTitle =
                amountWordsCell.AddParagraph();

            termsTitle.Format.Alignment =
                ParagraphAlignment.Left;

            termsTitle.Format.SpaceBefore =
                Unit.FromPoint(8);

            termsTitle.Format.SpaceAfter =
                Unit.FromPoint(4);

            AddBoldText(
                termsTitle,
                "Terms and Conditions",
                10
            );

            var terms =
                amountWordsCell.AddParagraph();

            terms.Format.Alignment =
                ParagraphAlignment.Left;

            terms.Format.SpaceBefore =
                Unit.FromPoint(0);

            terms.Format.SpaceAfter =
                Unit.FromPoint(10);

            var termsText =
                terms.AddFormattedText(
           "Thank you for doing business with us"
                );

            termsText.Bold =
                false;

            termsText.Font.Name =
                FontName;

            termsText.Font.Size =
                Unit.FromPoint(8.5);

            // =================================================
            // RIGHT CELL - TOTALS
            // =================================================

            var totalsCell =
                summaryRow.Cells[1];

            totalsCell.VerticalAlignment =
                VerticalAlignment.Top;

            // =================================================
            // SUB TOTAL
            // =================================================

            AddTotalsRow(
                totalsCell,
                "Sub Total :",
                totalAmount
            );

            // =================================================
            // EXMP
            // =================================================

            var ExmpAmount =
                0m;

            AddTotalsRow(
                totalsCell,
                "Exmp. @ 0.0% :",
                ExmpAmount
            );

            // =================================================
            // TOTAL
            // =================================================

            AddTotalsRow(
                totalsCell,
                "Total :",
                totalAmount
            );

            // =================================================
            // RECEIVED
            // =================================================

            var receivedAmount =
                0m;

            AddTotalsRow(
                totalsCell,
                "Received :",
                receivedAmount
            );

            // =================================================
            // BALANCE
            // =================================================

            var balanceAmount =
                totalAmount - receivedAmount;

            AddTotalsRow(
                totalsCell,
                "Balance :",
                balanceAmount
            );

            // =================================================
            // BALANCE LINE
            // =================================================

            var balanceLine =
                totalsCell.AddParagraph();

            balanceLine.Format.Alignment =
                ParagraphAlignment.Right;

            balanceLine.Format.SpaceBefore =
                Unit.FromPoint(3);

            balanceLine.Format.SpaceAfter =
                Unit.FromPoint(2);

            balanceLine.Format.Borders.Bottom.Width =
                Unit.FromPoint(1);

            balanceLine.Format.Borders.Bottom.Color =
                Colors.Gray;

            // =================================================
            // SIGNATURE
            // =================================================

            AddSpacing(
                section,
                18
            );

            var signatureTable =
                section.AddTable();

            signatureTable.Borders.Visible =
                false;

            signatureTable.AddColumn(
                Unit.FromCentimeter(ContentWidthCm)
            );

            var signatureCell =
                signatureTable.AddRow()
                    .Cells[0];

            signatureCell.VerticalAlignment =
                VerticalAlignment.Bottom;

            // =================================================
            // FOR COMPANY
            // =================================================

            var forCompany =
                signatureCell.AddParagraph();

            forCompany.Format.Alignment =
                ParagraphAlignment.Right;

            forCompany.Format.SpaceAfter =
                Unit.FromPoint(28);

            var forText =
                forCompany.AddFormattedText(
                    $"For: {billing.CompanyName ?? string.Empty}"
                );

            forText.Bold =
                false;

            forText.Font.Name =
                FontName;

            forText.Font.Size =
                Unit.FromPoint(9);

            // =================================================
            // AUTHORIZED SIGNATORY
            // =================================================

            var signature =
                signatureCell.AddParagraph();

            signature.Format.Alignment =
                ParagraphAlignment.Right;

            var signatureText =
                signature.AddFormattedText(
                    "Authorized Signatory"
                );

            signatureText.Bold =
                true;

            signatureText.Font.Name =
                FontName;

            signatureText.Font.Size =
                Unit.FromPoint(9);
        }

        // =====================================================
        // TOTALS ROW
        // =====================================================

        private void AddTotalsRow(
            Cell parentCell,
            string label,
            decimal amount)
        {
            var table =
                parentCell.Elements.AddTable();

            table.Borders.Visible =
                false;

            // =================================================
            // LABEL COLUMN
            // =================================================

            table.AddColumn(
                Unit.FromCentimeter(4.2)
            );

            // =================================================
            // AMOUNT COLUMN
            // =================================================

            table.AddColumn(
                Unit.FromCentimeter(3.7)
            );

            var row =
                table.AddRow();

            // =================================================
            // LABEL
            // =================================================

            var labelCell =
                row.Cells[0];

            labelCell.VerticalAlignment =
                VerticalAlignment.Center;

            var labelParagraph =
                labelCell.AddParagraph();

            labelParagraph.Format.Alignment =
                ParagraphAlignment.Left;

            labelParagraph.Format.SpaceBefore =
                Unit.FromPoint(1);

            labelParagraph.Format.SpaceAfter =
                Unit.FromPoint(3);

            var labelText =
                labelParagraph.AddFormattedText(
                    label
                );

            labelText.Bold =
                false;

            labelText.Font.Name =
                FontName;

            labelText.Font.Size =
                Unit.FromPoint(9);

            // =================================================
            // AMOUNT
            // =================================================

            var amountCell =
                row.Cells[1];

            amountCell.VerticalAlignment =
                VerticalAlignment.Center;

            var amountParagraph =
                amountCell.AddParagraph();

            amountParagraph.Format.Alignment =
                ParagraphAlignment.Right;

            amountParagraph.Format.SpaceBefore =
                Unit.FromPoint(1);

            amountParagraph.Format.SpaceAfter =
                Unit.FromPoint(3);

            var amountText =
                amountParagraph.AddFormattedText(
                    $"₹ {amount:N2}"
                );

            amountText.Bold =
                false;

            amountText.Font.Name =
                FontName;

            amountText.Font.Size =
                Unit.FromPoint(9);
        }

        // =====================================================
        // TABLE HEADER
        // =====================================================

        private void SetHeaderCell(
            Cell cell,
            string text,
            ParagraphAlignment alignment =
                ParagraphAlignment.Left)
        {
            cell.Shading.Color =
                Colors.LightGray;

            cell.Borders.Width =
                Unit.FromPoint(0.5);

            cell.Borders.Color =
                Colors.Gray;

            cell.VerticalAlignment =
                VerticalAlignment.Center;

            var paragraph =
                cell.AddParagraph();

            paragraph.Format.Alignment =
                alignment;

            paragraph.Format.SpaceBefore =
                Unit.FromPoint(4);

            paragraph.Format.SpaceAfter =
                Unit.FromPoint(4);

            var formatted =
                paragraph.AddFormattedText(
                    text ?? string.Empty
                );

            formatted.Bold =
                true;

            formatted.Font.Name =
                FontName;

            formatted.Font.Size =
                Unit.FromPoint(9);
        }

        // =====================================================
        // TABLE DATA
        // =====================================================

        private void SetDataCell(
            Cell cell,
            string? text,
            ParagraphAlignment alignment =
                ParagraphAlignment.Left)
        {
            cell.Borders.Bottom.Width =
                Unit.FromPoint(0.5);

            cell.Borders.Bottom.Color =
                Colors.LightGray;

            cell.VerticalAlignment =
                VerticalAlignment.Center;

            var paragraph =
                cell.AddParagraph();

            paragraph.Format.Alignment =
                alignment;

            paragraph.Format.SpaceBefore =
                Unit.FromPoint(4);

            paragraph.Format.SpaceAfter =
                Unit.FromPoint(4);

            var formatted =
                paragraph.AddFormattedText(
                    text ?? string.Empty
                );

            formatted.Bold =
                false;

            formatted.Font.Name =
                FontName;

            formatted.Font.Size =
                Unit.FromPoint(9);
        }

        // =====================================================
        // SUBTOTAL CELL
        // =====================================================

        private void SetSubtotalCell(
            Cell cell,
            string? text,
            ParagraphAlignment alignment =
                ParagraphAlignment.Left)
        {
            cell.Borders.Top.Width =
                Unit.FromPoint(0.8);

            cell.Borders.Top.Color =
                Colors.Gray;

            cell.Borders.Bottom.Width =
                Unit.FromPoint(0.5);

            cell.Borders.Bottom.Color =
                Colors.Gray;

            cell.VerticalAlignment =
                VerticalAlignment.Center;

            var paragraph =
                cell.AddParagraph();

            paragraph.Format.Alignment =
                alignment;

            paragraph.Format.SpaceBefore =
                Unit.FromPoint(4);

            paragraph.Format.SpaceAfter =
                Unit.FromPoint(4);

            var formatted =
                paragraph.AddFormattedText(
                    text ?? string.Empty
                );

            formatted.Bold =
                true;

            formatted.Font.Name =
                FontName;

            formatted.Font.Size =
                Unit.FromPoint(9);
        }

        // =====================================================
        // FOOTER
        // =====================================================

        private void ComposeFooter(
            Section section)
        {
            var footer =
                section.Footers.Primary;

            var paragraph =
                footer.AddParagraph();

            paragraph.Format.Alignment =
                ParagraphAlignment.Center;

            paragraph.Format.SpaceBefore =
                Unit.FromPoint(2);

            var text =
                paragraph.AddFormattedText(
                    "Billing Receipt"
                );

            text.Font.Name =
                FontName;

            text.Font.Size =
                Unit.FromPoint(8);

            text.Color =
                Colors.Gray;
        }

        // =====================================================
        // CELL TEXT
        // =====================================================

        private void AddCellText(
            Cell cell,
            string? text)
        {
            var paragraph =
                cell.AddParagraph();

            paragraph.Format.Alignment =
                ParagraphAlignment.Left;

            paragraph.Format.SpaceBefore =
                Unit.FromPoint(0);

            paragraph.Format.SpaceAfter =
                Unit.FromPoint(2);

            var formatted =
                paragraph.AddFormattedText(
                    text ?? string.Empty
                );

            formatted.Bold =
                false;

            formatted.Font.Name =
                FontName;

            formatted.Font.Size =
                Unit.FromPoint(9);
        }

        // =====================================================
        // BOLD TEXT
        // =====================================================

        private void AddBoldText(
            Paragraph paragraph,
            string text,
            double fontSize)
        {
            var formatted =
                paragraph.AddFormattedText(
                    text ?? string.Empty
                );

            formatted.Bold =
                true;

            formatted.Font.Name =
                FontName;

            formatted.Font.Size =
                Unit.FromPoint(fontSize);
        }

        // =====================================================
        // SPACING
        // =====================================================

        private void AddSpacing(
            Section section,
            double points)
        {
            var paragraph =
                section.AddParagraph();

            paragraph.Format.SpaceBefore =
                Unit.FromPoint(0);

            paragraph.Format.SpaceAfter =
                Unit.FromPoint(points);
        }

        // =====================================================
        // HORIZONTAL LINE
        // =====================================================

        private void AddHorizontalLine(
            Section section)
        {
            var paragraph =
                section.AddParagraph();

            paragraph.Format.Borders.Bottom.Width =
                Unit.FromPoint(1);

            paragraph.Format.Borders.Bottom.Color =
                Colors.Black;

            paragraph.Format.SpaceBefore =
                Unit.FromPoint(0);

            paragraph.Format.SpaceAfter =
                Unit.FromPoint(8);
        }

        private void AddRightCellText(
    Cell cell,
    string? text)
        {
            var paragraph =
                cell.AddParagraph();

            paragraph.Format.Alignment =
                ParagraphAlignment.Right;

            paragraph.Format.SpaceBefore =
                Unit.FromPoint(0);

            paragraph.Format.SpaceAfter =
                Unit.FromPoint(2);

            var formatted =
                paragraph.AddFormattedText(
                    text ?? string.Empty
                );

            formatted.Bold =
                false;

            formatted.Font.Name =
                FontName;

            formatted.Font.Size =
                Unit.FromPoint(9);
        }
    }
}