import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FiscalPeriods } from './fiscal-periods';

describe('FiscalPeriods', () => {
  let component: FiscalPeriods;
  let fixture: ComponentFixture<FiscalPeriods>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FiscalPeriods],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(FiscalPeriods);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
