import { MenuService } from './../../menu/services/menu.service';
import { WidgetComponent } from './widget.component';
import { Component, Input, ViewEncapsulation } from '@angular/core';
import { GridModule } from '@progress/kendo-angular-grid';
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

  constructor(private widgetComponent: WidgetComponent, private menuService: MenuService) {

  }

  onClick(event: any, rowIndex: any, item: any) {

    if (this.widget.linkedNamespace == undefined || this.widget.recordKeys == undefined)
      return;

    let args: string = "";
    let keys: string[] = this.widget.recordKeys.split(',');

    for (let i = 0; i < keys.length; i++) {
      let vals: string[] = keys[i].split(':');
      args += vals[0] + ":" + item[vals[1]] + ";"
    }

    let object = { target: this.widget.linkedNamespace, objectType: "Document", args: args };
    this.menuService.runFunction(object);
  }

  gridHeight(): number {
    let h = this.widgetComponent.ContentHeight ? this.widgetComponent.ContentHeight : 120;// 31 = altezza header @@TODO rendere dinamico!
    console.log(h)
    return h;
  }

  gridHeaderColorClass() {
    if (this.widget.layout.gridFormat && this.widget.layout.gridFormat.color) {
      return this.widget.layout.gridFormat.color;
    } else {
      return 'blue';
    }
  }
}
