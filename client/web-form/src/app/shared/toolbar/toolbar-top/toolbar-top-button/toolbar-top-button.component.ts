import { HttpService } from '@taskbuilder/core';
import { EventDataService } from './../../../../core/eventdata.service';
import { Component, Input } from '@angular/core';
import { TbComponent } from './../../..';

@Component({
  selector: 'tb-toolbar-top-button',
  templateUrl: './toolbar-top-button.component.html',
  styleUrls: ['./toolbar-top-button.component.scss']
})
export class ToolbarTopButtonComponent extends TbComponent {

  @Input() caption: string = '';
  @Input() disabled: boolean = false;

  @Input() iconType: string = 'IMG'; // MD, TB, CLASS, IMG  
  @Input() icon: string = '';

  imgUrl: string;

  constructor(
    private eventData: EventDataService,
    private httpService: HttpService
  ) {
    super();

    this.imgUrl = this.httpService.getDocumentBaseUrl() + 'getImage/?src=';
  }

  onCommand() {
    this.eventData.command.emit(this.cmpId);
  }
}