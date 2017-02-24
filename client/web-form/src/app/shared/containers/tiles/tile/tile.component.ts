import { Component, OnInit, Input } from '@angular/core';
import { TbComponent } from '../../../';


@Component({
  selector: 'tb-tile',
  templateUrl: './tile.component.html',
  styleUrls: ['./tile.component.scss']
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
