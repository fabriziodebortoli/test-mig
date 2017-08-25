import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-body-edit',
  templateUrl: './body-edit.component.html',
  styleUrls: ['./body-edit.component.scss']
})
export class BodyEditComponent extends ControlComponent {
  @Input() columns: Array<any>;
  constructor(layoutService: LayoutService, tbComponentService:TbComponentService) {
    super(layoutService, tbComponentService);

  }

}