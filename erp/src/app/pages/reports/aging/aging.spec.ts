import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AgingReport } from './aging';

describe('AgingReport', () => {
  let component: AgingReport;
  let fixture: ComponentFixture<AgingReport>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AgingReport],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(AgingReport);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
