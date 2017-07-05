import { Component, OnInit, Input } from '@angular/core';
import { URLSearchParams } from '@angular/http';

import { HttpService } from './../../../core/services/http.service';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-hotlink',
  templateUrl: './hotlink.component.html',
  styleUrls: ['./hotlink.component.scss']
})

export class HotlinkComponent extends ControlComponent {

  @Input() ns: string;
  @Input() enableMultiSelection: boolean = false;
  public isReport: boolean = false;
  public data: any;
  public selectionTypes: any[] = [];
  public selectionType: string = 'code';
  // private skipBlurFlag: boolean = false;

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

    this.showOptions = false;

    let p: URLSearchParams = new URLSearchParams(this.args);
    for (var key in this.args) {
      if (this.args.hasOwnProperty(key)) {
        var element = this.args[key];
        p.set(key, element);
      }
    }

    let subs = this.httpService.getHotlinkData(this.ns, this.selectionType, this.enableMultiSelection ? '' : this.value, p).subscribe((json) => {
      this.data = json;
      this.selectionColumn = this.data.key;
      if (this.enableMultiSelection && this.multiSelectedValues.length > 0) {
        for (let i = 0; i < this.data.rows.length; i++) {
          let item = this.data.rows[i];
          if (this.multiSelectedValues.indexOf(item[this.selectionColumn]) >= 0) {
            item.Selected = true;
          }
        }
      }
      subs.unsubscribe();
      this.showTable = true;
      this.showOptions = false;
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
  onBlur(value) {
    this.showTable = false;
    this.showOptions = false;
    /*if (this.skipBlurFlag || this.value==='') {
      this.skipBlurFlag = false;
      return;
    }*/
    /* let subs = this.httpService.getHotlinkData(this.ns, 'direct', this.enableMultiSelection ? '' : this.value, undefined).subscribe((json) => {
       this.data = json;
       subs.unsubscribe();
       this.showTable = true;
     })*/
  }

  // ---------------------------------------------------------------------------------------
 /* skipBlur() {
    this.skipBlurFlag = true;
  }
*/
  // ---------------------------------------------------------------------------------------
  onOptionsClick() {

    this.showTable = false;
    if (this.selectionTypes.length === 0) {
      let subs = this.httpService.getHotlinkSelectionTypes(this.ns).subscribe((json) => {
        this.selectionTypes = json.selections;
        subs.unsubscribe();
      })
      this.showOptions = true;
      return;
    }
    this.showOptions = !this.showOptions;
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



