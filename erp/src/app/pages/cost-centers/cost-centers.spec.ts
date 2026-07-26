import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CostCenters } from './cost-centers';

describe('CostCenters', () => {
  let component: CostCenters;
  let fixture: ComponentFixture<CostCenters>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CostCenters],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(CostCenters);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
