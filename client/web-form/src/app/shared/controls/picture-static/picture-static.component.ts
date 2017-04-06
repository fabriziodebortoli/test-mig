import { ControlComponent } from './../control.component';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-picture-static',
  templateUrl: './picture-static.component.html',
  styleUrls: ['./picture-static.component.scss']
})
export class PictureStaticComponent extends ControlComponent implements OnInit {

  constructor() {
    super();
    console.log(this.model);
  }

  ngOnInit() {
  }

}
