import { Component, OnInit, inject, input, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { BusinessPartnerService } from '../../core/business-partners/business-partner.service';
import { BusinessPartner, BusinessPartnerRequest, PartnerType } from '../../core/business-partners/business-partner.models';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';
import { BusinessPartnerListConfig } from './business-partner-list.models';

@Component({
  selector: 'app-business-partner-list',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    DialogModule,
    HasRightDirective,
    InputNumberModule,
    InputTextModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
  templateUrl: './business-partner-list.html',
  styleUrl: './business-partner-list.scss',
})
export class BusinessPartnerList implements OnInit {
  readonly config = input.required<BusinessPartnerListConfig>();

  private readonly fb = inject(FormBuilder);
  private readonly partnerService = inject(BusinessPartnerService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly partners = signal<BusinessPartner[]>([]);
  readonly dialogVisible = signal(false);
  readonly editingPartner = signal<BusinessPartner | null>(null);

  readonly partnerTypeOptions = signal<{ label: string; value: PartnerType }[]>([]);

  form = this.fb.nonNullable.group({
    partnerType: this.fb.nonNullable.control<PartnerType>('Supplier', [Validators.required]),
    code: ['', [Validators.required]],
    name: ['', [Validators.required]],
    contactPerson: this.fb.control<string | null>(null),
    phone: this.fb.control<string | null>(null),
    email: this.fb.control<string | null>(null),
    address: this.fb.control<string | null>(null),
    defaultPaymentTermDays: this.fb.nonNullable.control<number>(0),
    creditLimit: this.fb.control<number | null>(null),
  });

  ngOnInit(): void {
    this.partnerTypeOptions.set(this.config().allowedPartnerTypes.map((value) => ({ label: value, value })));
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.partnerService.getAll(this.config().listPartnerType).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.partners.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, `Unable to load ${this.config().title.toLowerCase()}.`));
      },
    });
  }

  openCreateDialog(): void {
    this.editingPartner.set(null);
    this.form.reset({
      partnerType: this.config().listPartnerType,
      code: '',
      name: '',
      contactPerson: null,
      phone: null,
      email: null,
      address: null,
      defaultPaymentTermDays: 0,
      creditLimit: null,
    });
    this.dialogVisible.set(true);
  }

  openEditDialog(partner: BusinessPartner): void {
    this.editingPartner.set(partner);
    this.form.reset({ ...partner });
    this.dialogVisible.set(true);
  }

  closeDialog(): void {
    this.dialogVisible.set(false);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const request: BusinessPartnerRequest = this.form.getRawValue();
    const editing = this.editingPartner();

    const call = editing ? this.partnerService.update(editing.id, request) : this.partnerService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(editing ? 'Saved successfully.' : 'Created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save.'));
      },
    });
  }

  toggleActive(partner: BusinessPartner): void {
    const call = partner.isActive ? this.partnerService.deactivate(partner.id) : this.partnerService.activate(partner.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(partner.isActive ? 'Deactivated.' : 'Activated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update status.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
