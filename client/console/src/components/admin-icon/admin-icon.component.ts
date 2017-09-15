
import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'admin-icon',
  templateUrl: './admin-icon.component.html',
  styleUrls: ['./admin-icon.component.css']
})
export class AdminIconComponent {

  @Input() iconSignature: string;

  constructor() { }
}
