import { Component, Input, ChangeDetectorRef, SimpleChanges } from '@angular/core';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-checkbox',
    templateUrl: 'checkbox.component.html',
    styleUrls: ['./checkbox.component.scss']
})

export class CheckBoxComponent extends ControlComponent {
    constructor(
        public eventData: EventDataService,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);
    }

    modelChanged(event) {
        this.model.value = event;
        this.eventData.change.emit(this.cmpId);
    }
}
