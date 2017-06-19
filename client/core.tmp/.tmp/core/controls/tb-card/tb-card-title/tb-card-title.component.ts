import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-card-title',
  template: "<div> <span>{{title}}</span> <md-icon *ngIf=\"isCollapsible\">{{getArrowIcon()}}</md-icon> </div>",
  styles: [""]
})
export class TbCardTitleComponent implements OnInit {
  private _isCollapsible = true;
  private _isCollapsed: boolean = false;
  @Input('title') title: string;

  @Input()
  set isCollapsible(value: boolean) {
    this._isCollapsible = value;
  }

  get isCollapsible(): boolean {
    return this._isCollapsible;
  }
  @Input()
  set isCollapsed(value: boolean) {
    this._isCollapsed = value;
  }

  get isCollapsed(): boolean {
    return this._isCollapsed;
  }

  constructor() { }

  ngOnInit() {
  }

  getArrowIcon() {
    return this._isCollapsed ? 'keyboard_arrow_down' : 'keyboard_arrow_up';
  }
}
