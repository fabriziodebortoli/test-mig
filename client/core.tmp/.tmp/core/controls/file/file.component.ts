import { Component } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-file',
  template: "<div class=\"tb-control tb-file\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <input type=\"file\"> </div>",
  styles: [""]
})
export class FileComponent extends ControlComponent {

  constructor() { super(); }

}
