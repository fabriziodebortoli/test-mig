import { Component, OnInit, Input, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'tb-card',
  templateUrl: './tb-card.component.html',
  styleUrls: ['./tb-card.component.scss']
})
export class TbCardComponent {
  @Input() isCollapsed: boolean;
  @Input() isCollapsible: boolean;
}
