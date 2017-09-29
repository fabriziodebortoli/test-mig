import { Component, OnInit, Input, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'tb-tile',
  templateUrl: './tile.component.html',
  styleUrls: ['./tile.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class TileComponent implements OnInit {
  @Input() title: string;
  public _isCollapsed: boolean = false;
  public _isCollapsible: boolean = true;
  public _hasTitle: boolean = true;

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
