import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { EnumsService } from './../../../core/services/enums.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, OnInit, Input, ViewEncapsulation, Type, ChangeDetectorRef, HostListener, ElementRef, ViewChild } from '@angular/core';
import { URLSearchParams } from '@angular/http';
import { HttpService } from './../../../core/services/http.service';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-hotlink',
  templateUrl: './hotlink.component.html',
  styleUrls: ['./hotlink.component.scss']
})

export class HotlinkComponent extends ControlComponent implements OnInit {

  @Input() ns: string;
  @Input() enableMultiSelection: boolean = false;
  public isReport: boolean = false;
  public data: any;
  public selectionTypes: any[] = [];
  public selectionType: string = 'code';
  // public  skipBlurFlag: boolean = false;

  showTable: boolean = false;
  showOptions: boolean = false;
  selectionColumn: string = '';
  multiSelectedValues: any[] = [];

  @ViewChild('anchor') public anchor: ElementRef;
  @ViewChild('popup', { read: ElementRef }) public popup: ElementRef;

  constructor(public httpService: HttpService,
    layoutService: LayoutService,
    public enumService: EnumsService,
    tbComponentService: TbComponentService,
    public cd: ChangeDetectorRef
  ) {
    super(layoutService, tbComponentService, cd);
  }

  // ---------------------------------------------------------------------------------------
  @HostListener('keydown', ['$event'])
  public keydown(event: any): void {
    if (event.keyCode === 27) {
      this.closeOptions();
      this.closeTable();
    }
  }

  // ---------------------------------------------------------------------------------------
  @HostListener('document:click', ['$event'])
  public documentClick(event: any): void {
    if (!this.contains(event.target)) {
      this.closeOptions();
      if ( !this.enableMultiSelection) {
        this.closeTable();
      }
    }
  }

  // ---------------------------------------------------------------------------------------
  private contains(target: any): boolean {
    return (this.anchor ? this.anchor.nativeElement.contains(target) : false) ||
      (this.popup ? this.popup.nativeElement.contains(target) : false);
  }

  // ---------------------------------------------------------------------------------------
  ngOnInit() {
    //Called after the constructor, initializing input properties, and the first call to ngOnChanges.
    //Add 'implements OnInit' to the class.
    this.cd.markForCheck();
  }

  // ---------------------------------------------------------------------------------------
  onSearchClick() {

    if (this.showTable) {
      this.showTable = false;
      return;
    }

    this.showOptions = false;

    let p: URLSearchParams = new URLSearchParams(this.args);
    if (!this.enableMultiSelection && this.value) {
      p.set('filter', this.value)
    }
    for (let key in this.args) {
      if (this.args.hasOwnProperty(key)) {
        let element = this.args[key];
        p.set(key, element);
      }
    }

    let subs = this.httpService.getHotlinkData(this.ns, this.selectionType, p).subscribe((json) => {
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
  SeletctionTypeChanged(type: string) {
    this.selectionType = type;
  }
  // ---------------------------------------------------------------------------------------
  selectionChanged(value: any) {
    if (this.enableMultiSelection) {
      return;
    }
    let k = this.data.rows[value.index];
    this.value = k[this.selectionColumn];
    this.model.value = this.value;
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
  closeTable() {
    this.showTable = false;
  }
  closeOptions() {
    this.showOptions = false;
  }

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
    this.model.value = this.value;
  }

  // ---------------------------------------------------------------------------------------
  getValue(dataItem: string, column) {
    if (column.type === 'Enum') {
      let res = this.enumService.getEnumsItem(Number(dataItem));
      if (res)
        return res.name;
      return dataItem;
    }
    else if (column.Type === 'Boolean') {
      return dataItem ? 'Yes' : 'No';
    }
    return dataItem;
  }

  // Styling
  // ---------------------------------------------------------------------------------------
  popupStyle() {
    return {
      'max-width': '700px',
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



