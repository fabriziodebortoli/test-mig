import { StateButton } from './state-button.model';
import { Component, OnInit, Input } from '@angular/core';


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

onClick(){
}
}
