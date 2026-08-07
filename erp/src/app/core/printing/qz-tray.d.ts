// Minimal ambient typings for the `qz-tray` package (it ships no `.d.ts` of its own).
// Only the surface this app actually calls is declared — extend as needed.
declare module 'qz-tray' {
  export interface QzWebsocketConnectOptions {
    host?: string | string[];
    port?: { secure?: number[]; insecure?: number[] };
    usingSecure?: boolean;
    keepAlive?: number;
    retries?: number;
    delay?: number;
  }

  export interface QzPrintConfigOptions {
    copies?: number;
    duplex?: boolean | string;
    orientation?: 'portrait' | 'landscape' | 'reverse-landscape' | null;
    jobName?: string;
    density?: number;
    scaleContent?: boolean;
    rasterize?: boolean;
    units?: 'in' | 'cm' | 'mm';
    margins?: number | { top?: number; right?: number; bottom?: number; left?: number };
    colorType?: 'color' | 'grayscale' | 'blackwhite' | 'default';
  }

  export interface QzPrintConfig {
    // opaque handle returned by qz.configs.create(); passed straight back into qz.print()
  }

  export type QzPrintDataFormat = 'plain' | 'file' | 'base64' | 'hex' | 'xml';
  export type QzPrintDataType = 'pixel' | 'raw' | 'html' | 'image' | 'pdf';

  export interface QzPrintData {
    type: QzPrintDataType;
    format?: 'html' | 'plain' | 'pdf' | 'image' | string;
    flavor?: QzPrintDataFormat;
    data: string;
    options?: Record<string, unknown>;
  }

  const qz: {
    websocket: {
      isActive(): boolean;
      connect(options?: QzWebsocketConnectOptions): Promise<void>;
      disconnect(): Promise<void>;
    };
    printers: {
      find(query?: string): Promise<string[] | string>;
      getDefault(): Promise<string>;
    };
    configs: {
      create(printer: string | null, options?: QzPrintConfigOptions): QzPrintConfig;
    };
    print(config: QzPrintConfig, data: QzPrintData[]): Promise<void>;
    security: {
      setCertificatePromise(handler: (resolve: (cert: string) => void, reject: (err?: unknown) => void) => void): void;
      setSignaturePromise(
        factory: (toSign: string) => (resolve: (signature: string) => void, reject: (err?: unknown) => void) => void,
      ): void;
      setSignatureAlgorithm(algorithm: 'SHA1' | 'SHA256' | 'SHA512'): void;
    };
    version: string;
  };

  export default qz;
}
