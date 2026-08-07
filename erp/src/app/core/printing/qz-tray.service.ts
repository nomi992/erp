import { Injectable } from '@angular/core';
import qz from 'qz-tray';

/**
 * Thin wrapper around the QZ Tray browser client (https://qz.io).
 *
 * QZ Tray is a small desktop app the user installs once on the machine doing the printing; this
 * service talks to it over a local WebSocket (ws://localhost:8181/8182 by default) to print
 * straight to any printer registered with the OS — including receipt/label printers that aren't
 * reachable from the browser's own `window.print()`.
 *
 * No certificate/signing is configured here, so QZ Tray will show a one-time "allow this site?"
 * prompt per browser session (the user can tick "remember this decision"). For unattended/kiosk
 * use, wire up `qz.security.setCertificatePromise` / `setSignaturePromise` against a backend
 * signing endpoint — see https://qz.io/wiki/signing-messages.
 */
@Injectable({ providedIn: 'root' })
export class QzTrayService {
  private connectPromise: Promise<void> | null = null;

  get isActive(): boolean {
    return qz.websocket.isActive();
  }

  connect(): Promise<void> {
    if (this.isActive) {
      return Promise.resolve();
    }

    if (!this.connectPromise) {
      this.connectPromise = qz.websocket
        .connect()
        .catch(() => {
          throw new Error('Unable to reach QZ Tray. Make sure it is installed and running on this computer.');
        })
        .finally(() => {
          this.connectPromise = null;
        });
    }

    return this.connectPromise;
  }

  async listPrinters(): Promise<string[]> {
    await this.connect();
    const result = await qz.printers.find();
    return Array.isArray(result) ? result : [result];
  }

  async printHtml(printerName: string, html: string, copies = 1): Promise<void> {
    await this.connect();
    const config = qz.configs.create(printerName, { copies, scaleContent: true, rasterize: true });
    await qz.print(config, [{ type: 'pixel', format: 'html', flavor: 'plain', data: html }]);
  }
}
