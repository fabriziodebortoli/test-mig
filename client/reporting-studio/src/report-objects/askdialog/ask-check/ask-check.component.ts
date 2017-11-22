import { check } from './../../../models/check.model';
import { ReportingStudioService } from './../../../reporting-studio.service';
import { AskdialogService } from './../askdialog.service';
import { LayoutService, TbComponentService, CheckBoxComponent } from '@taskbuilder/core';
import { Component, OnInit, Input, DoCheck, ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'rs-ask-check',
  templateUrl: './ask-check.component.html',
  styleUrls: ['./ask-check.component.scss']
})
export class AskCheckComponent extends CheckBoxComponent implements OnInit, DoCheck {

  @Input() check: check;
  constructor(
    public rsService: ReportingStudioService,
    public adService: AskdialogService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef:ChangeDetectorRef) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  public oldValue: boolean;

  ngOnInit() {
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
