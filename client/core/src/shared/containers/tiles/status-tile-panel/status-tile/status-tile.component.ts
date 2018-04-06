import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-status-tile',
  templateUrl: './status-tile.component.html',
  styleUrls: ['./status-tile.component.scss']
})
export class StatusTileComponent {


  active: boolean = true;

  @Input() title: string;
  @Input() _clickable: any;
  @Input() _visible: any;
  @Input() _backgroundColor: any;


  @Input()
  set clickable(clickable: any) {
    this._clickable = clickable instanceof Object ? clickable.value : clickable;
  }
  get clickable() {
    return this._clickable;
  }

  @Input()
  set visible(visible: any) {
    this._visible = visible instanceof Object ? visible.value : visible;
  }
  get visible() {
    return this._visible;
  }

  @Input()
  set backgroundColor(backgroundColor: any) {
    this._backgroundColor = backgroundColor instanceof Object ? backgroundColor.value : backgroundColor;
  }
  get backgroundColor() {
    return this._backgroundColor;
  }

  constructor() { }

  ngOnInit() { }
}