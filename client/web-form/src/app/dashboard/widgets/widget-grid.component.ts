import { WidgetComponent } from './widget.component';
import { Component, Input, ViewEncapsulation } from '@angular/core';

import { Widget } from './widgets.service';

@Component({
  selector: 'tb-widget-grid',
  templateUrl: './widget-grid.component.html',
  encapsulation: ViewEncapsulation.None,
  styles: [`
    .widget-grid {
      font-size: 13px;
      border: 0;
    }
    .widget-grid th {
      padding: 1px 5px 1px 5px;
      border: 0;
      background-color: #ffffff;
    }

    .green th {
      color: #43a047;
    }
    .purple th {
      color: #8e24aa;
    }
    .blue th {
      color: #0277BD;
    }
    .orange th {
      color: #fb8c00;
    }
    .red th {
      color: #e53935;
    }
    .cyan th {
      color: #00acc1;
    }

    .widget-grid .k-grid-header {
      border-bottom-width: 0;
      background-color: inherit;
    }
    .widget-grid .k-grid-header-wrap {
      border-width: 0;
    }
    .widget-grid td {
      padding: 5px;
      border-width: 1px 0 0 0;
      white-space: nowrap;
    }
    .widget-grid .grid-cell-right {
      text-align: right;
    }
    .widget-grid tr.k-alt {
      background-color: inherit;
    }
  `]
})
export class WidgetGridComponent {
  @Input() widget: Widget;

  constructor(private widgetComponent: WidgetComponent) { }

  gridHeight(): number {
    return this.widgetComponent.ContentHeight - 31; // 31 = altezza header @@TODO rendere dinamico!
  }

  gridHeaderColorClass() {
    if (this.widget.layout.gridFormat && this.widget.layout.gridFormat.color) {
      return this.widget.layout.gridFormat.color;
    } else {
      return 'blue';
    }
  }
}
