import { CheckBoxComponent } from './../../../../shared/controls/checkbox/checkbox.component';
import { check } from './../../../reporting-studio.model';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-ask-check',
  templateUrl: './ask-check.component.html',
  styleUrls: ['./ask-check.component.scss']
})
export class AskCheckComponent extends CheckBoxComponent implements OnInit {

  @Input() check: check;
  constructor() {
    super();
  }

  ngOnInit() {

    this.check.value = (this.check.value === 'True');
  }

}
