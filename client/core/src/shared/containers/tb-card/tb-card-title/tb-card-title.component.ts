import { Component, OnInit, Input, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'tb-card-title',
  templateUrl: './tb-card-title.component.html',
  styleUrls: ['./tb-card-title.component.scss'],
})
export class TbCardTitleComponent implements OnInit {
  
  public _isCollapsible = true;
  public _isCollapsed: boolean = false;
  
  @Input() title: string;
  @Input() iconClass: string;
  @Input() _icon: string = '';

    @Input()
    set icon(icon: any) {
        this._icon = icon instanceof Object ? icon.value : icon;
    }

    get icon() {
        return this._icon;
    }

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

  getIcon() {
    if (this.icon !== '') { return this.icon; }
    else { return this._isCollapsed ? 'tb-expandarrowfilled' : 'tb-collapsearrowfilled'; }
  }
}
