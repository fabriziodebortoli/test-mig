import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StrBinEditComponent } from './str-bin-edit.component';

describe('StrBinEditComponent', () => {
    let component: StrBinEditComponent;
    let fixture: ComponentFixture<StrBinEditComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [StrBinEditComponent]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(StrBinEditComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});