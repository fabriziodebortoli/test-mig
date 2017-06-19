import { Component } from '@angular/core';

import { ControlComponent } from '../control.component';

@Component({
  selector: 'tb-unknown',
  template: "<p> unknown works! </p> ",
  styles: [""]
})
export class UnknownComponent extends ControlComponent {

  constructor() {
    super();
  }

}
