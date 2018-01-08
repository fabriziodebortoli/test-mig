import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Widget, WidgetsService, WidgetRow } from './widgets.service';
import { MatSnackBar } from '@angular/material';

@Component({
  selector: 'tb-widget-container',
  templateUrl: './widget-container.component.html',
  styleUrls: ['./widget-container.component.scss']
})
export class WidgetContainerComponent implements OnInit, OnDestroy {
  rows: WidgetRow[] = [];
  subscriptions = [];
  constructor(public widgetsService: WidgetsService, public snackBar: MatSnackBar, private changeDetectorRef: ChangeDetectorRef) {
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
      (userPage) => {
        this.rows = [];
        userPage.forEach(pageRow => {
          this.rows.push({ cols: [] });
          if (pageRow.cols) {
            pageRow.cols.forEach(wdgInfo => {
              const emptyWdg = new Widget(wdgInfo.namespace);
              const row = this.rows.length - 1;
              const col = this.rows[row].cols.push(emptyWdg) - 1;
              this.subscriptions.push(this.widgetsService.getWidget(wdgInfo.namespace).subscribe(
                wdg => {
                  this.rows[row].cols[col] = wdg;
                  this.changeDetectorRef.detectChanges();
                }
              ));
            });
          }
        });
        if (this.widgetsService.isFirstUse) {
          this.snackBar.open('Your dashboard was empty, and has been created with a default layout.', 'Ok');
        }
      },
      (error) => {
        this.snackBar.open('There was an error retrieving your dashboard:' + error, 'Close');
      }
    ));
  }

  ngOnDestroy() {
    this.subscriptions.forEach(subs => subs.unsubscribe());
  }
}
