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

  setStyles(background: number, foreground: number, width:number)
  {
//    let intNumber = 16737111;
    let hexNumber = 0xff6357;

    let htmBackColor = '#' + background.toString(16);
    let htmForeColor = '#' + foreground.toString(16);
    let styles = {
      'backgroundColor': htmBackColor,
      'color': htmForeColor,
      'width.px':width
    }
    return styles;
  }
}
