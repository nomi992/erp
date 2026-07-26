import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { TenancyService } from '../tenancy/tenancy.service';
import { LoginRequest, LoginResponse } from './auth.models';

const STORAGE_KEY = 'erp.auth';

interface StoredAuth {
  token: string;
  username: string;
  role: string;
  expiresAtUtc: string;
  rights: string[];
}

function readStoredAuth(): StoredAuth | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    const parsed = JSON.parse(raw) as StoredAuth;
    return { ...parsed, rights: parsed.rights ?? [] };
  } catch {
    return null;
  }
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly tenancyService = inject(TenancyService);

  private readonly auth = signal<StoredAuth | null>(readStoredAuth());

  readonly token = computed(() => this.auth()?.token ?? null);
  readonly username = computed(() => this.auth()?.username ?? null);
  readonly role = computed(() => this.auth()?.role ?? null);
  readonly rights = computed(() => this.auth()?.rights ?? []);
  readonly isAuthenticated = computed(() => this.auth() !== null);

  hasRight(code: string): boolean {
    return this.rights().includes(code);
  }

  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http
      .post<ApiResponse<LoginResponse>>(`${environment.apiUrl}/api/auth/login`, request)
      .pipe(
        tap((response) => {
          if (response.data) {
            this.setAuth(response.data);
          }
        }),
      );
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.auth.set(null);
    this.tenancyService.clear();
    this.router.navigateByUrl('/login');
  }

  private setAuth(data: LoginResponse): void {
    const stored: StoredAuth = {
      token: data.token,
      username: data.username,
      role: data.role,
      expiresAtUtc: data.expiresAtUtc,
      rights: data.rights,
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
    this.auth.set(stored);
    this.tenancyService.setFromLogin(data);
  }
}
