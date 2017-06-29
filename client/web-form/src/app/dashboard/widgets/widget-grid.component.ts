import { MenuService } from './../../menu/services/menu.service';
import { WidgetComponent } from './widget.component';
import { Component, Input, ViewEncapsulation, OnInit, AfterViewInit } from '@angular/core';
import { GridModule } from '@progress/kendo-angular-grid';
import { Widget } from './widgets.service';

@Component({
  selector: 'tb-widget-grid',
  templateUrl: './widget-grid.component.html',
  styleUrls: ['./widget-grid.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class WidgetGridComponent{
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
