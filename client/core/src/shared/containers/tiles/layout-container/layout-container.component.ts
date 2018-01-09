import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-layoutcontainer',
  templateUrl: './layout-container.component.html',
  styleUrls: ['./layout-container.component.scss']
})
export class LayoutContainerComponent {

  _isCollapsed: boolean = false;
  _isCollapsible: boolean = true;

  constructor() { }


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
}