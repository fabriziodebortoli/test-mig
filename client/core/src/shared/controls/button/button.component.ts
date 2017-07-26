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
    constructor(private eventData: EventDataService, layoutService: LayoutService) {
        super(layoutService);
    }
    onCommand() {
        this.eventData.command.emit(this.cmpId);
    }
}
