import { Component, OnInit, Input } from '@angular/core';

import { StateButton } from '../../../shared/models/state-button.model';

@Component({
  selector: 'tb-state-button',
  template: "<md-icon (click)=\"onClick()\" id=\"{{button.IDD_Comand}}\">{{button.iconFont}}</md-icon>",
  styles: [""]
})
export class StateButtonComponent implements OnInit {

  @Input() button: StateButton;
  constructor() { }

  ngOnInit() {
  }

  onClick() {
  }
}
