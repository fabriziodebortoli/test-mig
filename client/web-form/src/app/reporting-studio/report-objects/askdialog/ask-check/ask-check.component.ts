import { ReportingStudioService } from './../../../reporting-studio.service';
import { CheckBoxComponent } from './../../../../shared/controls/checkbox/checkbox.component';
import { check, CommandType } from './../../../reporting-studio.model';
import { Component, OnInit, Input, DoCheck } from '@angular/core';

@Component({
  selector: 'rs-ask-check',
  templateUrl: './ask-check.component.html',
  styleUrls: ['./ask-check.component.scss']
})
export class AskCheckComponent extends CheckBoxComponent implements OnInit, DoCheck {

  @Input() check: check;
  constructor(private rService: ReportingStudioService) {
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
        let obj = {
          id: this.check.id,
          value: this.check.value.toString()
        };
        let message = {
          commandType: CommandType.UPDATEASK,
          message: JSON.stringify(obj),
          page: 0
        };
        this.rService.doSend(JSON.stringify(message));
      }
    }
  }
}
