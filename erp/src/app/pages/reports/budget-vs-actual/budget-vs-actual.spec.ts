import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BudgetVsActualReport } from './budget-vs-actual';

describe('BudgetVsActualReport', () => {
  let component: BudgetVsActualReport;
  let fixture: ComponentFixture<BudgetVsActualReport>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BudgetVsActualReport],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(BudgetVsActualReport);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
