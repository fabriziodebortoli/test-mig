<<<<<<< HEAD
import { Component, Input } from '@angular/core';
import { ViewModeType } from '../../';
=======
import { Component, OnInit, Input } from '@angular/core';
>>>>>>> 1900989819e01352802e2b1c314c29cfeecd2324

@Component({
  selector: 'tb-toolbar-top',
  templateUrl: './toolbar-top.component.html',
  styleUrls: ['./toolbar-top.component.scss']
})

export class ToolbarTopComponent {

<<<<<<< HEAD
  @Input('title') title: string = '...';
=======
  constructor() {

  }

  ngOnInit() {
  }
>>>>>>> 1900989819e01352802e2b1c314c29cfeecd2324

  @Input('viewModeType') viewModeType: ViewModeType = ViewModeType.D;
  viewModeTypeModel = ViewModeType;

  constructor() { }

}
