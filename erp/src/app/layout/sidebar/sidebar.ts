import { Component, inject, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  roles?: string[];
}

interface NavGroup {
  label: string;
  items: NavItem[];
}

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class Sidebar {
  private readonly authService = inject(AuthService);

  readonly open = input(true);

  readonly navGroups: NavGroup[] = [
    {
      label: '',
      items: [{ label: 'Dashboard', icon: 'pi pi-home', route: '/dashboard' }],
    },
    {
      label: 'Setup',
      items: [
        { label: 'Chart of Accounts', icon: 'pi pi-sitemap', route: '/chart-of-accounts' },
        { label: 'Cost Centers', icon: 'pi pi-building', route: '/cost-centers' },
        { label: 'Fiscal Periods', icon: 'pi pi-calendar', route: '/fiscal-periods' },
        { label: 'Tax Rates', icon: 'pi pi-percentage', route: '/tax-rates' },
      ],
    },
    {
      label: 'Vouchers',
      items: [
        { label: 'All Vouchers', icon: 'pi pi-file-edit', route: '/vouchers' },
        { label: 'Recurring Templates', icon: 'pi pi-history', route: '/recurring-vouchers' },
      ],
    },
    {
      label: 'Ledgers',
      items: [
        { label: 'General Ledger', icon: 'pi pi-book', route: '/ledgers/general' },
        { label: 'Account Ledger', icon: 'pi pi-wallet', route: '/ledgers/account' },
        { label: 'Sub-Ledger Aging', icon: 'pi pi-clock', route: '/ledgers/sub-ledger' },
        { label: 'Bank Reconciliation', icon: 'pi pi-credit-card', route: '/bank-reconciliation' },
      ],
    },
    {
      label: 'Reports',
      items: [
        { label: 'Trial Balance', icon: 'pi pi-list', route: '/reports/trial-balance' },
        { label: 'Profit & Loss', icon: 'pi pi-chart-line', route: '/reports/profit-loss' },
        { label: 'Balance Sheet', icon: 'pi pi-file', route: '/reports/balance-sheet' },
        { label: 'Cash Flow', icon: 'pi pi-money-bill', route: '/reports/cash-flow' },
        { label: 'AR/AP Aging', icon: 'pi pi-hourglass', route: '/reports/aging' },
        { label: 'Day Book', icon: 'pi pi-calendar-times', route: '/reports/day-book' },
        { label: 'Budget vs Actual', icon: 'pi pi-chart-bar', route: '/reports/budget-vs-actual' },
        { label: 'Budgets', icon: 'pi pi-wallet', route: '/budgets' },
        { label: 'Report Schedules', icon: 'pi pi-send', route: '/report-schedules' },
      ],
    },
    {
      label: 'Administration',
      items: [
        { label: 'Tenants', icon: 'pi pi-globe', route: '/admin/tenants', roles: ['SystemAdmin'] },
        { label: 'Branches', icon: 'pi pi-map-marker', route: '/admin/branches', roles: ['Admin', 'SystemAdmin'] },
        { label: 'Users', icon: 'pi pi-users', route: '/admin/users', roles: ['Admin', 'SystemAdmin'] },
        { label: 'Roles', icon: 'pi pi-shield', route: '/admin/roles', roles: ['Admin', 'SystemAdmin'] },
      ],
    },
  ];

  isVisible(item: NavItem): boolean {
    return !item.roles || item.roles.includes(this.authService.role() ?? '');
  }

  isGroupVisible(group: NavGroup): boolean {
    return group.items.some((item) => this.isVisible(item));
  }
}
