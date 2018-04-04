import { askGroup } from './../../../models/ask-group.model';


import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-ask-group',
  templateUrl: './ask-group.component.html',
  styleUrls: ['./ask-group.component.scss']
})
export class AskGroupComponent implements OnInit {

  @Input () askGroup: askGroup;

  
  constructor() { }

  ngOnInit() {
  }

}
