import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-tilegroup',
  template: "<div class=\"tile-group\" *ngIf=\"active\"> <ng-content></ng-content> </div>",
  styles: [".tile-group { flex-direction: row; flex-wrap: wrap; justify-content: flex-start; padding-bottom: 100px; } "]
})
export class TileGroupComponent {

  active: boolean;

  @Input() title: string;

  @Input() iconType: string = 'M4';
  @Input() icon: string = 'erp-purchaseorder';

  constructor() { }

  ngOnInit() { }
}