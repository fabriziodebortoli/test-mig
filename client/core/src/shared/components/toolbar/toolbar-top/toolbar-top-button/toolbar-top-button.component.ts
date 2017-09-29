import { Component, Input } from '@angular/core';

import { ComponentInfoService } from './../../../../../core/services/component-info.service';
import { TbComponentService } from './../../../../../core/services/tbcomponent.service';
import { InfoService } from './../../../../../core/services/info.service';
import { EventDataService } from './../../../../../core/services/eventdata.service';

import { TbComponent } from '../../../tb.component';

@Component({
  selector: 'tb-toolbar-top-button',
  templateUrl: './toolbar-top-button.component.html',
  styleUrls: ['./toolbar-top-button.component.scss']
})
export class ToolbarTopButtonComponent extends TbComponent {

  @Input() caption: string = '';
  @Input() disabled: boolean = false;

  @Input() iconType: string = 'M4'; // MD, TB, CLASS, IMG  
  @Input() icon: string = '';

  imgUrl: string;

  constructor(
    private eventData: EventDataService,
    private infoService: InfoService,
    private ciService: ComponentInfoService,
    tbComponentService: TbComponentService
  ) {
    super(tbComponentService);
    //this.imgUrl = this.infoService.getDocumentBaseUrl() + 'getImage/?src=';
  }
  onCommand() {
    this.eventData.raiseCommand(this.ciService.getComponentId(), this.cmpId);
  }
}