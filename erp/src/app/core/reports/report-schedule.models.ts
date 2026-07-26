import { RecurringFrequency } from '../vouchers/voucher.models';

export interface ReportSchedule {
  id: number;
  reportType: string;
  recipients: string;
  frequency: RecurringFrequency;
  isActive: boolean;
  lastRunAtUtc: string | null;
}

export interface ReportScheduleRequest {
  reportType: string;
  recipients: string;
  frequency: RecurringFrequency;
}
