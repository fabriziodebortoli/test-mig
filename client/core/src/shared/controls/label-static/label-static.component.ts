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

  @Input() public hotLink: { namespace: string, name: string };
  //TODOLUCA, aggiungere derivazione da textedit, e spostare rows e chars come gestione nel componente text
  @Input('rows') rows: number = 0;
  @Input('chars') chars: number = 0;
  
  constructor(
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

}