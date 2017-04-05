import { MenuItem } from './../context-menu/menu-item.model';
import { StateButton } from './../state-button/state-button.model';
import { EventDataService } from './../../../core/eventdata.service';
import { ControlComponent } from './../control.component';
import { Component, Input } from '@angular/core';
import { ControlTypes } from '../control-types.enum';


@Component({
    selector: 'tb-edit',
    templateUrl: 'edit.component.html',
    styleUrls: ['./edit.component.scss']
})

export class EditComponent extends ControlComponent {
    @Input() buttons: StateButton[] = [];
    @Input('controlType') controlType: ControlTypes;
    @Input() contextMenu: MenuItem[] = [{text: 'Menu1', id:'idd_menu1'}];

    controlTypeModel = ControlTypes;
    constructor(
        private eventData: EventDataService
    ) {
        super();
    }

    onBlur() {
        this.eventData.change.emit(this.cmpId);
    }
}
