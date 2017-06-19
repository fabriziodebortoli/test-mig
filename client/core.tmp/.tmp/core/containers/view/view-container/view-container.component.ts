import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-view-container',
  template: "<ng-content></ng-content>",
  styles: [":host(tb-view-container) { display: flex; flex: 1; max-width: 100%; } "]
})
export class ViewContainerComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
