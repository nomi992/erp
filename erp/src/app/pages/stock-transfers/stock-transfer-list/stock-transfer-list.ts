import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { TableLazyLoadEvent } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { StockTransferService } from '../../../core/stock-transfers/stock-transfer.service';
import { StockTransferListItem, StockTransferStatus } from '../../../core/stock-transfers/stock-transfer.models';
import { HasRightDirective } from '../../../core/auth/has-right.directive';
import { RightCode } from '../../../core/auth/right-code';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';

@Component({
  selector: 'app-stock-transfer-list',
  imports: [DatePipe, ButtonModule, CardModule, HasRightDirective, PrimeTemplate, TableModule, TagModule, TooltipModule],
  templateUrl: './stock-transfer-list.html',
  styleUrl: './stock-transfer-list.scss',
})
export class StockTransferList {
  protected readonly RightCode = RightCode;

  private readonly transferService = inject(StockTransferService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly transfers = signal<StockTransferListItem[]>([]);
  readonly totalRecords = signal(0);
  readonly rows = signal(25);

  load(event?: TableLazyLoadEvent): void {
    const first = event?.first ?? 0;
    const rows = event?.rows ?? this.rows();
    const pageNumber = Math.floor(first / rows) + 1;

    this.loading.set(true);
    this.transferService.getAll({ pageNumber, pageSize: rows }).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.transfers.set(response.data?.items ?? []);
        this.totalRecords.set(response.data?.totalCount ?? 0);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load stock transfers.'));
      },
    });
  }

  statusSeverity(status: StockTransferStatus): 'secondary' | 'warn' | 'success' | 'danger' | 'info' {
    switch (status) {
      case 'Draft':
        return 'secondary';
      case 'PendingApproval':
      case 'PendingReceipt':
        return 'warn';
      case 'Completed':
        return 'success';
      case 'Rejected':
      case 'Cancelled':
        return 'danger';
    }
  }

  create(): void {
    this.router.navigate(['/stock-transfers/new']);
  }

  view(item: StockTransferListItem): void {
    this.router.navigate(['/stock-transfers', item.id]);
  }

  submit(item: StockTransferListItem): void {
    this.transferService.submit(item.id).subscribe({
      next: () => {
        this.notificationService.success(`${item.transferNo} submitted for approval.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to submit.'));
      },
    });
  }

  approve(item: StockTransferListItem): void {
    this.transferService.approve(item.id).subscribe({
      next: () => {
        this.notificationService.success(`${item.transferNo} approved.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to approve.'));
      },
    });
  }

  reject(item: StockTransferListItem): void {
    this.transferService.reject(item.id).subscribe({
      next: () => {
        this.notificationService.success(`${item.transferNo} rejected.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to reject.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
