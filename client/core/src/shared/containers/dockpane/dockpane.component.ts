
import { Component, NgModule, trigger, transition, style, animate, state, Input } from '@angular/core';

@Component({
  selector: 'tb-dockpane',
  templateUrl: './dockpane.component.html',
  styleUrls: ['./dockpane.component.scss']
})


export class DockpaneComponent {
 @Input() title:string;
   active: boolean;
  @Input() iconType: string = 'M4';
  @Input() icon: string = 'erp-purchaseorder';
  menuState:string = 'out';
 
//   toggleMenu() {
//     // 1-line if statement that toggles the value:
//     this.menuState = this.menuState === 'out' ? 'in' : 'out';
//   }
 }
