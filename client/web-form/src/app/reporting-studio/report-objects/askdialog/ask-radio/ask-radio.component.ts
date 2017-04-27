import { radio } from './../../../reporting-studio.model';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-ask-radio',
  templateUrl: './ask-radio.component.html',
  styleUrls: ['./ask-radio.component.scss']
})
export class AskRadioComponent implements OnInit {

  @Input() radio : radio;
  @Input() otherRadios: radio[];
  constructor() { }

  ngOnInit() {
    this.radio.value=(this.radio.value==='True');
  }

  public checkedRadio(){
    /*for (let i = 0; i < this.otherRadios.entries.length; i++) {
      let elem = this.otherRadios.entries[i];
      if(this.radio.id == elem.id){
        elem.value = !elem.value;
      
      }

    }*/
    this.radio.value = !this.radio.value;


  }

}
