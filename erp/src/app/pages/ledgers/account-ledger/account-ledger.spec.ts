import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AccountLedgerPage } from './account-ledger';

describe('AccountLedgerPage', () => {
  let component: AccountLedgerPage;
  let fixture: ComponentFixture<AccountLedgerPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountLedgerPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(AccountLedgerPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
