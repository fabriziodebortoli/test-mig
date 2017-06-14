import { HttpService } from './../../../../core/http.service';
import { Observable } from 'rxjs/Rx';
import { ReportingStudioService } from './../../../reporting-studio.service';
import { hotlink, CommandType } from './../../../reporting-studio.model';
import { Component, Input, DoCheck, KeyValueDiffers, Output, EventEmitter, ViewChild, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'rs-ask-hotlink',
  templateUrl: './ask-hotlink.component.html',
  styleUrls: ['./ask-hotlink.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class AskHotlinkComponent {


  @Input() hotlink: hotlink;

  showTable: boolean = false;
  showOptions: boolean = false;
  selectionColumn: string = '';
  multiSelectedValues: any[] = [];

  constructor(private httpService: HttpService) {
  }

  // ---------------------------------------------------------------------------------------
  onSearchClick() {

    if (this.showTable) {
      this.showTable = false;
      return;
    }

    let subs = this.httpService.getHotlinkData(this.hotlink.ns, this.hotlink.selection_type, this.hotlink.multi_selection ? '' : this.hotlink.value).subscribe((json) => {
      this.hotlink.values = json;
      subs.unsubscribe();
      this.showTable = true;
    })
  }

  // ---------------------------------------------------------------------------------------
  selectionChanged(value: any) {
    if (this.hotlink.multi_selection) {
      return;
    }
    let k = this.hotlink.values.rows[value.index];
    this.hotlink.value = k[this.selectionColumn];
  }

  // ---------------------------------------------------------------------------------------
  onBlur() {
    this.showTable = false;
    this.showOptions = false;
  }

  // ---------------------------------------------------------------------------------------
  onOptionsClick() {

    if (this.hotlink.selectionList.length === 0) {
      let subs = this.httpService.getHotlinkSelectionTypes(this.hotlink.ns).subscribe((json) => {
        this.hotlink.selectionList = json.selections;
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

    this.hotlink.value = '';
    this.multiSelectedValues.forEach(item => {

      this.hotlink.value += ' ' + item + ',';

    });

    if (this.multiSelectedValues.length > 0) {
      this.hotlink.value = this.hotlink.value.substring(0, this.hotlink.value.length - 1);
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
