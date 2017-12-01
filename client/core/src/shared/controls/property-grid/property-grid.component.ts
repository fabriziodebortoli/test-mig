import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { ControlComponent } from './../control.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';

@Component({
    selector: 'tb-property-grid',
    templateUrl: './property-grid.component.html',
    styleUrls: ['./property-grid.component.scss']
})
export class PropertyGridComponent extends ControlComponent {
    @Input() itemSource: any;

    constructor(
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef
    ) {
        super(layoutService, tbComponentService, changeDetectorRef)
    }
}
