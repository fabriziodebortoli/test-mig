
import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { fadeInContent } from '@angular/material';

@Component({
  selector: 'admin-list',
  templateUrl: './admin-list.component.html',
  styleUrls: ['./admin-list.component.css']
})
export class AdminListComponent implements OnInit {

  @Input() items: Array<object>;
  @Input() columnNames: Array<string>;
  @Input() rowCommand: string;
  @Input() readingData: boolean;
  @Input() showHeader: boolean;
  
  @Output() onSelectedItem: EventEmitter<object> = new EventEmitter<object>();
  @Output() onFireRowCommand: EventEmitter<object> = new EventEmitter<object>();

  supportedCommands: string[] = ['delete'];
  rowCommandIcon: string;

  constructor() { 
    this.showHeader = false;
  }

  ngOnInit() {

    if (this.rowCommand == undefined || this.rowCommand == '') {
      return;
    }

    // check if @Input command is supported and valid
    if (this.supportedCommands.find(k => k == this.rowCommand) == '') {
      return;
    }

    switch (this.rowCommand)
    {
      case 'delete':
        this.rowCommandIcon = 'delete';
        break;
      default:
        this.rowCommandIcon = '';
        break;
    }
  }

  goEditMode(item:object) {
    this.onSelectedItem.emit(item);
  }

  fireRowCommand(item:object){
    this.onFireRowCommand.emit(item);
  }
}
