import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfitLossReport } from './profit-loss';

describe('ProfitLossReport', () => {
  let component: ProfitLossReport;
  let fixture: ComponentFixture<ProfitLossReport>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProfitLossReport],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(ProfitLossReport);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
