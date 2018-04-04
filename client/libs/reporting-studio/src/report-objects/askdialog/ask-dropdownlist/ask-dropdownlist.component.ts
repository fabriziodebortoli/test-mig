import { dropdownListPair } from './../../../models/dropdown-list-pair.model';
import { dropdownlist } from './../../../models/dropdownlist.model';
import { AskdialogService } from './../askdialog.service';

import { LayoutService } from '@taskbuilder/core';

import { ReportingStudioService } from './../../../reporting-studio.service';
import { ComboSimpleComponent, TbComponentService } from '@taskbuilder/core';


import { Component, OnInit, Input, ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'rs-ask-dropdownlist',
  templateUrl: './ask-dropdownlist.component.html',
  styleUrls: ['./ask-dropdownlist.component.scss']
})
export class AskDropdownlistComponent extends ComboSimpleComponent {

  @Input() dropdownlist: dropdownlist;

  constructor(
    public rsService: ReportingStudioService,
    public adService: AskdialogService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }


  getDefItem() {
    for (let i = 0; i < this.dropdownlist.list.length; i++) {
      const elem: dropdownListPair = this.dropdownlist.list[i];
      if (elem.code.toString() === this.dropdownlist.value) {
        return elem;
      }
    }
  }

  onChange(value) {
    if (this.dropdownlist.runatserver) {
      /*let obj = {
        id: this.dropdownlist.id,
        value: this.dropdownlist.value.toString()
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
