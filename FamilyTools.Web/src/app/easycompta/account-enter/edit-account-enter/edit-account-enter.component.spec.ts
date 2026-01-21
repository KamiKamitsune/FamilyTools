import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditAccountEnterComponent } from './edit-account-enter.component';

describe('EditAccountEnterComponent', () => {
  let component: EditAccountEnterComponent;
  let fixture: ComponentFixture<EditAccountEnterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditAccountEnterComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditAccountEnterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
