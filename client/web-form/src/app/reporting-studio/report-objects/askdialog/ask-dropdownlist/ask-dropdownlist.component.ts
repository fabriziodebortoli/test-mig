import { ComboSimpleComponent } from './../../../../shared/controls/combo-simple/combo-simple.component';

import { dropdownlist, dropdownListPair } from './../../../reporting-studio.model';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-ask-dropdownlist',
  templateUrl: './ask-dropdownlist.component.html',
  styleUrls: ['./ask-dropdownlist.component.scss']
})
export class AskDropdownlistComponent extends ComboSimpleComponent {

  @Input() dropdownlist: dropdownlist;

  getDefItem() {
    for (let i = 0; i < this.dropdownlist.list.length; i++) {
      const elem: dropdownListPair = this.dropdownlist.list[i];
      if (elem.code.toString() === this.dropdownlist.value) {
        return elem;
      }
    }
  }
}
