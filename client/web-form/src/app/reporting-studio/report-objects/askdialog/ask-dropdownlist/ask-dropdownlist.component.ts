import { ComboComponent } from './../../../../shared/controls/combo/combo.component';


import { dropdownlist, dropdownListPair } from './../../../reporting-studio.model';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-ask-dropdownlist',
  templateUrl: './ask-dropdownlist.component.html',
  styleUrls: ['./ask-dropdownlist.component.scss']
})
export class AskDropdownlistComponent {

  @Input() dropdownlist: dropdownlist;
  private selectedValue;

  constructor() {

  }

  onChange(value: any) {
    this.dropdownlist.value = value.code;
  }

  getDefItem() {
    for (let i = 0; i < this.dropdownlist.list.length; i++) {
      const elem: dropdownListPair = this.dropdownlist.list[i];
      if (elem.code === this.dropdownlist.value) {
        this.selectedValue = elem;
        return this.selectedValue;
      }
    }
  }
}
