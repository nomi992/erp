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
import { ProductCategoryService } from '../../core/product-categories/product-category.service';
import { ProductCategory, ProductCategoryRequest } from '../../core/product-categories/product-category.models';
import { HasRightDirective } from '../../core/auth/has-right.directive';
import { RightCode } from '../../core/auth/right-code';
import { ApiResponse } from '../../core/models/api-response.model';
import { NotificationService } from '../../core/notifications/notification.service';

@Component({
  selector: 'app-product-categories',
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
  templateUrl: './product-categories.html',
  styleUrl: './product-categories.scss',
})
export class ProductCategories implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly categoryService = inject(ProductCategoryService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly categories = signal<ProductCategory[]>([]);
  readonly dialogVisible = signal(false);
  readonly editingCategory = signal<ProductCategory | null>(null);

  readonly parentOptions = signal<{ label: string; value: number | null }[]>([]);

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    parentProductCategoryId: this.fb.control<number | null>(null),
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.categoryService.getAll().subscribe({
      next: (response) => {
        this.loading.set(false);
        this.categories.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load product categories.'));
      },
    });
  }

  openCreateDialog(): void {
    this.editingCategory.set(null);
    this.form.reset({ name: '', parentProductCategoryId: null });
    this.setParentOptions(null);
    this.dialogVisible.set(true);
  }

  openEditDialog(category: ProductCategory): void {
    this.editingCategory.set(category);
    this.form.reset({ name: category.name, parentProductCategoryId: category.parentProductCategoryId });
    this.setParentOptions(category);
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
    const request: ProductCategoryRequest = this.form.getRawValue();
    const editing = this.editingCategory();

    const call = editing
      ? this.categoryService.update(editing.id, request)
      : this.categoryService.create(request);

    call.subscribe({
      next: () => {
        this.saving.set(false);
        this.notificationService.success(editing ? 'Product category updated successfully.' : 'Product category created successfully.');
        this.dialogVisible.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save product category.'));
      },
    });
  }

  toggleActive(category: ProductCategory): void {
    const call = category.isActive ? this.categoryService.deactivate(category.id) : this.categoryService.activate(category.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(category.isActive ? 'Product category deactivated.' : 'Product category activated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update product category status.'));
      },
    });
  }

  private setParentOptions(editing: ProductCategory | null): void {
    const options = this.categories()
      .filter((c) => c.id !== editing?.id)
      .map((c) => ({ label: c.name, value: c.id }));

    this.parentOptions.set([{ label: '(None — root category)', value: null }, ...options]);
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
