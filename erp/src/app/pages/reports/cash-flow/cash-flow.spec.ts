import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CashFlowReport } from './cash-flow';

describe('CashFlowReport', () => {
  let component: CashFlowReport;
  let fixture: ComponentFixture<CashFlowReport>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CashFlowReport],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(CashFlowReport);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
