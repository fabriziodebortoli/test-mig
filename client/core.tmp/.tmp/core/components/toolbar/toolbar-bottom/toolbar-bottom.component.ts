import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-toolbar-bottom',
  template: "<div class=\"toolbar-bottom\"> <div class=\"status-bar\"> <span>{{statusMessage}}</span> </div> <div class=\"buttons\"> <ng-content></ng-content> </div> </div>",
  styles: [".toolbar-bottom { height: 40px; line-height: 40px; background: #1976d2; display: flex; flex-direction: row; flex-wrap: nowrap; justify-content: space-between; align-items: stretch; width: 100%; } .toolbar-bottom .status-bar { padding-left: 5px; } .toolbar-bottom .status-bar > span { color: #fff; font-size: 12px; } "]
})
export class ToolbarBottomComponent implements OnInit {

  statusMessage = '';

  constructor() { }

  ngOnInit() {
  }

}
