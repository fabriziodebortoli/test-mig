
import { Component, NgModule, trigger, transition, style, animate, state, Input } from '@angular/core';

@Component({
  selector: 'tb-dockpane',
  templateUrl: './dockpane.component.html',
  styleUrls: ['./dockpane.component.scss']
})
export class DockpaneComponent {
  @Input() title: string = '???';
  active: boolean;
}
