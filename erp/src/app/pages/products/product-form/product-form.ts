import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { PrimeTemplate } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ProductService } from '../../../core/products/product.service';
import {
  Product,
  ProductRequest,
  ProductVariant,
  ProductVariantPriceRequest,
  ProductVariantRequest,
  UOMConversion,
  UOMConversionRequest,
} from '../../../core/products/product.models';
import { ProductCategoryService } from '../../../core/product-categories/product-category.service';
import { UnitOfMeasureService } from '../../../core/units-of-measure/unit-of-measure.service';
import { HasRightDirective } from '../../../core/auth/has-right.directive';
import { RightCode } from '../../../core/auth/right-code';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';

interface VariantFormControls {
  name: FormControl<string>;
  variantCode: FormControl<string>;
  barcode: FormControl<string | null>;
  isDefault: FormControl<boolean>;
}

interface ConversionFormControls {
  unitOfMeasureId: FormControl<number | null>;
  conversionFactor: FormControl<number>;
}

@Component({
  selector: 'app-product-form',
  imports: [
    ReactiveFormsModule,
    DecimalPipe,
    ButtonModule,
    CardModule,
    CheckboxModule,
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
  templateUrl: './product-form.html',
  styleUrl: './product-form.scss',
})
export class ProductForm implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(ProductCategoryService);
  private readonly unitService = inject(UnitOfMeasureService);
  private readonly notificationService = inject(NotificationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly productId = signal<number | null>(null);
  readonly product = signal<Product | null>(null);

  readonly categoryOptions = signal<{ label: string; value: number | null }[]>([{ label: '(None)', value: null }]);
  readonly uomOptions = signal<{ label: string; value: number | null }[]>([]);

  // Create-mode only: initial variants/conversions travel inside the create request.
  readonly variantsArray = this.fb.array<FormGroup<VariantFormControls>>([]);
  readonly conversionsArray = this.fb.array<FormGroup<ConversionFormControls>>([]);

  form = this.fb.nonNullable.group({
    sku: ['', [Validators.required]],
    name: ['', [Validators.required]],
    productCategoryId: this.fb.control<number | null>(null),
    baseUnitOfMeasureId: this.fb.control<number | null>(null, [Validators.required]),
    hasVariants: this.fb.nonNullable.control<boolean>(false),
    isStockTracked: this.fb.nonNullable.control<boolean>(true),
    reorderLevel: this.fb.control<number | null>(null),
  });

  // --- Edit-mode sub-dialogs: variants, conversions, and per-variant pricing are managed through
  // their own endpoints once a product already exists (the backend doesn't accept them via update).
  readonly variantDialogVisible = signal(false);
  readonly editingVariant = signal<ProductVariant | null>(null);
  variantForm = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    variantCode: ['', [Validators.required]],
    barcode: this.fb.control<string | null>(null),
    isDefault: this.fb.nonNullable.control<boolean>(false),
  });

  readonly priceDialogVisible = signal(false);
  readonly editingVariantForPrices = signal<ProductVariant | null>(null);
  priceForm = this.fb.nonNullable.group({
    purchaseAmount: this.fb.nonNullable.control<number>(0),
    retailAmount: this.fb.nonNullable.control<number>(0),
    saleAmount: this.fb.nonNullable.control<number>(0),
  });

  readonly conversionDialogVisible = signal(false);
  readonly editingConversion = signal<UOMConversion | null>(null);
  conversionForm = this.fb.nonNullable.group({
    unitOfMeasureId: this.fb.control<number | null>(null, [Validators.required]),
    conversionFactor: this.fb.nonNullable.control<number>(1, [Validators.required]),
  });

  ngOnInit(): void {
    this.loadLookups();

    if (this.variantsArray.length === 0) {
      this.variantsArray.push(this.createVariantGroup());
    }

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.productId.set(Number(idParam));
      this.loadProduct(Number(idParam));
    }
  }

  get isEditMode(): boolean {
    return this.productId() !== null;
  }

  addVariantRow(): void {
    this.variantsArray.push(this.createVariantGroup());
  }

  removeVariantRow(index: number): void {
    if (this.variantsArray.length > 1) {
      this.variantsArray.removeAt(index);
    }
  }

  addConversionRow(): void {
    this.conversionsArray.push(this.createConversionGroup());
  }

  removeConversionRow(index: number): void {
    this.conversionsArray.removeAt(index);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const id = this.productId();

    const request: ProductRequest = {
      sku: raw.sku,
      name: raw.name,
      productCategoryId: raw.productCategoryId,
      baseUnitOfMeasureId: raw.baseUnitOfMeasureId!,
      hasVariants: raw.hasVariants,
      isStockTracked: raw.isStockTracked,
      reorderLevel: raw.reorderLevel,
      // The backend ignores Variants entirely (and seeds its own single "Default" variant) whenever
      // hasVariants is false - so the always-present blank row from variantsArray must not be sent
      // in that case, or its empty Name/VariantCode trip ASP.NET's automatic [Required] validation
      // before ProductRepository.CreateAsync ever gets a chance to apply that "ignore" behavior.
      variants: id || !raw.hasVariants
        ? []
        : this.variantsArray.getRawValue().map((v) => ({
            name: v.name,
            variantCode: v.variantCode,
            barcode: v.barcode,
            isDefault: v.isDefault,
          })),
      uomConversions: id
        ? []
        : this.conversionsArray.getRawValue()
            .filter((c) => c.unitOfMeasureId)
            .map((c) => ({ unitOfMeasureId: c.unitOfMeasureId!, conversionFactor: c.conversionFactor })),
    };

    this.saving.set(true);
    const call = id ? this.productService.update(id, request) : this.productService.create(request);

    call.subscribe({
      next: (response) => {
        this.saving.set(false);
        this.notificationService.success(id ? 'Product updated successfully.' : 'Product created successfully.');
        if (!id && response.data) {
          this.router.navigate(['/products', response.data.id, 'edit']);
        } else {
          this.router.navigate(['/products']);
        }
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save product.'));
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/products']);
  }

  // --- Variants (edit mode) ---

  openAddVariantDialog(): void {
    this.editingVariant.set(null);
    this.variantForm.reset({ name: '', variantCode: '', barcode: null, isDefault: false });
    this.variantDialogVisible.set(true);
  }

  openEditVariantDialog(variant: ProductVariant): void {
    this.editingVariant.set(variant);
    this.variantForm.reset({
      name: variant.name,
      variantCode: variant.variantCode,
      barcode: variant.barcode,
      isDefault: variant.isDefault,
    });
    this.variantDialogVisible.set(true);
  }

  closeVariantDialog(): void {
    this.variantDialogVisible.set(false);
  }

  submitVariant(): void {
    const id = this.productId();
    if (!id || this.variantForm.invalid) {
      this.variantForm.markAllAsTouched();
      return;
    }

    const request: ProductVariantRequest = this.variantForm.getRawValue();
    const editing = this.editingVariant();

    const call = editing
      ? this.productService.updateVariant(id, editing.id, request)
      : this.productService.addVariant(id, request);

    call.subscribe({
      next: () => {
        this.notificationService.success(editing ? 'Variant updated.' : 'Variant added.');
        this.variantDialogVisible.set(false);
        this.refreshProduct(id);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save variant.'));
      },
    });
  }

  toggleVariantActive(variant: ProductVariant): void {
    const id = this.productId();
    if (!id) return;

    const call = variant.isActive
      ? this.productService.deactivateVariant(id, variant.id)
      : this.productService.activateVariant(id, variant.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(variant.isActive ? 'Variant deactivated.' : 'Variant activated.');
        this.refreshProduct(id);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update variant status.'));
      },
    });
  }

  // --- Variant pricing (edit mode) ---

  openPriceDialog(variant: ProductVariant): void {
    this.editingVariantForPrices.set(variant);
    this.priceForm.reset({
      purchaseAmount: variant.prices.find((p) => p.priceType === 'Purchase')?.amount ?? 0,
      retailAmount: variant.prices.find((p) => p.priceType === 'Retail')?.amount ?? 0,
      saleAmount: variant.prices.find((p) => p.priceType === 'Sale')?.amount ?? 0,
    });
    this.priceDialogVisible.set(true);
  }

  closePriceDialog(): void {
    this.priceDialogVisible.set(false);
  }

  submitPrices(): void {
    const id = this.productId();
    const variant = this.editingVariantForPrices();
    if (!id || !variant) return;

    const raw = this.priceForm.getRawValue();
    const prices: ProductVariantPriceRequest[] = [
      { priceType: 'Purchase', amount: raw.purchaseAmount },
      { priceType: 'Retail', amount: raw.retailAmount },
      { priceType: 'Sale', amount: raw.saleAmount },
    ];

    this.productService.setVariantPrices(id, variant.id, prices).subscribe({
      next: () => {
        this.notificationService.success('Prices updated.');
        this.priceDialogVisible.set(false);
        this.refreshProduct(id);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save prices.'));
      },
    });
  }

  // --- UOM conversions (edit mode) ---

  openAddConversionDialog(): void {
    this.editingConversion.set(null);
    this.conversionForm.reset({ unitOfMeasureId: null, conversionFactor: 1 });
    this.conversionDialogVisible.set(true);
  }

  openEditConversionDialog(conversion: UOMConversion): void {
    this.editingConversion.set(conversion);
    this.conversionForm.reset({ unitOfMeasureId: conversion.unitOfMeasureId, conversionFactor: conversion.conversionFactor });
    this.conversionDialogVisible.set(true);
  }

  closeConversionDialog(): void {
    this.conversionDialogVisible.set(false);
  }

  submitConversion(): void {
    const id = this.productId();
    if (!id || this.conversionForm.invalid) {
      this.conversionForm.markAllAsTouched();
      return;
    }

    const raw = this.conversionForm.getRawValue();
    const request: UOMConversionRequest = { unitOfMeasureId: raw.unitOfMeasureId!, conversionFactor: raw.conversionFactor };
    const editing = this.editingConversion();

    const call = editing
      ? this.productService.updateConversion(id, editing.id, request)
      : this.productService.addConversion(id, request);

    call.subscribe({
      next: () => {
        this.notificationService.success(editing ? 'Conversion updated.' : 'Conversion added.');
        this.conversionDialogVisible.set(false);
        this.refreshProduct(id);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to save conversion.'));
      },
    });
  }

  deleteConversion(conversion: UOMConversion): void {
    const id = this.productId();
    if (!id) return;

    this.productService.deleteConversion(id, conversion.id).subscribe({
      next: () => {
        this.notificationService.success('Conversion removed.');
        this.refreshProduct(id);
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to remove conversion.'));
      },
    });
  }

  private createVariantGroup(): FormGroup<VariantFormControls> {
    return this.fb.group({
      name: this.fb.nonNullable.control('', [Validators.required]),
      variantCode: this.fb.nonNullable.control('', [Validators.required]),
      barcode: this.fb.control<string | null>(null),
      isDefault: this.fb.nonNullable.control(false),
    });
  }

  private createConversionGroup(): FormGroup<ConversionFormControls> {
    return this.fb.group({
      unitOfMeasureId: this.fb.control<number | null>(null),
      conversionFactor: this.fb.nonNullable.control(1),
    });
  }

  private loadLookups(): void {
    forkJoin({
      categories: this.categoryService.getAll(),
      units: this.unitService.getAll(),
    }).subscribe({
      next: ({ categories, units }) => {
        this.categoryOptions.set([
          { label: '(None)', value: null },
          ...(categories.data ?? []).map((c) => ({ label: c.name, value: c.id as number | null })),
        ]);
        this.uomOptions.set((units.data ?? []).map((u) => ({ label: `${u.code} - ${u.name}`, value: u.id as number | null })));
      },
    });
  }

  private loadProduct(id: number): void {
    this.loading.set(true);
    this.productService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        const product = response.data;
        if (!product) {
          this.notificationService.error('Product not found.');
          this.router.navigate(['/products']);
          return;
        }
        this.applyProduct(product);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load product.'));
        this.router.navigate(['/products']);
      },
    });
  }

  private refreshProduct(id: number): void {
    this.productService.getById(id).subscribe({
      next: (response) => {
        if (response.data) {
          this.applyProduct(response.data);
        }
      },
    });
  }

  private applyProduct(product: Product): void {
    this.product.set(product);
    this.form.patchValue({
      sku: product.sku,
      name: product.name,
      productCategoryId: product.productCategoryId,
      baseUnitOfMeasureId: product.baseUnitOfMeasureId,
      hasVariants: product.hasVariants,
      isStockTracked: product.isStockTracked,
      reorderLevel: product.reorderLevel,
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
