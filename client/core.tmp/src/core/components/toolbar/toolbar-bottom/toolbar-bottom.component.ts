import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-toolbar-bottom',
  templateUrl: './toolbar-bottom.component.html',
  styleUrls: ['./toolbar-bottom.component.scss']
})
export class ToolbarBottomComponent implements OnInit {

  statusMessage = '';

  constructor() { }

  ngOnInit() {
  }

}
