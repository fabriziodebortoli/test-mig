import { Component, ChangeDetectorRef } from '@angular/core';

import { LayoutService } from './../../../core/services/layout.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-coloredstatic',
  templateUrl: './coloredstatic.component.html',
  styleUrls: ['./coloredstatic.component.scss']
})
export class ColoredStaticComponent extends ControlComponent {

  constructor(
    layoutService: LayoutService,
    changeDetectorRef: ChangeDetectorRef,
    tbComponentService: TbComponentService
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  setStyles(background: string, foreground: string)
  {   
    let styles = {
      'backgroundColor': background
    }

    return styles;
  }
}
