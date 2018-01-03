import { Component, OnInit, Input, ViewEncapsulation} from '@angular/core';

@Component({
  selector: 'tb-tile',
  templateUrl: './tile.component.html',
  styleUrls: ['./tile.component.scss']
})
export class TileComponent implements OnInit {
  @Input() title: string;
  public _isCollapsed: boolean = false;
  public _isCollapsible: boolean = true;
  
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
}
