import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-dockpane',
  template: "<div class=\"tb-dockpane\"> <ng-content></ng-content> </div>",
  styles: [""]
})
export class DockpaneComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
