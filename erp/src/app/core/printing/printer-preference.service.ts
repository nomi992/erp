import { Injectable, signal } from '@angular/core';

const STORAGE_KEY = 'erp.printing.printerPreferences';

type PrinterPreferences = Record<string, string>;

function readStored(): PrinterPreferences {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return {};
  }

  try {
    return JSON.parse(raw) as PrinterPreferences;
  } catch {
    return {};
  }
}

/**
 * Remembers the last QZ Tray printer picked for a given document key (e.g. a `InvoiceType` like
 * `'SalesInvoice'`) so the user isn't asked to reselect it on every print. Follows the same
 * signal-backed-by-localStorage shape as `AuthService`/`TenancyService`.
 */
@Injectable({ providedIn: 'root' })
export class PrinterPreferenceService {
  private readonly preferences = signal<PrinterPreferences>(readStored());

  getPreferred(docKey: string): string | null {
    return this.preferences()[docKey] ?? null;
  }

  setPreferred(docKey: string, printerName: string): void {
    const updated = { ...this.preferences(), [docKey]: printerName };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
    this.preferences.set(updated);
  }
}
