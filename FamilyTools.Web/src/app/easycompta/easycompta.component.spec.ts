import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { EasycomptaComponent } from './easycompta.component';

describe('EasycomptaComponent', () => {
  let component: EasycomptaComponent;
  let fixture: ComponentFixture<EasycomptaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EasycomptaComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(EasycomptaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
