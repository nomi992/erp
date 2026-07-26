import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BankReconciliation } from './bank-reconciliation';

describe('BankReconciliation', () => {
  let component: BankReconciliation;
  let fixture: ComponentFixture<BankReconciliation>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BankReconciliation],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(BankReconciliation);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
