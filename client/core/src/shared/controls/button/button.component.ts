import { Component, Input, ChangeDetectorRef } from '@angular/core';

import { ComponentInfoService } from './../../../core/services/component-info.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-button',
    templateUrl: './button.component.html',
    styleUrls: ['./button.component.scss']
})
export class ButtonComponent extends ControlComponent {

    @Input() icon: string = 'tb-execute';

    constructor(
        public eventData: EventDataService,
        public ciService: ComponentInfoService,
        public layoutService: LayoutService,
        public tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef
    ) {
        super(layoutService, tbComponentService, changeDetectorRef)
    }

    onCommand() {
        this.eventData.raiseCommand(this.ciService.getComponentId(), this.cmpId);
    }
}
