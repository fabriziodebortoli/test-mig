import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-toolbar-top',
  templateUrl: './toolbar-top.component.html',
  styleUrls: ['./toolbar-top.component.scss']
})

export class ToolbarTopComponent implements OnInit {

  private docTitle: string = 'Language'; // TODO read document title?

  constructor() {

  }

  ngOnInit() {
  }



}
