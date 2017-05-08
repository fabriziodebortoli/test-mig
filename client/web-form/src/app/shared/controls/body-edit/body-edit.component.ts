import { ControlComponent } from './../control.component';
import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

@Component({
  selector: 'tb-body-edit',
  templateUrl: './body-edit.component.html',
  styleUrls: ['./body-edit.component.scss']
})
export class BodyEditComponent extends ControlComponent {
  @Input() columns: Array<any>;
  constructor() {
    super();

  }

}

