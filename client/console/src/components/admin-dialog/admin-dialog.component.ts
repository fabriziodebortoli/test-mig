import { Component, EventEmitter, Input, Output, OnInit, AfterContentChecked, AfterContentInit, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'admin-dialog',
  templateUrl: './admin-dialog.component.html',
  styleUrls: ['./admin-dialog.component.css']
})
export class AdminDialogComponent implements OnInit, OnDestroy {

  @Input() mode: string; // available modes: yesno, message, fields
  @Input() title: string;
  @Input() message: string;
  @Input() fields: Array<{label:string, value:string, hide:boolean}>;
  @Input() opened: boolean;
  @Input() result: boolean;
  
  @Output() openedChange: EventEmitter<any>;
  @Output() resultChange: EventEmitter<any>;
  @Output() fieldsChange: EventEmitter<any>;
  @Output() onClose: EventEmitter<any>;

  originalFields: Array<{label:string, value:string, hide:boolean}>; // we hold a copy of original data, just in case user change and press Cancel
 
  constructor() { 
    this.mode = '';
    this.title = '';
    this.message = '';
    this.fields = new Array<{label:string, value:string, hide:boolean}>();
    this.originalFields = new Array<{label:string, value:string, hide:boolean}>();
    this.opened = false;
    this.result = false;
    this.resultChange = new EventEmitter<any>();
    this.openedChange = new EventEmitter<any>();
    this.fieldsChange = new EventEmitter<any>();
  }

  ngOnInit() {
    this.fields.forEach(i => {
      this.originalFields.push({label:i['label'], value:i['value'], hide:i['hide']});
    });
  }

  ngOnDestroy(): void {
    this.fields = new Array<{label:string, value:string, hide:boolean}>();
    this.originalFields = new Array<{label:string, value:string, hide:boolean}>();
  }  

  public close(status) {
    this.result = status === 'yes';
    this.resultChange.emit(this.result);

    if (this.mode === 'fields'){
      
      if (status === 'no') {
        // restoring previous values
        this.fields = new Array<{label:string, value:string, hide:boolean}>();
        this.originalFields.forEach(i => {
          this.fields.push({label:i['label'], value:i['value'], hide:i['hide']});
        });
      } else {
        // updating new originalFields
        this.originalFields = new Array<{label:string, value:string, hide:boolean}>();
        this.fields.forEach(i => {
          this.originalFields.push({label:i['label'], value:i['value'], hide:i['hide']});
        });
      }

      this.fieldsChange.emit(this.fields);
    }

    this.opened = false;
    this.openedChange.emit(false);

    // this event doesn't update any model,
    // it's just a closing point for the consuming app
    this.onClose.emit(true);
  }

}
