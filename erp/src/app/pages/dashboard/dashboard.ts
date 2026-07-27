import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { CardModule } from 'primeng/card';
import { ChartModule } from 'primeng/chart';
import { NotificationService } from '../../core/notifications/notification.service';
import { ReportService } from '../../core/reports/report.service';
import { BalanceSheet, ProfitLoss } from '../../core/reports/report.models';
import { StockLedgerService } from '../../core/stock-ledger/stock-ledger.service';
import { PartnerAging, StockOnHand } from '../../core/stock-ledger/stock-ledger.models';
import { InvoiceService } from '../../core/invoices/invoice.service';
import { ThemeService } from '../../core/theme/theme.service';

interface Kpi {
  label: string;
  value: number;
  icon: string;
  severity: 'success' | 'danger' | 'info' | 'warn';
}

function toIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function sumAging(rows: PartnerAging[]): { current: number; days1To30: number; days31To60: number; days61To90: number; daysOver90: number } {
  return rows.reduce(
    (acc, row) => ({
      current: acc.current + row.current,
      days1To30: acc.days1To30 + row.days1To30,
      days31To60: acc.days31To60 + row.days31To60,
      days61To90: acc.days61To90 + row.days61To90,
      daysOver90: acc.daysOver90 + row.daysOver90,
    }),
    { current: 0, days1To30: 0, days31To60: 0, days61To90: 0, daysOver90: 0 },
  );
}

function cssVar(name: string): string {
  return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
}

