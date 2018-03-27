import { Component, Input, AfterContentInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';

import { MenuService } from '@taskbuilder/core';

import { Widget, WidgetsService } from './widgets.service';

@Component({
  selector: 'tb-widget',
  templateUrl: './widget.component.html',
  styleUrls: ['./widget.component.scss']
})
export class WidgetComponent implements AfterContentInit {
  @Input() widget: Widget;
  @ViewChild('cardContent') cardContent: ElementRef;
  ContentHeight: number;
  ContentWidth: number;
  subscriptions = [];

  constructor(public widgetsService: WidgetsService, public menuService: MenuService) {
  }

  ngAfterContentInit() {
    this.onRefreshClicked();
    setTimeout(() => {
      this.ContentHeight = this.cardContent ? this.cardContent.nativeElement.offsetHeight : 0;
      this.ContentWidth = this.cardContent ? this.cardContent.nativeElement.offsetWidth : 0;
    }, 0);
  }

  onRefreshClicked() {
    this.widgetsService.refreshContent(this.widget);
  }

  executeLink() {
    let object = { target: this.widget.link, objectType: "Report" };
    this.menuService.runFunction(object);
  }
}