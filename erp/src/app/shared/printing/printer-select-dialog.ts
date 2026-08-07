import { Component, effect, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { PrinterPreferenceService } from '../../core/printing/printer-preference.service';
import { QzTrayService } from '../../core/printing/qz-tray.service';

/**
 * Lets the user pick which QZ Tray printer to send a document to, remembering the choice (keyed
 * by `docKey`, e.g. an `InvoiceType`) so it isn't asked again next time. Used by both the invoice
 * list and the invoice form for the "Print" action.
 */
@Component({
  selector: 'app-printer-select-dialog',
  imports: [FormsModule, ButtonModule, DialogModule, SelectModule],
  templateUrl: './printer-select-dialog.html',
  styleUrl: './printer-select-dialog.scss',
})
export class PrinterSelectDialog {
  readonly visible = input.required<boolean>();
  readonly visibleChange = output<boolean>();
  readonly docKey = input.required<string>();
  readonly header = input<string>('Select Printer');
  readonly confirmed = output<string>();

  private readonly qzTrayService = inject(QzTrayService);
  private readonly printerPreferenceService = inject(PrinterPreferenceService);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly printerOptions = signal<{ label: string; value: string }[]>([]);
  readonly selectedPrinter = signal<string | null>(null);

  constructor() {
    effect(() => {
      if (this.visible()) {
        this.discoverPrinters();
      }
    });
  }

  retry(): void {
    this.discoverPrinters();
  }

  close(): void {
    this.visibleChange.emit(false);
  }

  confirm(): void {
    const printer = this.selectedPrinter();
    if (!printer) return;

    this.printerPreferenceService.setPreferred(this.docKey(), printer);
    this.confirmed.emit(printer);
    this.visibleChange.emit(false);
  }

  private discoverPrinters(): void {
    this.loading.set(true);
    this.error.set(null);

    this.qzTrayService
      .listPrinters()
      .then((printers) => {
        this.loading.set(false);
        this.printerOptions.set(printers.map((name) => ({ label: name, value: name })));

        const preferred = this.printerPreferenceService.getPreferred(this.docKey());
        this.selectedPrinter.set(preferred && printers.includes(preferred) ? preferred : (printers[0] ?? null));
      })
      .catch((err: unknown) => {
        this.loading.set(false);
        this.printerOptions.set([]);
        this.selectedPrinter.set(null);
        this.error.set(err instanceof Error ? err.message : 'Unable to list printers.');
      });
  }
}
