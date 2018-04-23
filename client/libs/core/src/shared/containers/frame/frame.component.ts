import { LayoutService } from './../../../core/services/layout.service';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-frame',
  templateUrl: './frame.component.html',
  styleUrls: ['./frame.component.scss']
})
export class FrameComponent{
  @Input() title = "";
}
