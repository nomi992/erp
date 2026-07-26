import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TrialBalanceReport } from './trial-balance';

describe('TrialBalanceReport', () => {
  let component: TrialBalanceReport;
  let fixture: ComponentFixture<TrialBalanceReport>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TrialBalanceReport],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(TrialBalanceReport);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
