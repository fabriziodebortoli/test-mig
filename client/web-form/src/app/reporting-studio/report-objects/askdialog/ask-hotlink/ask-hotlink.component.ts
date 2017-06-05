import { Observable } from 'rxjs/Rx';
import { ReportingStudioService } from './../../../reporting-studio.service';
import { hotlink, CommandType } from './../../../reporting-studio.model';
import { Component, Input, DoCheck, KeyValueDiffers, Output, EventEmitter, ViewChild } from '@angular/core';

@Component({
  selector: 'rs-ask-hotlink',
  templateUrl: './ask-hotlink.component.html',
  styleUrls: ['./ask-hotlink.component.scss']
})
export class AskHotlinkComponent implements DoCheck {


  @Input() hotlink: hotlink;
  differ: any;
  showTable: boolean = false;
  filter: string = '';
  selectionCreteria: string = '';
  constructor(private rsService: ReportingStudioService, private differs: KeyValueDiffers) {

    this.differ = differs.find({}).create(null);
  }


  ngDoCheck() {
    if (this.hotlink === undefined) {
      return;
    }
    let changes = this.differ.diff(this.hotlink);
    if (changes && this.hotlink.values && this.hotlink.values.rows) {
      this.selectionCreteria = this.hotlink.values.key;
      this.showTable = true;
    }
  }

  onButtonClick() {
    let msg = {
      ns: this.hotlink.ns,
      filter: this.filter,
      name: this.hotlink.name
    };

    let message = {
      commandType: CommandType.HOTLINK,
      message: JSON.stringify(msg),
      page: this.hotlink.id
    };

    this.rsService.doSend(JSON.stringify(message));
  }

  selectionChanged(value: any) {
    let k = this.hotlink.values.rows[value.index];
    this.hotlink.value = k[this.selectionCreteria];
    this.filter = this.hotlink.value;
  }

  popupStyle() {
    return {
      'max-width': '50%',
      'font-size': 'small'
    };
  }

  inputStyle() {
    return {
      'width': '60%',
    };
  }

  onBlur() {
    this.showTable = false;
  }

  showFilteredTable() {
    if (this.showTable) {
      this.showTable = false;
    }
    else if (this.hotlink.values && this.hotlink.values.rows) {
      this.showTable = true;
    }
  }
}
