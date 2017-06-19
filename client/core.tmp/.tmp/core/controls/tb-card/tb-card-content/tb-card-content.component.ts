import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-card-content',
  template: "<ng-content></ng-content>",
  styles: [""]
})
export class TbCardContentComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
