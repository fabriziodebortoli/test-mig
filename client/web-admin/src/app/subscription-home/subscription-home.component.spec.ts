import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SubscriptionHomeComponent } from './subscription-home.component';

describe('SubscriptionHomeComponent', () => {
  let component: SubscriptionHomeComponent;
  let fixture: ComponentFixture<SubscriptionHomeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SubscriptionHomeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SubscriptionHomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
