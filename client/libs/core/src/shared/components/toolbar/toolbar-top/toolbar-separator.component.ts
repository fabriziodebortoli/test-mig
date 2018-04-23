import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-toolbar-separator',
  template: `<div></div>`,
  styles: [`
    div{
      width:1px;
      height:35px;
      background:#ddd;
    }
  `]
})
export class ToolbarSeparatorComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
