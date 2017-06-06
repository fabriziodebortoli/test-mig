import { Observable } from 'rxjs/Rx';
import { ReportingStudioService } from './../../../reporting-studio.service';
import { hotlink, CommandType } from './../../../reporting-studio.model';
import { Component, Input, DoCheck, KeyValueDiffers, Output, EventEmitter, ViewChild, ViewEncapsulation, OnInit } from '@angular/core';

@Component({
  selector: 'rs-ask-hotlink',
  templateUrl: './ask-hotlink.component.html',
  styleUrls: ['./ask-hotlink.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class AskHotlinkComponent implements DoCheck, OnInit {


  @Input() hotlink: hotlink;
  differ: any;
  showTable: boolean = false;
  showOptions: boolean = false;
  selectionColumn: string = '';
  selectionTypeLower: string = '';
  constructor(private rsService: ReportingStudioService, private differs: KeyValueDiffers) {

    this.differ = differs.find({}).create(null);
  }

  ngOnInit() {
    this.selectionTypeLower = this.hotlink.selection_type.toLocaleLowerCase();
  }

  ngDoCheck() {
    if (this.hotlink === undefined) {
      return;
    }

    let hotLinkChanged = this.differ.diff(this.hotlink);
    if (hotLinkChanged && this.hotlink.values && this.hotlink.values.rows) {
      this.selectionColumn = this.hotlink.values.key;
      this.showTable = true;
    }
  }


  onButtonClick() {

    if (this.showTable) {
      this.showTable = false;
      return;
    }

    let msg = {
      ns: this.hotlink.ns,
      filter: this.hotlink.value[this.selectionTypeLower],
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

  selectionChanged(value: any) {
    let k = this.hotlink.values.rows[value.index];
    this.hotlink.value[this.selectionTypeLower] = k[this.selectionColumn];
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
    this.showOptions = false;
  }

  showFilteredTable() {
    if (this.showTable) {
      this.showTable = false;
    }
    else if (this.hotlink.values && this.hotlink.values.rows) {
      this.showTable = true;
    }
  }

  selectionTypeChanged() {
    let oldValue = this.hotlink.value[this.selectionTypeLower];
    this.hotlink.value[this.selectionTypeLower] = '';
    this.selectionTypeLower = this.hotlink.selection_type.toLowerCase();
    this.hotlink.value[this.selectionTypeLower] = oldValue;
  }
}
