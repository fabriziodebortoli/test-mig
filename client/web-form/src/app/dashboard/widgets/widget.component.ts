import { MenuService } from './../../menu/services/menu.service';
import { Component, Input, AfterContentInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
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
  isLoading: boolean = false;

  constructor(private widgetsService: WidgetsService, private menuService: MenuService) {
   this.isLoading = true; 
  }

  ngAfterContentInit() {
    this.onRefreshClicked();
     setTimeout(() => {
      this.ContentHeight = this.cardContent ? this.cardContent.nativeElement.offsetHeight : 0;
      this.ContentWidth = this.cardContent ? this.cardContent.nativeElement.offsetWidth : 0;
    }, 0);
  }

  onRefreshClicked() {
    this.isLoading = true;
    let thiz = this;
    let subs = this.widgetsService.refreshContent(this.widget).subscribe(
      (data) => {
        this.widget.data = data;
        setTimeout(function () {
          thiz.isLoading = false;
        }, 1);
        if (subs)
          subs.unsubscribe();
      },
      (err) => {
        // TODO report error
        thiz.isLoading = false;
        if (subs)
          subs.unsubscribe();
      }
    );
  }

  executeLink() {
    let object = { target: this.widget.link, objectType: "Report" };
    this.menuService.runFunction(object);
  }
}