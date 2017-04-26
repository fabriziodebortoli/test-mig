import { ControlComponent } from './../control.component';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-file',
  templateUrl: './file.component.html',
  styleUrls: ['./file.component.scss']
})
export class FileComponent extends ControlComponent {

  constructor() { super(); }

  ngOnInit() {
  }

}
