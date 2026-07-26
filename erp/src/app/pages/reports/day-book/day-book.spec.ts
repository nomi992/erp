import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DayBookReport } from './day-book';

describe('DayBookReport', () => {
  let component: DayBookReport;
  let fixture: ComponentFixture<DayBookReport>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DayBookReport],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(DayBookReport);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
