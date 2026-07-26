import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BalanceSheetReport } from './balance-sheet';

describe('BalanceSheetReport', () => {
  let component: BalanceSheetReport;
  let fixture: ComponentFixture<BalanceSheetReport>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BalanceSheetReport],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(BalanceSheetReport);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
