import { text } from './../../../reporting-studio.model';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-ask-text',
  templateUrl: './ask-text.component.html',
  styleUrls: ['./ask-text.component.scss']
})
export class AskTextComponent implements OnInit {

  @Input() text: text;

  type: string;

  constructor() { }

  ngOnInit() {
    if (this.text.type === 'DateTime') {
      this.type = 'datetime-local';
      this.text.value = new Date().toDateString();
    }
    if (this.text.type === 'Date') {
      this.type = 'date';
      this.text.value = new Date().toDateString();
    }
    if (this.text.type === 'Text') {
      this.type = 'text';
    }
    if (this.text.type === 'Double') {
      this.type = 'number';
    }
  }

}
