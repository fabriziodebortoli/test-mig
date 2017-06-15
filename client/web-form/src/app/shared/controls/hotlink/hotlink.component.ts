import { HttpService } from './../../../core/http.service';

import { ControlComponent } from './../control.component';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-hotlink',
  templateUrl: './hotlink.component.html',
  styleUrls: ['./hotlink.component.scss']
})

export class HotlinkComponent extends ControlComponent {

  @Input() namespace: string;
  @Input() enableMultiSelection: boolean = false;
  public isReport: boolean = false;
  public data: any;
  public selectionTypes: any[] = [];
  public selectionType: string = 'code';

  showTable: boolean = false;
  showOptions: boolean = false;
  selectionColumn: string = '';
  multiSelectedValues: any[] = [];

  constructor(private httpService: HttpService) {
    super();
  }

  // ---------------------------------------------------------------------------------------
  onSearchClick() {

    if (this.showTable) {
      this.showTable = false;
      return;
    }

    let subs = this.httpService.getHotlinkData(this.namespace, this.selectionType, this.enableMultiSelection ? '' : this.value).subscribe((json) => {
      this.data = json;
      subs.unsubscribe();
      this.showTable = true;
    })
  }

  // ---------------------------------------------------------------------------------------
  selectionChanged(value: any) {
    if (this.enableMultiSelection) {
      return;
    }
    let k = this.data.rows[value.index];
    this.value = k[this.selectionColumn];
  }

  // ---------------------------------------------------------------------------------------
  onFocus() {
    this.showTable = false;
    this.showOptions = false;
  }

   // ---------------------------------------------------------------------------------------
  onBlur() {
    this.showTable = false;
    this.showOptions = false;
     let subs = this.httpService.getHotlinkData(this.namespace, 'direct', this.enableMultiSelection ? '' : this.value).subscribe((json) => {
      this.data = json;
      subs.unsubscribe();
      this.showTable = true;
    })
  }

  // ---------------------------------------------------------------------------------------
  onOptionsClick() {

    if (this.selectionTypes.length === 0) {
      let subs = this.httpService.getHotlinkSelectionTypes(this.namespace).subscribe((json) => {
        this.selectionTypes = json.selections;
        subs.unsubscribe();
      })
      this.showOptions = true;
      return;
    }
    this.showOptions = !this.showOptions;
    this.showTable = false;
  }

  // ---------------------------------------------------------------------------------------
  onRowChecked(event, dataItem) {
    if (dataItem.Selected === undefined) {
      dataItem.Selected = false;
    }

    dataItem.Selected = !dataItem.Selected;

    if (dataItem.Selected) {
      this.multiSelectedValues.push(dataItem[this.selectionColumn]);
      this.multiSelectedValues.sort();
    }
    else {
      let index = this.multiSelectedValues.indexOf(dataItem[this.selectionColumn]);
      this.multiSelectedValues.splice(index, 1);
    }

    this.value = '';
    this.multiSelectedValues.forEach(item => {

      this.value += ' ' + item + ',';

    });

    if (this.multiSelectedValues.length > 0) {
      this.value = this.value.substring(0, this.value.length - 1);
    }
  }

  // Styling
  // ---------------------------------------------------------------------------------------
  popupStyle() {
    return {
      'max-width': '50%',
      'font-size': 'small'
    };
  }

  // ---------------------------------------------------------------------------------------
  inputStyle() {
    return {
      'width': '60%',
    };
  }
}



