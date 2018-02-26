import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, Input, ChangeDetectorRef } from '@angular/core';
import { ViewModeType } from './../../../shared/models/view-mode-type.model';
import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-body-edit-toolbar',
  templateUrl: './body-edit-toolbar.component.html',
  styleUrls: ['./body-edit-toolbar.component.scss']
})

  export class BodyEditToolbarComponent {
    constructor(public eventData: EventDataService) { }
    
    @Input() caption: string = '...';
    @Input() history: boolean = false;
  
    public viewModeTypeModel = ViewModeType;
  }
  