import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RecurringVouchers } from './recurring-vouchers';

describe('RecurringVouchers', () => {
  let component: RecurringVouchers;
  let fixture: ComponentFixture<RecurringVouchers>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RecurringVouchers],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(RecurringVouchers);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
