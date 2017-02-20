import { Component, OnInit, Input } from '@angular/core';
import { TbComponent } from './../../..';
import { HttpService, EventDataService } from '../../../../core';

enum IconType { MD, TB, IMG };

@Component({
  selector: 'tb-toolbar-top-button',
  templateUrl: './toolbar-top-button.component.html',
  styleUrls: ['./toolbar-top-button.component.scss']
})
export class ToolbarTopButtonComponent extends TbComponent implements OnInit {

  @Input() caption: string = '';

  @Input() icon: string = '';
  iconType: IconType;
  iconTypes = IconType;
  iconTxt: string;

  constructor(
    private eventData: EventDataService,
    private httpService: HttpService
  ) {
    super();
  }

  ngOnInit() {
    this.checkIcon();
  }

  checkIcon() {
    if (this.icon.startsWith("md-")) {
      this.iconType = IconType.MD;
      this.iconTxt = this.icon.slice(3);
    } else if (this.icon.startsWith("tb-")) {
      this.iconType = IconType.TB;
      this.iconTxt = this.icon.slice(3);
    } else {
      this.iconType = IconType.IMG;
      this.iconTxt = this.httpService.getDocumentBaseUrl() + 'getImage/?src=' + this.icon;
    }
  }

  onCommand() {
    this.eventData.command.emit(this.cmpId);
  }
}