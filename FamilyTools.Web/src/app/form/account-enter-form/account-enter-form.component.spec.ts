import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AccountEnterFormComponent } from './account-enter-form.component';

describe('AccountEnterFormComponent', () => {
  let component: AccountEnterFormComponent;
  let fixture: ComponentFixture<AccountEnterFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountEnterFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AccountEnterFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
