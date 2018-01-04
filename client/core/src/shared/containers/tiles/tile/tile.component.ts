import { Component, OnInit, Input, ViewEncapsulation} from '@angular/core';

@Component({
  selector: 'tb-tile',
  templateUrl: './tile.component.html',
  styleUrls: ['./tile.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class TileComponent implements OnInit {
  @Input() title: string;
  _isCollapsed = false;
  _isCollapsible = true;
  _hasTitle = true;
  constructor() { }

  ngOnInit() { 
   
  }

  toggleCollapse(event: MouseEvent): void {
    if (!this._isCollapsible)
      return;
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
