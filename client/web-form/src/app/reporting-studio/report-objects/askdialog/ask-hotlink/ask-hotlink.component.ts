import { Observable } from 'rxjs/Rx';
import { ReportingStudioService } from './../../../reporting-studio.service';
import { hotlink, CommandType } from './../../../reporting-studio.model';
import { Component, Input, DoCheck, KeyValueDiffers, Output, EventEmitter, ViewChild, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'rs-ask-hotlink',
  templateUrl: './ask-hotlink.component.html',
  styleUrls: ['./ask-hotlink.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class AskHotlinkComponent implements DoCheck {


  @Input() hotlink: hotlink;
  differ: any;
  showTable: boolean = false;
  showOptions: boolean = false;
  selectionColumn: string = '';
  multiSelectedValues: any[] = [];

  constructor(private rsService: ReportingStudioService, private differs: KeyValueDiffers) {

    this.differ = differs.find({}).create(null);
  }

  // ---------------------------------------------------------------------------------------
  ngDoCheck() {
    if (this.hotlink === undefined) {
      return;
    }

    let hotLinkChanged = this.differ.diff(this.hotlink.values);
    if (hotLinkChanged && this.hotlink.values && this.hotlink.values.rows) {
      this.selectionColumn = this.hotlink.values.key;
      if (this.hotlink.multi_selection && this.multiSelectedValues.length > 0) {
        for (let i = 0; i < this.hotlink.values.rows.length; i++) {
          let item = this.hotlink.values.rows[i];
          if (this.multiSelectedValues.indexOf(item[this.selectionColumn]) >= 0) {
            item.Selected = true;
          }
        }
      }
      this.showTable = true;
    }
  }

  // ---------------------------------------------------------------------------------------
  onButtonClick() {

    if (this.showTable) {
      this.showTable = false;
      return;
    }

    let msg = {
      ns: this.hotlink.ns,
      filter: this.hotlink.multi_selection ? '' : this.hotlink.value,
      selection_type: this.hotlink.selection_type,
      name: this.hotlink.name
    };

    let message = {
      commandType: CommandType.HOTLINK,
      message: JSON.stringify(msg),
      page: this.hotlink.id
    };

    this.rsService.doSend(JSON.stringify(message));
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
  toggleOptions() {
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
