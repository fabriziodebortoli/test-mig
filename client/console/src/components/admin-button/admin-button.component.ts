import { Component, Input, Output, EventEmitter, Inject } from '@angular/core';

@Component({
  selector: 'admin-button',
  templateUrl: './admin-button.component.html',
  styleUrls: ['./admin-button.component.css']
})
export class AdminButtonComponent {

  @Input() buttonText: string;

  constructor() {
    this.buttonText = '';
  }
}