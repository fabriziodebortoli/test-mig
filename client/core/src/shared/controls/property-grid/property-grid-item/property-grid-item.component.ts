import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { ControlComponent } from './../../control.component';
import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../../core/services/layout.service';


@Component({
    selector: 'tb-property-grid-item',
    templateUrl: './property-grid-item.component.html',
    styleUrls: ['./property-grid-item.component.scss']
})
export class PropertyGridItemComponent extends ControlComponent {
    @Input() text: string;
    @Input() hint: string;
    @Input() itemSource: any;
    @Input() hotLink: { namespace: string, name: string };

    constructor(
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef
    ) {
        super(layoutService, tbComponentService, changeDetectorRef)
    }
}

