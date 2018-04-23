import { Component, OnInit, Input } from '@angular/core';

import { StateButton } from './../../models/state-button.model';

@Component({
  selector: 'tb-state-button',
  templateUrl: './state-button.component.html',
  styleUrls: ['./state-button.component.scss']
})
export class StateButtonComponent implements OnInit {

  @Input() button: StateButton;
  constructor() { }

  ngOnInit() {
  }

  onClick() {
  }
}
