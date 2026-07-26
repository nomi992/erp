import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaxRates } from './tax-rates';

describe('TaxRates', () => {
  let component: TaxRates;
  let fixture: ComponentFixture<TaxRates>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaxRates],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(TaxRates);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
