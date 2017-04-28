import { RadioComponent } from './../../../../shared/controls/radio/radio.component';
import { radio } from './../../../reporting-studio.model';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-ask-radio',
  templateUrl: './ask-radio.component.html',
  styleUrls: ['./ask-radio.component.scss']
})
export class AskRadioComponent extends RadioComponent implements OnInit {

  @Input() radio: radio;
  @Input() otherRadios: radio[];
  constructor() { 
    super()
  }

  ngOnInit() {
    //this.radio.value = (this.radio.value === 'True');
  }

  public checkedRadio() {
    for (let i = 0; i < this.otherRadios.length; i++) {
      let elem: radio = this.otherRadios[i];
      if (this.radio.id !== elem.id) {
        elem.value = false;
      }

    }
  }

}
