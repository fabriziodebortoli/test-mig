import { Component, OnInit } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-file',
  templateUrl: './file.component.html',
  styleUrls: ['./file.component.scss']
})
export class FileComponent extends ControlComponent {
  ngOnInit() {
  }

}
