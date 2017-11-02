import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'admin-input-text',
  templateUrl: './admin-input-text.component.html',
  styleUrls: ['./admin-input-text.component.css']
})
export class AdminInputTextComponent implements OnInit {

  @Input() material: boolean;
  @Input() inputLabel: string;
  @Input() readOnly: boolean;
  @Input() textType: string;
  @Input() inputData: string;
  @Output() inputDataChange;

  marginTopStyle: string;
  displayStyle: string;

  constructor() { 
    this.material = false;
    this.textType = '';
    this.inputLabel = '';
    this.inputData = '';
    this.readOnly = false;
    this.inputDataChange = new EventEmitter();
  }

  ngOnInit() {
    if (this.material){
      this.marginTopStyle = 'margin-top:-2px';
      this.displayStyle = 'block';
      return;
    }

    this.marginTopStyle = '';
    this.displayStyle = 'inline';
  }

  change(newValue) {
    this.inputData = newValue;
    this.inputDataChange.emit(newValue);
  }
}
