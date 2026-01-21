import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AccountLineFormComponent } from './account-line-form.component';

describe('AccountLineFormComponent', () => {
  let component: AccountLineFormComponent;
  let fixture: ComponentFixture<AccountLineFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountLineFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AccountLineFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
