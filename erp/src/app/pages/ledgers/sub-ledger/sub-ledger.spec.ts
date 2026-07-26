import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SubLedger } from './sub-ledger';

describe('SubLedger', () => {
  let component: SubLedger;
  let fixture: ComponentFixture<SubLedger>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SubLedger],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(SubLedger);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
