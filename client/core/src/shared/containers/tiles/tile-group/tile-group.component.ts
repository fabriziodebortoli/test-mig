import { Component, Input, TemplateRef, ContentChild } from '@angular/core';

@Component({
  selector: 'tb-tilegroup',
  templateUrl: './tile-group.component.html',
  styleUrls: ['./tile-group.component.scss']
})
export class TileGroupComponent {
  @ContentChild(TemplateRef) templateRef: any;
  @Input() active: boolean;
  @Input() iconType: string = 'M4';
  @Input() icon: string = 'erp-purchaseorder';
  private _title: string;

  public get title(): string {
    return this._title;
  }

  @Input() public set title(value: string) {
    this._title = value.replace("&", "");
  }
  constructor() { }
}
