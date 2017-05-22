import { MenuService } from './../../menu/services/menu.service';
import { Component, Input, AfterContentInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { Widget, WidgetsService } from './widgets.service';

@Component({
  selector: 'tb-widget',
  templateUrl: './widget.component.html',
  styleUrls: ['./widget.component.scss']
})
export class WidgetComponent implements AfterViewInit {
  @Input() widget: Widget;
  @ViewChild('cardContent') cardContent: ElementRef;
  ContentHeight: number;
  ContentWidth: number;

  isLoading: boolean = false;

  constructor(private widgetsService: WidgetsService, private menuService: MenuService) {

  }

  ngAfterViewInit() {
    setTimeout(() => {
      this.ContentHeight = this.cardContent ? this.cardContent.nativeElement.offsetHeight : 0;
      this.ContentWidth = this.cardContent ? this.cardContent.nativeElement.offsetWidth : 0;
      console.log(this.ContentHeight, this.ContentWidth);
    }, 0);
  }

  onRefreshClicked() {
    this.isLoading = true;
    this.widgetsService.refreshContent(this.widget).subscribe(
      (data) => {
        this.widget.data = data;
        this.isLoading = false;
      },
      (err) => {
        // TODO report error
        this.isLoading = false;
      }
    );
  }

  executeLink() {
    let object = { target: this.widget.link, objectType: "Document" };
    this.menuService.runFunction(object);
  }

}
