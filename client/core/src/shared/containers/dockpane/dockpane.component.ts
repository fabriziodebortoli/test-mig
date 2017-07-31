<<<<<<< HEAD
import { Component, NgModule, trigger, transition, style, animate, state, OnInit, Input } from '@angular/core';
=======
import { Component, NgModule, trigger, transition, style, animate, state} from '@angular/core';
>>>>>>> 0c7e17dea94e08b0a2c5a3cd4f602608a98203d2

@Component({
  selector: 'tb-dockpane',
  animations: [
    trigger('slideInOut', [
      state('in', style({
        transform: 'translate3d(0, 0, 0)'
      })),
      state('out', style({
        transform: 'translate3d(100%, 0, 0)'
      })),
      transition('in => out', animate('400ms ease-in-out')),
      transition('out => in', animate('400ms ease-in-out'))
    ]),
  ],
  templateUrl: './dockpane.component.html',
  styleUrls: ['./dockpane.component.css']
})


<<<<<<< HEAD
export class DockpaneComponent implements OnInit {
 @Input() title:string;
=======
export class DockpaneComponent {

>>>>>>> 0c7e17dea94e08b0a2c5a3cd4f602608a98203d2

 
  menuState:string = 'out';
 
  toggleMenu() {
    // 1-line if statement that toggles the value:
    this.menuState = this.menuState === 'out' ? 'in' : 'out';
  }
}
