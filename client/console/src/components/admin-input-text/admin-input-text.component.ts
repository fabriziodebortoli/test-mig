import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'admin-input-text',
  templateUrl: './admin-input-text.component.html',
  styleUrls: ['./admin-input-text.component.css']
})
export class AdminInputTextComponent {

  @Input() material: boolean;
  @Input() inputLabel: string;
  @Input() inputData: string;
  @Input() readOnly: boolean;
  @Input() textType: string;
  @Output() inputDataChange;

  constructor() { 
    this.material = false;
    this.textType = '';
    this.inputLabel = '';
    this.inputData = '';
    this.readOnly = false;
    this.inputDataChange = new EventEmitter();
  }

  change(newValue) {
    this.inputData = newValue;
    this.inputDataChange.emit(newValue);
  }
}
