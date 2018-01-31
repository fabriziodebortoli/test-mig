import { Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef, OnChanges, AfterContentInit, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from '../control.component';

@Component({
    selector: 'tb-control-container',
    templateUrl: './control-container.component.html',
    styleUrls: ['./control-container.component.scss']
})

export class ControlContainerComponent extends ControlComponent {
    @Input('type') type: string = '';
    @Input('errorMessage') errorMessage: string = '';
}
