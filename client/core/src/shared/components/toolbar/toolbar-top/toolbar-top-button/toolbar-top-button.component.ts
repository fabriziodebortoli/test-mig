import { Component, Input, ChangeDetectorRef } from '@angular/core';

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

  /**
   * Optional command called on button click.
   * @returns {boolean} return true to call default command as well.
   */
  @Input() command: () => boolean = () => true;

  imgUrl: string;

  constructor(
    public eventData: EventDataService,
    public infoService: InfoService,
    public ciService: ComponentInfoService,
    public tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
    //this.imgUrl = this.infoService.getDocumentBaseUrl() + 'getImage/?src=';
  }
  onCommand() {
    if (!this.command())
      return;
    this.eventData.raiseCommand(this.ciService.getComponentId(), this.cmpId);
  }
}