@Component({
  selector: 'app-dashboard',
  imports: [DecimalPipe, CardModule, ChartModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private readonly reportService = inject(ReportService);
  private readonly stockLedgerService = inject(StockLedgerService);
  private readonly invoiceService = inject(InvoiceService);
  private readonly notificationService = inject(NotificationService);
  private readonly themeService = inject(ThemeService);

  readonly loading = signal(false);

  private readonly profitLoss = signal<ProfitLoss | null>(null);
  private readonly balanceSheet = signal<BalanceSheet | null>(null);
  private readonly arAging = signal<PartnerAging[]>([]);
  private readonly apAging = signal<PartnerAging[]>([]);
  private readonly stockOnHand = signal<StockOnHand[]>([]);
  private readonly pendingApprovals = signal(0);

  readonly lowStockItems = computed(() =>
    this.stockOnHand()
      .filter((item) => item.isLowStock)
      .sort((a, b) => a.quantityOnHand - b.quantityOnHand)
      .slice(0, 5),
  );

  readonly kpis = computed<Kpi[]>(() => {
    const pl = this.profitLoss();
    const ar = sumAging(this.arAging());
    const ap = sumAging(this.apAging());
    const totalAr = ar.current + ar.days1To30 + ar.days31To60 + ar.days61To90 + ar.daysOver90;
    const totalAp = ap.current + ap.days1To30 + ap.days31To60 + ap.days61To90 + ap.daysOver90;
    const stockValue = this.stockOnHand().reduce((sum, item) => sum + item.stockValue, 0);
    const netProfit = pl?.netProfit ?? 0;

    return [
      { label: 'Net Profit (MTD)', value: netProfit, icon: 'pi pi-chart-line', severity: netProfit >= 0 ? 'success' : 'danger' },
      { label: 'Receivables', value: totalAr, icon: 'pi pi-arrow-down-left', severity: 'info' },
      { label: 'Payables', value: totalAp, icon: 'pi pi-arrow-up-right', severity: 'warn' },
      { label: 'Stock Value', value: stockValue, icon: 'pi pi-box', severity: 'info' },
      { label: 'Low Stock Items', value: this.lowStockItems().length, icon: 'pi pi-exclamation-triangle', severity: 'danger' },
      { label: 'Pending Approvals', value: this.pendingApprovals(), icon: 'pi pi-clock', severity: 'warn' },
    ];
  });

  readonly incomeExpenseChart = computed(() => {
    this.themeService.currentMode();
    const pl = this.profitLoss();
    const textColor = cssVar('--p-text-color');
    const borderColor = cssVar('--p-content-border-color');

    return {
      data: {
        labels: ['Income', 'Expenses'],
        datasets: [
          {
            data: [pl?.totalIncome ?? 0, pl?.totalExpenses ?? 0],
            backgroundColor: [cssVar('--p-green-500'), cssVar('--p-red-500')],
            borderRadius: 4,
            maxBarThickness: 64,
          },
        ],
      },
      options: {
        plugins: { legend: { display: false } },
        scales: {
          x: { ticks: { color: textColor }, grid: { display: false } },
          y: { ticks: { color: textColor }, grid: { color: borderColor }, beginAtZero: true },
        },
      },
    };
  });

  readonly balanceSheetChart = computed(() => {
    this.themeService.currentMode();
    const bs = this.balanceSheet();
    const textColor = cssVar('--p-text-color');

    return {
      data: {
        labels: ['Assets', 'Liabilities', 'Equity'],
        datasets: [
          {
            data: [bs?.totalAssets ?? 0, bs?.totalLiabilities ?? 0, bs?.totalEquity ?? 0],
            backgroundColor: [cssVar('--p-blue-500'), cssVar('--p-orange-500'), cssVar('--p-purple-500')],
          },
        ],
      },
      options: {
        plugins: { legend: { position: 'bottom' as const, labels: { color: textColor } } },
      },
    };
  });

  readonly agingChart = computed(() => {
    this.themeService.currentMode();
    const ar = sumAging(this.arAging());
    const ap = sumAging(this.apAging());
    const textColor = cssVar('--p-text-color');
    const borderColor = cssVar('--p-content-border-color');

    return {
      data: {
        labels: ['Current', '1-30 Days', '31-60 Days', '61-90 Days', '90+ Days'],
        datasets: [
          {
            label: 'Receivables',
            data: [ar.current, ar.days1To30, ar.days31To60, ar.days61To90, ar.daysOver90],
            backgroundColor: cssVar('--p-blue-500'),
            borderRadius: 4,
          },
          {
            label: 'Payables',
            data: [ap.current, ap.days1To30, ap.days31To60, ap.days61To90, ap.daysOver90],
            backgroundColor: cssVar('--p-orange-500'),
            borderRadius: 4,
          },
        ],
      },
      options: {
        plugins: { legend: { position: 'bottom' as const, labels: { color: textColor } } },
        scales: {
          x: { ticks: { color: textColor }, grid: { display: false } },
          y: { ticks: { color: textColor }, grid: { color: borderColor }, beginAtZero: true },
        },
      },
    };
  });

  readonly topDebtorsChart = computed(() => this.buildTopPartnersChart(this.arAging(), '--p-blue-500'));

  readonly topCreditorsChart = computed(() => this.buildTopPartnersChart(this.apAging(), '--p-orange-500'));

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    const today = new Date();
    const from = toIsoDate(new Date(today.getFullYear(), today.getMonth(), 1));
    const to = toIsoDate(today);

    this.loading.set(true);
    forkJoin({
      profitLoss: this.reportService.getProfitLoss(from, to),
      balanceSheet: this.reportService.getBalanceSheet(to),
      arAging: this.stockLedgerService.getAccountsReceivableAging(),
      apAging: this.stockLedgerService.getAccountsPayableAging(),
      stockOnHand: this.stockLedgerService.getOnHand(),
      pendingApprovals: this.invoiceService.getAll({ status: 'PendingApproval', pageSize: 1 }),
    }).subscribe({
      next: (result) => {
        this.loading.set(false);
        this.profitLoss.set(result.profitLoss.data ?? null);
        this.balanceSheet.set(result.balanceSheet.data ?? null);
        this.arAging.set(result.arAging.data ?? []);
        this.apAging.set(result.apAging.data ?? []);
        this.stockOnHand.set(result.stockOnHand.data ?? []);
        this.pendingApprovals.set(result.pendingApprovals.data?.totalCount ?? 0);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notificationService.error(this.extractErrorMessage(error, 'Unable to load dashboard data.'));
      },
    });
  }

  private buildTopPartnersChart(rows: PartnerAging[], colorVar: string) {
    this.themeService.currentMode();
    const textColor = cssVar('--p-text-color');
    const borderColor = cssVar('--p-content-border-color');
    const top = [...rows]
      .filter((row) => row.total > 0)
      .sort((a, b) => b.total - a.total)
      .slice(0, 5);

    return {
      data: {
        labels: top.map((row) => row.partnerName),
        datasets: [
          {
            data: top.map((row) => row.total),
            backgroundColor: cssVar(colorVar),
            borderRadius: 4,
          },
        ],
      },
      options: {
        indexAxis: 'y' as const,
        plugins: { legend: { display: false } },
        scales: {
          x: { ticks: { color: textColor }, grid: { color: borderColor }, beginAtZero: true },
          y: { ticks: { color: textColor }, grid: { display: false } },
        },
      },
    };
  }

  private extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
    const apiError = error.error as { message?: string } | null;
    return apiError?.message ?? fallback;
  }
}
