import { AskdialogComponent } from './../askdialog.component';
import { ReportingStudioService } from './../../../reporting-studio.service';
import { text, CommandType } from '@taskbuilder/reporting-studio';
import { Component, OnInit, Input, Type, EventEmitter, Output } from '@angular/core';
import * as moment from 'moment';
import { AskdialogService } from "app/reporting-studio/report-objects/askdialog/askdialog.service";


@Component({
  selector: 'rs-ask-text',
  templateUrl: './ask-text.component.html',
  styleUrls: ['./ask-text.component.scss'],

})


export class AskTextComponent implements OnInit {

  @Input() text: text;

  constructor(private rsService: ReportingStudioService, private adService: AskdialogService) { }


  onBlur(value) {
    if (this.text.runatserver) {
      /*let obj = {
        id: this.text.id,
        value: this.text.value.toString()
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

  ngOnInit() {

    if (this.text.type === 'DateTime') {
      const t2 = moment.parseZone(this.text.value, 'DD/MM/YYYY HH:mm:ss').format('YYYY-MM-DDTHH:mm:ss');
      this.text.value = new Date(t2);
    }
  }

}
