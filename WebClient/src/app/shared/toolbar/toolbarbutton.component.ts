import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-toolbarbutton',
  template: '<span></span>'
})
export class ToolbarButtonComponent implements OnInit {
  @Input() buttonCaption: string = '';
  
  constructor() {

  }

  ngOnInit() {
  }

}