import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { NoSpacesEditComponent } from './no-spaces.component';

describe('NoSpacesComponent', () => {
  let component: NoSpacesEditComponent;
  let fixture: ComponentFixture<NoSpacesEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [NoSpacesEditComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(NoSpacesEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
