import { ComponentInfoService } from './../../../core/services/component-info.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { CommandEventArgs } from './../../models/eventargs.model';
import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-button',
    templateUrl: 'button.component.html',
    styleUrls: ['./button.component.scss']
})

export class ButtonComponent extends ControlComponent {
    constructor(
        public eventData: EventDataService,
        public ciService: ComponentInfoService,
        public layoutService: LayoutService,
        public tbComponentService: TbComponentService) {
        super(layoutService, tbComponentService)
    }
    onCommand() {
        this.eventData.raiseCommand(this.ciService.getComponentId(), this.cmpId);
    }
}
