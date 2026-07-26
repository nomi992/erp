import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { GeneralLedger } from './general-ledger';

describe('GeneralLedger', () => {
  let component: GeneralLedger;
  let fixture: ComponentFixture<GeneralLedger>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GeneralLedger],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(GeneralLedger);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
