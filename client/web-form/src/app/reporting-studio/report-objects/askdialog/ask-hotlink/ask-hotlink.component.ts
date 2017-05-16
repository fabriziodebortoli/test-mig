import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import { ReportingStudioService } from './../../../reporting-studio.service';
import { hotlink, CommandType } from './../../../reporting-studio.model';
import { Component, Input, DoCheck, KeyValueDiffers, Output, EventEmitter, ViewChild } from '@angular/core';

@Component({
  selector: 'rs-ask-hotlink',
  templateUrl: './ask-hotlink.component.html',
  styleUrls: ['./ask-hotlink.component.scss']
})
export class AskHotlinkComponent implements DoCheck {

  @ViewChild(AutoCompleteComponent)
  private acComp: AutoCompleteComponent;

  @Input() hotlink: hotlink;
  value: string = '';
  differ: any;
 



  constructor(private rsService: ReportingStudioService, private differs: KeyValueDiffers) {

    this.differ = differs.find({}).create(null);
  }

  ngDoCheck() {
    let changes = this.differ.diff(this.hotlink);
    if (changes && this.hotlink.values.length !== 0) {
      this.acComp.toggle(true);
    }
  }

  OnOpen(event: any) {
    if (this.hotlink.values.length === 0) {
      event.preventDefault();
    }
  }

  onButtonClick() {
    let msg = {
      ns: this.hotlink.ns,
      filter: this.value,
      name: this.hotlink.name
    };

    let message = {
      commandType: CommandType.HOTLINK,
      message: JSON.stringify(msg),
      page: this.hotlink.id
    };

    this.rsService.doSend(JSON.stringify(message));
  }

}
