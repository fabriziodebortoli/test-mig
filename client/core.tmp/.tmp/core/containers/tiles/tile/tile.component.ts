import { Component, OnInit, Input, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'tb-tile',
  template: "<!--<md-card [ngClass]=\"{'sameHeight': !isCollapsed}\"> <md-card-title (click)=\"toggleCollapse($event)\" *ngIf=\"hasTitle\" [ngClass]=\"{'c-pointer': isCollapsible }\"> <span>{{title}}</span> <md-icon *ngIf=\"isCollapsible\">{{getArrowIcon()}}</md-icon> </md-card-title> <md-card-content *ngIf=\"!isCollapsed\"> <ng-content></ng-content> </md-card-content> </md-card>--> <div [ngClass]=\"{'sameHeight': !isCollapsed}\" class=\"tbcard\"> <tb-card-title [ngClass]=\"{'c-pointer': isCollapsible }\" [isCollapsible]=\"isCollapsible\" [isCollapsed]=\"isCollapsed\" (click)=\"toggleCollapse($event)\" *ngIf=\"hasTitle\" [title]=\"title\"></tb-card-title> <md-card-content *ngIf=\"!isCollapsed\"> <ng-content></ng-content> </md-card-content> </div>",
  styles: [".h200 md-card-content { min-height: 200px; max-height: 200px; overflow: auto; } tb-tile { flex: 1; } tb-tile.tile-micro { flex: 0 0 12.5%; } tb-tile.tile-mini { flex: 0 0 25%; } tb-tile.tile-standard { flex: 0 0 50%; } tb-tile.tile-wide { flex: 0 0 100%; } tb-tile.tile-autofill { flex: 1; } tb-tile.tile-standard md-card.sameHeight { height: 100%; } tb-tile.tile-standard div.sameHeight { height: 100%; } tb-tile.tile-wide md-card-content { display: flex; flex-direction: column; } tb-tile.tile-wide .col { flex: 0 0 100%; } tb-tile.tile-wide .col2 { margin-left: 16px; } .anchored { display: flex; flex-direction: column; } tb-tile > div { background: white; margin: 5px 5px 0px 5px; padding: 5px; } tb-tile > div tb-card-title { position: relative; font-size: 1.2rem; font-weight: 500; color: #000; font-size: 1rem; text-transform: uppercase; } tb-tile > div tb-card-title div { display: flex; flex-wrap: nowrap; flex-basis: row; } tb-tile > div tb-card-title div span { flex: 2 0 0%; align-self: flex-start; } tb-tile > div tb-card-title div md-icon { align-self: flex-end; } tb-tile > div md-card-content { border-top: 0px; display: flex; flex-wrap: nowrap; flex-basis: row; } @media screen and (min-width: 48em) { tb-tile.tile-wide md-card-content { flex-direction: row; } tb-tile.tile-wide .col { flex: 0 0 100%; } } @media screen and (min-width: 75em) { tb-tile.tile-wide md-card-content { flex-direction: row; } tb-tile.tile-wide .col { flex: 0 0 50%; } .anchored { flex-direction: row; } } "],
  encapsulation: ViewEncapsulation.None
})
export class TileComponent implements OnInit {

  @Input('title') title: string;

  private _isCollapsed: boolean = false;
  private _isCollapsible: boolean = true;
  private _hasTitle: boolean = true;

  constructor() { }

  ngOnInit() { }

  getArrowIcon() {
    return this._isCollapsed ? 'keyboard_arrow_down' : 'keyboard_arrow_up';
  }

  toggleCollapse(event: MouseEvent): void {
    if (!this._isCollapsible)
      return;

    // event.preventDefault();
    this._isCollapsed = !this._isCollapsed;
  }

  @Input()
  set isCollapsed(value: boolean) {
    this._isCollapsed = value;
  }

  get isCollapsed(): boolean {
    return this._isCollapsed;
  }

  @Input()
  set isCollapsible(value: boolean) {
    this._isCollapsible = value;
  }

  get isCollapsible(): boolean {
    return this._isCollapsible;
  }

  @Input()
  set hasTitle(value: boolean) {
    this._hasTitle = value;
  }

  get hasTitle(): boolean {
    return this._hasTitle;
  }
}
