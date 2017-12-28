import { Component, OnInit, Input, ChangeDetectorRef } from '@angular/core';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-label-static',
  templateUrl: './label-static.component.html',
  styleUrls: ['./label-static.component.scss']
})
export class LabelStaticComponent extends ControlComponent {

  @Input() public hotLink: { namespace: string, name: string};

  constructor(
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

}