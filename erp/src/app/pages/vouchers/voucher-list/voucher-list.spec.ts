import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { VoucherList } from './voucher-list';

describe('VoucherList', () => {
  let component: VoucherList;
  let fixture: ComponentFixture<VoucherList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VoucherList],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(VoucherList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
