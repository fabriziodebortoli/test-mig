import { text } from './../../../reporting-studio.model';
import { Component, OnInit, Input, Type } from '@angular/core';
@Component({
  selector: 'rs-ask-text',
  templateUrl: './ask-text.component.html',
  styleUrls: ['./ask-text.component.scss']
})
export class AskTextComponent implements OnInit {

  @Input() text: text;

  constructor() { }

  ngOnInit() {

    if (this.text.type === 'DateTime') {
      this.text.value = new Date();
    }
  }
}
