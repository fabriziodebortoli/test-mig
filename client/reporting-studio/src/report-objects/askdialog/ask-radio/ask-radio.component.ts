import { AskdialogService } from './../askdialog.service';
import { ReportingStudioService } from './../../../reporting-studio.service';

import { RadioComponent } from '@taskbuilder/core';
import { radio, CommandType } from './../../../reporting-studio.model';
import { Component, OnInit, Input, ViewEncapsulation } from '@angular/core';


@Component({
  selector: 'rs-ask-radio',
  templateUrl: './ask-radio.component.html',
  encapsulation: ViewEncapsulation.None,
  styleUrls: ['./ask-radio.component.scss']
})
export class AskRadioComponent extends RadioComponent implements OnInit {

  @Input() radio: radio;
  @Input() otherRadios: radio[];
  constructor(private rsService: ReportingStudioService, private adService: AskdialogService) {
    super()
  }

  ngOnInit() {
    this.radio.value = (this.radio.value === 'True');
  }

  public checkedRadio() {
    for (let i = 0; i < this.otherRadios.length; i++) {
      let elem: radio = this.otherRadios[i];
      if (this.radio.id !== elem.id) {
        elem.value = false;
      }
      else elem.value = true;
    }
    if (this.radio.runatserver) {
      /*let obj = {
        id: this.radio.id,
        value: this.radio.value.toString()
      };
      let message = {
        commandType: CommandType.UPDATEASK,
        message: JSON.stringify(obj),
        page: this.rsService.askPage
      };*/
      this.adService.askChanged.emit();
      //this.rsService.doSend(JSON.stringify(message));
    }
  }
}
