import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChartOfAccounts } from './chart-of-accounts';

describe('ChartOfAccounts', () => {
  let component: ChartOfAccounts;
  let fixture: ComponentFixture<ChartOfAccounts>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChartOfAccounts],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(ChartOfAccounts);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
