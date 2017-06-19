import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-view',
  template: "<ng-content></ng-content>",
  styles: [":host(tb-view) { display: flex; flex: 1; max-width: 100%; } "]
})
export class ViewComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
