import { LayoutService } from './../../../core/services/layout.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-caption',
  templateUrl: './caption.component.html',
  styleUrls: ['./caption.component.scss']
})
export class CaptionComponent extends ControlComponent {
  @Input() for: string;
  constructor(layoutService: LayoutService, tbComponentService:TbComponentService)
  {
    super(layoutService, tbComponentService);
  }
}
