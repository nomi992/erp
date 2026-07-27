import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { StockAccountMappingService } from '../../core/stock-account-mappings/stock-account-mapping.service';
import { StockAccountMapping, StockAccountMappingRequest } from '../../core/stock-account-mappings/stock-account-mapping.models';
import { ProductCategoryService } from '../../core/product-categories/product-category.service';
import { AccountService } from '../../core/accounts/account.service';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { RightCode } from '../../core/auth/right-code';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';

@Component({
  selector: 'app-stock-account-mappings',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    DialogModule,
    HasRightDirective,
    InputTextModule,
    PrimeTemplate,
    SelectModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
  templateUrl: './stock-account-mappings.html',
  styleUrl: './stock-account-mappings.scss',
})
export class StockAccountMappings implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly mappingService = inject(StockAccountMappingService);
  private readonly categoryService = inject(ProductCategoryService);
  private readonly accountService = inject(AccountService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly mappings = signal<StockAccountMapping[]>([]);
  readonly dialogVisible = signal(false);
  readonly editingMapping = signal<StockAccountMapping | null>(null);

  readonly categoryOptions = signal<{ label: string; value: number | null }[]>([{ label: '(Tenant-wide default)', value: null }]);
  readonly accountOptions = signal<{ label: string; value: number }[]>([]);
  readonly optionalAccountOptions = signal<{ label: string; value: number | null }[]>([{ label: '(None)', value: null }]);

  form = this.fb.nonNullable.group({
    productCategoryId: this.fb.control<number | null>(null),
    inventoryAssetAccountId: this.fb.control<number | null>(null, [Validators.required]),
    cogsAccountId: this.fb.control<number | null>(null, [Validators.required]),
    accountsPayableAccountId: this.fb.control<number | null>(null, [Validators.required]),
    salesRevenueAccountId: this.fb.control<number | null>(null, [Validators.required]),
    accountsReceivableAccountId: this.fb.control<number | null>(null, [Validators.required]),
    cashOrBankAccountId: this.fb.control<number | null>(null),
    inputTaxAccountId: this.fb.control<number | null>(null),
    outputTaxAccountId: this.fb.control<number | null>(null),
    stockAdjustmentVarianceAccountId: this.fb.control<number | null>(null, [Validators.required]),
    openingBalanceEquityAccountId: this.fb.control<number | null>(null, [Validators.required]),
  });

  ngOnInit(): void {
    this.load();
    this.categoryService.getAll().subscribe({
      next: (response) => {
        this.categoryOptions.set([
          { label: '(Tenant-wide default)', value: null },
          ...(response.data ?? []).map((c) => ({ label: c.name, value: c.id as number | null })),
        ]);
      },
    });
    this.accountService.getAll().subscribe({
      next: (response) => {
        const options = (response.data ?? []).map((a) => ({ label: `${a.code} - ${a.name}`, value: a.id }));
        this.accountOptions.set(options);
        this.optionalAccountOptions.set([{ label: '(None)', value: null }, ...options]);
      },
    });
  }

  load(): void {
    this.loading.set(true);
    this.mappingService.getAll().subscribe({
      next: (response) => {
        this.loading.set(false);
        this.mappings.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load stock account mappings.'));
      },
    });
  }

  openCreateDialog(): void {
    this.editingMapping.set(null);
    this.form.reset({
      productCategoryId: null,
      inventoryAssetAccountId: null,
      cogsAccountId: null,
      accountsPayableAccountId: null,
      salesRevenueAccountId: null,
      accountsReceivableAccountId: null,
      cashOrBankAccountId: null,
      inputTaxAccountId: null,
      outputTaxAccountId: null,
      stockAdjustmentVarianceAccountId: null,
      openingBalanceEquityAccountId: null,
    });
    this.dialogVisible.set(true);
  }

  openEditDialog(mapping: StockAccountMapping): void {
    this.editingMapping.set(mapping);
    this.form.reset({ ...mapping });
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
    const request = this.form.getRawValue() as StockAccountMappingRequest;
    const editing = this.editingMapping();

    const call = editing ? this.mappingService.update(editing.id, request) : this.mappingService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(editing ? 'Stock account mapping updated successfully.' : 'Stock account mapping created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save stock account mapping.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
