import { Component, OnInit, OnDestroy } from '@angular/core';
import { Widget, WidgetsService, WidgetRow } from './widgets.service';
import { MdSnackBar } from '@angular/material';

@Component({
  selector: 'tb-widget-container',
  templateUrl: './widget-container.component.html',
  styleUrls: ['./widget-container.component.scss']
})
export class WidgetContainerComponent implements OnInit, OnDestroy {
  widgets: WidgetRow[] = [];
  subscriptions = [];
  constructor(private widgetsService: WidgetsService, public snackBar: MdSnackBar) {



   }

  getColspan(size: string) {
    switch (size) {
      case 'small': return 'col-xs-12 col-sm-6 col-md-4 col-lg-3';
      case 'medium': return 'col-xs-12 col-sm-12 col-md-6 col-lg-4';
      case 'large': return 'col-xs-12 col-sm-12 col-md-6 col-lg-6';
      default: return 'col-xs-12 col-sm-12 col-md-6 col-lg-6';
    }
  }

  ngOnInit() {
    this.subscriptions.push(this.widgetsService.getActiveWidgets().subscribe(
      (w) => {
        this.widgets = w;

        this.widgets.forEach((row) => {
          row.widgets.forEach((wdg) => {
            this.subscriptions.push(this.widgetsService.refreshContent(wdg).subscribe(
              (data) => {
                wdg.data = data;
              }
            ));
          });
        });

        if (this.widgetsService.isFirstUse) {
          this.snackBar.open('Your dashboard was empty, and has been created with a default layout.', 'Ok');
        }
      },
      (error) => {
        this.snackBar.open('There was an error retreiving your dashboard:' + error, 'Close');
      }
    ));
  }

  ngOnDestroy() {
    this.subscriptions.forEach(subs => subs.unsubscribe());
  }

}
