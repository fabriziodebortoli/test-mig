import { check } from './../../../reporting-studio.model';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-ask-check',
  templateUrl: './ask-check.component.html',
  styleUrls: ['./ask-check.component.scss']
})
export class AskCheckComponent implements OnInit {

  @Input() check: check;
  constructor() { }

  ngOnInit() {
    this.check.value = (this.check.value === 'True');
  }

  public checked() {
    this.check.value = !this.check.value;
  }

}
