import { TabberComponent } from './../../tabs/tabber/tabber.component';
import { DockpaneComponent } from './../dockpane.component';
import { Component, OnInit, ElementRef, ViewEncapsulation, AfterContentInit, ContentChildren, QueryList, ViewChild, trigger, transition, style, animate, state } from '@angular/core';


//https://embed.plnkr.co/7qWuHmbFFie4p8Y9h53T/
@Component({
  selector: 'tb-dockpane-container',
  templateUrl: './dockpane-container.component.html',
  styleUrls: ['./dockpane-container.component.scss'],
  //  animations: [   
  //   trigger('slideInOut', [
  //     state('in', 
  //     style({transform: 'translateX(100%)'}),
  //     style({width: '0%'}),
  //   ),
  //     state('out', 
  //     style({transform: 'translateX(50%)'}),
  //     style({width: '100%'})),
  //     transition('in => out', animate('400ms ease-in-out')),
  //     transition('out => in', animate('400ms ease-in-out'))
  //   ]),
  // ],
  encapsulation: ViewEncapsulation.None
})
export class DockpaneContainerComponent implements AfterContentInit {
  public menuState:string = 'in';
  
  @ContentChildren(DockpaneComponent) tiles: QueryList<DockpaneComponent>;

  getTiles() {
    return this.tiles.toArray();
  }

  // private viewHeightSubscription: Subscription;
  // viewHeight: number;

  constructor() { 
  }

  // ngOnInit() {
  //   this.viewHeightSubscription = this.layoutService.getViewHeight().subscribe((viewHeight) => this.viewHeight = viewHeight);
  // }

  // ngOnDestroy() {
  //   this.viewHeightSubscription.unsubscribe();
  // }

  ngAfterContentInit() {
    // get all active tiles
    let activeTiles = this.tiles.filter((tile) => tile.active);
   
    //if there is no active tab set, activate the first
    if (activeTiles.length === 0 && this.tiles.toArray().length > 0) {
      this.selectTile(this.tiles.first);
    }
  }

  selectTile(tile: DockpaneComponent) {
    if (tile.active) return;

    // deactivate all tabs
    this.tiles.toArray().forEach(tile => tile.active = false);

    // activate the tab the user has clicked on.
    tile.active = true;
  }

  changeTabByIndex(event) {
    let index = event.index;

    let currentTile = this.tiles.toArray()[index];
    this.selectTile(currentTile);
  }
 
 
  toggleMenu(tile: DockpaneComponent) {
     this.menuState = tile.active ? "out" : this.menuState === 'out' ? 'in' : 'out';
  }
}