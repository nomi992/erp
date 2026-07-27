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
import { InputTextModule } from 'primeng/inputtext';
import { StockAdjustmentService } from '../../../core/stock-adjustments/stock-adjustment.service';
import { StockAdjustmentListItem, StockAdjustmentStatus } from '../../../core/stock-adjustments/stock-adjustment.models';
import { HasRightDirective } from '../../../core/auth/has-right.directive';
import { RightCode } from '../../../core/auth/right-code';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';

@Component({
  selector: 'app-stock-adjustment-list',
  imports: [DatePipe, ButtonModule, CardModule, HasRightDirective, PrimeTemplate, TableModule, TagModule, TooltipModule, InputTextModule],
  templateUrl: './stock-adjustment-list.html',
  styleUrl: './stock-adjustment-list.scss',
})
export class StockAdjustmentList {
  protected readonly RightCode = RightCode;

  private readonly adjustmentService = inject(StockAdjustmentService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly adjustments = signal<StockAdjustmentListItem[]>([]);
  readonly totalRecords = signal(0);
  readonly rows = signal(25);
  readonly searchTerm = signal('');

  private lastEvent?: TableLazyLoadEvent;

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
    this.load(this.lastEvent);
  }

  load(event?: TableLazyLoadEvent): void {
    this.lastEvent = event;
    const first = event?.first ?? 0;
    const rows = event?.rows ?? this.rows();
    const pageNumber = Math.floor(first / rows) + 1;

    const sortField = Array.isArray(event?.sortField) ? event?.sortField[0] : event?.sortField;
    const sortBy = sortField ?? undefined;
    const sortDirection = event?.sortOrder === 1 ? 'asc' : event?.sortOrder === -1 ? 'desc' : undefined;

    this.loading.set(true);
    this.adjustmentService
      .getAll({ pageNumber, pageSize: rows, search: this.searchTerm() || undefined, sortBy, sortDirection })
      .subscribe({
        next: (response) => {
          this.loading.set(false);
          this.adjustments.set(response.data?.items ?? []);
          this.totalRecords.set(response.data?.totalCount ?? 0);
        },
        error: (error: HttpErrorResponse) => {
          this.loading.set(false);
          this.notificationService.error(this.extractErrorMessage(error, 'Unable to load stock adjustments.'));
        },
      });
  }

  statusSeverity(status: StockAdjustmentStatus): 'secondary' | 'warn' | 'success' | 'danger' {
    switch (status) {
      case 'Draft':
        return 'secondary';
      case 'PendingApproval':
        return 'warn';
      case 'Posted':
        return 'success';
      case 'Rejected':
        return 'danger';
    }
  }

  create(): void {
    this.router.navigate(['/stock-adjustments/new']);
  }

  view(item: StockAdjustmentListItem): void {
    this.router.navigate(['/stock-adjustments', item.id]);
  }

  submit(item: StockAdjustmentListItem): void {
    this.adjustmentService.submit(item.id).subscribe({
      next: () => {
        this.notificationService.success(`${item.adjustmentNo} submitted for approval.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to submit.'));
      },
    });
  }

  approve(item: StockAdjustmentListItem): void {
    this.adjustmentService.approve(item.id).subscribe({
      next: () => {
        this.notificationService.success(`${item.adjustmentNo} approved.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to approve.'));
      },
    });
  }

  reject(item: StockAdjustmentListItem): void {
    this.adjustmentService.reject(item.id).subscribe({
      next: () => {
        this.notificationService.success(`${item.adjustmentNo} rejected.`);
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
