import { ReportingStudioService } from './../../../reporting-studio.service';
import { hotlink, CommandType } from './../../../reporting-studio.model';
import { Component, Input, OnChanges } from '@angular/core';

@Component({
  selector: 'rs-ask-hotlink',
  templateUrl: './ask-hotlink.component.html',
  styleUrls: ['./ask-hotlink.component.scss']
})
export class AskHotlinkComponent implements OnChanges{

  @Input() hotlink: hotlink;
  value: string = '';

  constructor(private rsService: ReportingStudioService) { }

  ngOnChanges() {
    let k=0;
  }

  OnOpen(event: any) {
    if (this.hotlink.selectionList.length === 0) {
      event.preventDefault();
    }
  }

  onButtonClick() {
    let msg = {
      ns: this.hotlink.ns,
      filter: this.value,
      id: this.hotlink.id
    };

    let message = {
      commandType: CommandType.HOTLINK,
      message: JSON.stringify(msg),
      page: 0
    };

    this.rsService.doSend(JSON.stringify(message));
  }

}
