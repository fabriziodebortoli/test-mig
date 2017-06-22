import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-card-title',
  templateUrl: './tb-card-title.component.html',
  styleUrls: ['./tb-card-title.component.scss']
})
export class TbCardTitleComponent implements OnInit {
private _isCollapsible = true;
  private _isCollapsed: boolean = false;
 @Input() title: string;
 @Input() icon: string;
@Input() iconClass: string;
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
     if(this.icon !== ''){return this.icon;}
    else {return this._isCollapsed ? 'keyboard_arrow_down' : 'keyboard_arrow_up';}
  }
}
