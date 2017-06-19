import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-layoutcontainer',
  template: "<ng-content></ng-content>",
  styles: [":host { display: flex; flex-direction: column; margin-bottom: 16px; } :host(.layoutType-column) { flex-direction: row; flex: 0 0 100%; } :host(.layoutType-hbox) { display: flex; flex-direction: row; } :host(.layoutType-vbox) { display: flex; flex-direction: column; } @media screen and (min-width: 75em) { .layout-container { flex-direction: row; } } "]
})
export class LayoutContainerComponent implements OnInit {

  constructor() {
  }
  ngOnInit() {
  }
}