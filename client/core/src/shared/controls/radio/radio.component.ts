import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { ControlComponent } from '../control.component';
import { Component, Input,ChangeDetectorRef } from '@angular/core';


@Component({
    selector: 'tb-radio',
    templateUrl: 'radio.component.html',
    styleUrls: ['./radio.component.scss']
})

export class RadioComponent extends ControlComponent {
    @Input() name: string;

    constructor(
        layoutService:LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef:ChangeDetectorRef
    ) {
        super(layoutService, tbComponentService,changeDetectorRef)
    }
}
