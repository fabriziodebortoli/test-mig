import { radio } from './../../../models/radio.model';

import { AskdialogService } from './../askdialog.service';
import { ReportingStudioService } from './../../../reporting-studio.service';

import { LayoutService, TbComponentService, RadioComponent } from '@taskbuilder/core';
import { Component, OnInit, Input, ViewEncapsulation,ChangeDetectorRef } from '@angular/core';


@Component({
  selector: 'rs-ask-radio',
  templateUrl: './ask-radio.component.html',
  encapsulation: ViewEncapsulation.None,
  styleUrls: ['./ask-radio.component.scss']
})
export class AskRadioComponent extends RadioComponent implements OnInit {

  @Input() radio: radio;
  @Input() otherRadios: radio[];
  constructor(
    public rsService: ReportingStudioService,
    public adService: AskdialogService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef:ChangeDetectorRef
  ) {
    super(layoutService, tbComponentService, changeDetectorRef)
  }

  ngOnInit() {
   
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
      this.adService.askChanged.emit();
    }
  }
}
