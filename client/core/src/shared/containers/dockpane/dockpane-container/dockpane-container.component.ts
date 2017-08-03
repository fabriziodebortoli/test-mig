import { TabberComponent } from './../../tabs/tabber/tabber.component';
import { DockpaneComponent } from './../dockpane.component';
import { Component, OnInit, AfterContentInit, ContentChildren, QueryList, ViewChild, trigger, transition, style, animate, state } from '@angular/core';


//https://embed.plnkr.co/7qWuHmbFFie4p8Y9h53T/
@Component({
  selector: 'tb-dockpane-container',
  templateUrl: './dockpane-container.component.html',
  styleUrls: ['./dockpane-container.component.scss'],
  // animations: [
  //    trigger('isVisibleChanged', [
  //     state('true' , style({ transform: 'translateX(100%)' })),
  //     state('false', style({ transform: 'translateX(0%)'  })),
  //     transition('1 => 0', animate('900ms')),
  //     transition('0 => 1', animate('900ms'))
  //   ])
    // trigger('slideInOut', [
    //   state('opened', style({ width: '*' })),
    //         state('closed', style({ width: 0})),
    //         transition('opened <=> closed', animate('400ms ease-in-out')),
    // ]),
  //],
})
export class DockpaneContainerComponent implements AfterContentInit {
    menuState:boolean = false;
    width: number = 0;

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
  // setStyles() {
  //       let styles = {
  //           'width':  this.menuState ? '400px' : '0px',     
  //       };
  //       return styles;
  //   }

  
  changeTabByIndex(event) {
    let index = event.index;

    let currentTile = this.tiles.toArray()[index];
    this.selectTile(currentTile);
  }
 
 
  toggleMenu(tile: DockpaneComponent) {
    // 1-line if statement that toggles the value:
     if(tile.active)
      this.menuState = !this.menuState;
    else
      this.menuState = true;
    this.width = this.menuState ? 400 : 0;
  }
}