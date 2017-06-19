import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-caption',
  template: "<label for=\"{{for}}\" class=\"control-label\">{{caption}}</label>",
  styles: [""]
})
export class CaptionComponent extends ControlComponent {
  @Input() for: string;
}
