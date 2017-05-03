import { text } from './../../../reporting-studio.model';
import { Component, OnInit, Input, Type, DoCheck } from '@angular/core';
@Component({
  selector: 'rs-ask-text',
  templateUrl: './ask-text.component.html',
  styleUrls: ['./ask-text.component.scss'],
  
})
export class AskTextComponent implements OnInit{

  @Input() text: text;

  constructor() { }


onBlur(value)
{
  let a=value;
}

  ngOnInit() {

    if (this.text.type === 'DateTime') {
      this.text.value = new Date();
    }
  }

}
