import { Component, Input } from '@angular/core';

import { MenuService } from './../../../services/menu.service';

@Component({
  selector: 'tb-menu-stepper',
  templateUrl: './menu-stepper.component.html',
  styleUrls: ['./menu-stepper.component.scss']
})
export class MenuStepperComponent {
  @Input() menu: Array<any>;
  constructor(public menuService: MenuService) { }
}