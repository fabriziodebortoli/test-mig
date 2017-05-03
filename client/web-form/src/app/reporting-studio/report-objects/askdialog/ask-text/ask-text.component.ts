import { text } from './../../../reporting-studio.model';
import { Component, OnInit, Input, Type } from '@angular/core';
import * as moment from 'moment';
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
      const t2 = moment.parseZone(this.text.value, 'DD/MM/YYYY HH:mm:ss').format('YYYY-MM-DDTHH:mm:ss');
      this.text.value = new Date(t2);
    }
  }
}
