import { check } from './../../../models/check.model';
import { ReportingStudioService } from './../../../reporting-studio.service';
import { AskdialogService } from './../askdialog.service';
import { CheckBoxComponent } from '@taskbuilder/core';

import { Component, OnInit, Input, DoCheck } from '@angular/core';

@Component({
  selector: 'rs-ask-check',
  templateUrl: './ask-check.component.html',
  styleUrls: ['./ask-check.component.scss']
})
export class AskCheckComponent extends CheckBoxComponent implements OnInit, DoCheck {

  @Input() check: check;
  constructor(private rsService: ReportingStudioService, private adService: AskdialogService) {
    super();
  }

  private oldValue: boolean;

  ngOnInit() {

    this.check.value = (this.check.value === 'True');
    this.oldValue = this.check.value;
  }



  ngDoCheck() {
    if (this.oldValue != this.check.value) {
      this.oldValue = this.check.value;
      if (this.check.runatserver) {
        /*let obj = {
          id: this.check.id,
          value: this.check.value.toString()
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
}
