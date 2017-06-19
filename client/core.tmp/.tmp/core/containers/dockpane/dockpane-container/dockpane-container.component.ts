import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-dockpane-container',
  template: "<ng-content></ng-content>",
  styles: [":host(tb-dockpane-container) { display: none; } "]
})
export class DockpaneContainerComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
