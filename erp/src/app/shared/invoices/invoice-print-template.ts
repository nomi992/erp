import { Invoice } from '../../core/invoices/invoice.models';
import { InvoiceTypeConfig } from './invoice-type-config';

export interface InvoicePrintContext {
  companyName: string | null;
  branchName: string | null;
  printedBy: string | null;
}

function escapeHtml(value: string | null | undefined): string {
  if (!value) return '';
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

function money(value: number): string {
  return value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function formatDate(value: string): string {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : date.toLocaleDateString();
}

/**
 * Renders a self-contained (no external CSS/fonts/images) HTML document for a single invoice.
 * QZ Tray prints this by rasterizing it in an offscreen browser view (`type: 'pixel', format:
 * 'html'`), so it has to look right with only inline `<style>` — same constraint as an Artifact.
 */
export function buildInvoicePrintHtml(invoice: Invoice, config: InvoiceTypeConfig, ctx: InvoicePrintContext): string {
  const documentTitle = config.singularLabel.toUpperCase();
  const partnerLabel = config.isPurchaseSide ? 'Supplier' : 'Customer';

  const rows = invoice.lines
    .map(
      (line, index) => `
        <tr>
          <td>${index + 1}</td>
          <td>${escapeHtml(line.productName)}<br/><span class="muted">${escapeHtml(line.productVariantName)}</span></td>
          <td class="num">${line.qty} ${escapeHtml(line.unitOfMeasureCode)}</td>
          <td class="num">${money(line.unitAmount)}</td>
          <td>${escapeHtml(line.taxRateName) || '—'}</td>
          <td class="num">${money(line.taxAmount)}</td>
          <td class="num">${money(line.lineTotal)}</td>
        </tr>`,
    )
    .join('');

  return `
    <!doctype html>
    <html>
      <head>
        <meta charset="utf-8" />
        <style>
          * { box-sizing: border-box; }
          body { font-family: Arial, Helvetica, sans-serif; color: #1a1a1a; margin: 0; padding: 24px; font-size: 13px; }
          .muted { color: #666; font-size: 11px; }
          .header { display: flex; justify-content: space-between; align-items: flex-start; border-bottom: 2px solid #1a1a1a; padding-bottom: 12px; margin-bottom: 16px; }
          .company { font-size: 20px; font-weight: bold; }
          .branch { font-size: 12px; color: #444; }
          .doc-title { text-align: right; }
          .doc-title h1 { margin: 0; font-size: 18px; letter-spacing: 1px; }
          .doc-title .invoice-no { font-size: 14px; font-weight: bold; margin-top: 4px; }
          .meta { display: flex; justify-content: space-between; margin-bottom: 16px; gap: 24px; }
          .meta-block div { margin-bottom: 3px; }
          .meta-block .label { color: #666; display: inline-block; min-width: 110px; }
          table.lines { width: 100%; border-collapse: collapse; margin-bottom: 16px; }
          table.lines th, table.lines td { border: 1px solid #ccc; padding: 6px 8px; font-size: 12px; text-align: left; vertical-align: top; }
          table.lines th { background: #f2f2f2; }
          .num { text-align: right; white-space: nowrap; }
          .totals { width: 260px; margin-left: auto; }
          .totals div { display: flex; justify-content: space-between; padding: 3px 0; }
          .totals .grand { font-weight: bold; font-size: 15px; border-top: 2px solid #1a1a1a; margin-top: 4px; padding-top: 6px; }
          .narration { margin-top: 16px; font-size: 12px; color: #333; }
          .footer { margin-top: 32px; font-size: 10px; color: #888; border-top: 1px solid #ddd; padding-top: 8px; }
        </style>
      </head>
      <body>
        <div class="header">
          <div>
            <div class="company">${escapeHtml(ctx.companyName) || 'Company'}</div>
            ${ctx.branchName ? `<div class="branch">${escapeHtml(ctx.branchName)}</div>` : ''}
          </div>
          <div class="doc-title">
            <h1>${documentTitle}</h1>
            <div class="invoice-no">${escapeHtml(invoice.invoiceNo)}</div>
            ${invoice.linkedVoucherNo ? `<div class="muted">GL Voucher: ${escapeHtml(invoice.linkedVoucherNo)}</div>` : ''}
          </div>
        </div>

        <div class="meta">
          <div class="meta-block">
            <div><span class="label">${partnerLabel}:</span> ${escapeHtml(invoice.partnerName)}</div>
            <div><span class="label">Warehouse:</span> ${escapeHtml(invoice.warehouseName)}</div>
            ${invoice.externalReferenceNo ? `<div><span class="label">External Ref:</span> ${escapeHtml(invoice.externalReferenceNo)}</div>` : ''}
            ${invoice.referenceInvoiceNo ? `<div><span class="label">Reference:</span> ${escapeHtml(invoice.referenceInvoiceNo)}</div>` : ''}
          </div>
          <div class="meta-block">
            <div><span class="label">Date:</span> ${formatDate(invoice.date)}</div>
            <div><span class="label">Due Date:</span> ${formatDate(invoice.dueDate)}</div>
            <div><span class="label">Payment Mode:</span> ${escapeHtml(invoice.paymentMode)}</div>
            <div><span class="label">Status:</span> ${escapeHtml(invoice.status)}</div>
          </div>
        </div>

        <table class="lines">
          <thead>
            <tr>
              <th>#</th>
              <th>Item</th>
              <th class="num">Qty</th>
              <th class="num">Unit Price</th>
              <th>Tax</th>
              <th class="num">Tax Amt</th>
              <th class="num">Line Total</th>
            </tr>
          </thead>
          <tbody>${rows}</tbody>
        </table>

        <div class="totals">
          <div><span>Net Total</span><span>${money(invoice.totalNet)}</span></div>
          <div><span>Tax Total</span><span>${money(invoice.totalTax)}</span></div>
          <div class="grand"><span>Grand Total</span><span>${money(invoice.totalAmount)}</span></div>
          ${!config.isOrder ? `<div><span>Amount Paid</span><span>${money(invoice.amountPaid)}</span></div>
          <div><span>Outstanding</span><span>${money(invoice.outstandingAmount)}</span></div>` : ''}
        </div>

        ${invoice.narration ? `<div class="narration"><strong>Narration:</strong> ${escapeHtml(invoice.narration)}</div>` : ''}

        <div class="footer">
          Printed ${new Date().toLocaleString()}${ctx.printedBy ? ` by ${escapeHtml(ctx.printedBy)}` : ''}
        </div>
      </body>
    </html>`;
}
