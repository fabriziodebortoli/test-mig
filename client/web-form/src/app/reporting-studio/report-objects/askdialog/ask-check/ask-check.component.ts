import { CheckBoxComponent } from './../../../../shared/controls/checkbox/checkbox.component';
import { check } from './../../../reporting-studio.model';
import { Component, OnInit, Input, DoCheck } from '@angular/core';

@Component({
  selector: 'rs-ask-check',
  templateUrl: './ask-check.component.html',
  styleUrls: ['./ask-check.component.scss']
})
export class AskCheckComponent extends CheckBoxComponent implements OnInit, DoCheck {

  @Input() check: check;
  constructor() {
    super();
  }

  private oldValue: boolean;

  ngOnInit() {

    this.check.value = (this.check.value === 'True');
    this.oldValue = this.check.value;
  }

 

  ngDoCheck() {
    if (this.oldValue != this.check.value) {
      this.oldValue = this.check.value;
      if (this.check.runatserver) {
        
      }
    }
  }
}
