import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-frame-content',
  template: "<ng-content></ng-content>",
  styles: [":host(tb-frame-content) { flex: 1 1 auto; overflow: hidden; display: flex; flex-direction: column; } :host(tb-frame-content).scroll { overflow: scroll; } "]
})
export class FrameContentComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
