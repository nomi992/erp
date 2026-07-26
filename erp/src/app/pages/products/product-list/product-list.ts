import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PrimeTemplate } from 'primeng/api';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ProductService } from '../../../core/products/product.service';
import { Product } from '../../../core/products/product.models';
import { HasRightDirective } from '../../../core/auth/has-right.directive';
import { RightCode } from '../../../core/auth/right-code';
import { ApiResponse } from '../../../core/models/api-response.model';
import { NotificationService } from '../../../core/notifications/notification.service';

@Component({
  selector: 'app-product-list',
  imports: [ButtonModule, CardModule, HasRightDirective, InputTextModule, PrimeTemplate, TableModule, TagModule, TooltipModule],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss',
})
export class ProductList implements OnInit {
  protected readonly RightCode = RightCode;

  private readonly productService = inject(ProductService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly products = signal<Product[]>([]);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.productService.getAll().subscribe({
      next: (response) => {
        this.loading.set(false);
        this.products.set(response.data ?? []);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load products.'));
      },
    });
  }

  createProduct(): void {
    this.router.navigate(['/products/new']);
  }

  editProduct(product: Product): void {
    this.router.navigate(['/products', product.id, 'edit']);
  }

  toggleActive(product: Product): void {
    const call = product.isActive ? this.productService.deactivate(product.id) : this.productService.activate(product.id);

    call.subscribe({
      next: () => {
        this.notificationService.success(product.isActive ? 'Product deactivated.' : 'Product activated.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to update product status.'));
      },
    });
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as ApiResponse<unknown> | null;
    return apiError?.message ?? fallback;
  }
}
