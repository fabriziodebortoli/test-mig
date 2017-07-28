import { Component, NgModule, trigger, transition, style, animate, state, OnInit } from '@angular/core';

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


export class DockpaneComponent implements OnInit {


 
  menuState:string = 'out';
 
  toggleMenu() {
    // 1-line if statement that toggles the value:
    this.menuState = this.menuState === 'out' ? 'in' : 'out';
  }
}
