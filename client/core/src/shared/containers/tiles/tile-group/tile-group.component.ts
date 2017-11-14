import { Component, Input, TemplateRef, ContentChild } from '@angular/core';

@Component({
  selector: 'tb-tilegroup',
  templateUrl: './tile-group.component.html',
  styleUrls: ['./tile-group.component.scss']
})
export class TileGroupComponent {
  @ContentChild(TemplateRef) templateRef: any;
  @Input() active: boolean;
  @Input() title: string;
  @Input() iconType: string = 'M4';
  @Input() icon: string = 'erp-purchaseorder';

  constructor() { }
}
