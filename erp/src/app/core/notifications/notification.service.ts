import { Injectable, inject } from '@angular/core';
import { MessageService } from 'primeng/api';

const DEFAULT_LIFE_MS = 4000;

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly messageService = inject(MessageService);

  success(detail: string, summary = 'Success'): void {
    this.show('success', summary, detail);
  }

  error(detail: string, summary = 'Error'): void {
    this.show('error', summary, detail);
  }

  info(detail: string, summary = 'Info'): void {
    this.show('info', summary, detail);
  }

  warn(detail: string, summary = 'Warning'): void {
    this.show('warn', summary, detail);
  }

  private show(severity: 'success' | 'error' | 'info' | 'warn', summary: string, detail: string): void {
    this.messageService.add({ severity, summary, detail, life: DEFAULT_LIFE_MS });
  }
}
