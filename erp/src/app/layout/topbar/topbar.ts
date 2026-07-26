import { Component, inject, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { AvatarModule } from 'primeng/avatar';
import { SelectModule } from 'primeng/select';
import { AuthService } from '../../core/auth/auth.service';
import { TenancyService } from '../../core/tenancy/tenancy.service';

@Component({
  selector: 'app-topbar',
  imports: [FormsModule, ToolbarModule, ButtonModule, AvatarModule, SelectModule],
  templateUrl: './topbar.html',
  styleUrl: './topbar.scss',
})
export class Topbar {
  private readonly authService = inject(AuthService);
  readonly tenancyService = inject(TenancyService);

  readonly toggleSidebar = output<void>();

  onBranchChange(branchId: number): void {
    this.tenancyService.selectBranch(branchId);
  }

  logout(): void {
    this.authService.logout();
  }
}
