import { ControlComponent } from './../control.component';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-caption',
  templateUrl: './caption.component.html',
  styleUrls: ['./caption.component.scss']
})
export class CaptionComponent extends ControlComponent {
  @Input() for: string;
}
