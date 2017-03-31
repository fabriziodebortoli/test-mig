import { Component, OnInit } from '@angular/core';
import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-label-static',
  templateUrl: './label-static.component.html',
  styleUrls: ['./label-static.component.scss']
})
export class LabelStaticComponent  extends ControlComponent implements OnInit {

  ngOnInit() {
  }

}
