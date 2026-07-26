import { Component, inject, input, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { TableLazyLoadEvent } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { PartnerPaymentService } from '../../core/partner-payments/partner-payment.service';
import { PartnerPaymentListItem, PartnerPaymentStatus } from '../../core/partner-payments/partner-payment.models';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';
import { PartnerPaymentConfig } from './partner-payment-config';

@Component({
  selector: 'app-partner-payment-list',
  imports: [DatePipe, DecimalPipe, ButtonModule, CardModule, HasRightDirective, PrimeTemplate, TableModule, TagModule, TooltipModule],
  templateUrl: './partner-payment-list.html',
  styleUrl: './partner-payment-list.scss',
})
export class PartnerPaymentList {
  readonly config = input.required<PartnerPaymentConfig>();

  private readonly paymentService = inject(PartnerPaymentService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly payments = signal<PartnerPaymentListItem[]>([]);
  readonly totalRecords = signal(0);
  readonly rows = signal(25);

  load(event?: TableLazyLoadEvent): void {
    const first = event?.first ?? 0;
    const rows = event?.rows ?? this.rows();
    const pageNumber = Math.floor(first / rows) + 1;

    this.loading.set(true);
    this.paymentService.getAll({ direction: this.config().direction, pageNumber, pageSize: rows }).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.payments.set(response.data?.items ?? []);
        this.totalRecords.set(response.data?.totalCount ?? 0);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, `Unable to load ${this.config().title.toLowerCase()}.`));
      },
    });
  }

  statusSeverity(status: PartnerPaymentStatus): 'secondary' | 'warn' | 'success' | 'danger' {
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
    this.router.navigate([this.config().formRoute, 'new']);
  }

  view(item: PartnerPaymentListItem): void {
    this.router.navigate([this.config().formRoute, item.id]);
  }

  submit(item: PartnerPaymentListItem): void {
    this.paymentService.submit(item.id).subscribe({
      next: () => {
        this.notificationService.success(`${item.paymentNo} submitted for approval.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to submit.'));
      },
    });
  }

  approve(item: PartnerPaymentListItem): void {
    this.paymentService.approve(item.id).subscribe({
      next: () => {
        this.notificationService.success(`${item.paymentNo} approved.`);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to approve.'));
      },
    });
  }

  reject(item: PartnerPaymentListItem): void {
    this.paymentService.reject(item.id).subscribe({
      next: () => {
        this.notificationService.success(`${item.paymentNo} rejected.`);
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
