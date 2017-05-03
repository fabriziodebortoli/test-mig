import { ReportingStudioService } from './../../../reporting-studio.service';
import { ComboSimpleComponent } from './../../../../shared/controls/combo-simple/combo-simple.component';

import { dropdownlist, dropdownListPair, CommandType } from './../../../reporting-studio.model';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-ask-dropdownlist',
  templateUrl: './ask-dropdownlist.component.html',
  styleUrls: ['./ask-dropdownlist.component.scss']
})
export class AskDropdownlistComponent extends ComboSimpleComponent {

  @Input() dropdownlist: dropdownlist;

  constructor(private rService: ReportingStudioService) {
    super();
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
      let obj = {
        id: this.dropdownlist.id,
        value: this.dropdownlist.value.toString()
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
