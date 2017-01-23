import { Component, OnInit, Input } from '@angular/core';
import { TbComponent } from '../../../';

@Component({
  selector: 'tb-tile',
  templateUrl: './tile.component.html',
  styleUrls: ['./tile.component.scss']
})
export class TileComponent extends TbComponent implements OnInit {

  @Input()
  title: string;

  private _isCollapsed: boolean = false;
  private _isCollapsible: boolean = true;

  ngOnInit() {
  }

  getExpandCollapseClass() {
    return this._isCollapsed ? 'keyboard_arrow_down' : 'keyboard_arrow_up' ;
  }

  toggleCollapse(event: MouseEvent): void {
    event.preventDefault();
    this._isCollapsed = !this._isCollapsed;
  }

  @Input()
  set isCollapsed(value: boolean) {
    this._isCollapsed = value;
  }

  get isCollapsed() {
    return this._isCollapsed;
  }

    @Input()
  set isCollapsible(value: boolean) {
    this._isCollapsible = value;
  }

  get isCollapsible() {
    return this._isCollapsible;
  }

}
