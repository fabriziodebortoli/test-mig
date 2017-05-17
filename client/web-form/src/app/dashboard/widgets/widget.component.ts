import { Component, Input, AfterContentInit, ViewChild, ElementRef } from '@angular/core';
import { Widget, WidgetsService } from './widgets.service';

@Component({
  selector: 'tb-widget',
  templateUrl: './widget.component.html',
  styles: [`
    :host {
      height : 100%;
      width: 100%;
      box-sizing: border-box;
    }
    .widget {
      height : 100%;
      width: 100%;
      box-sizing: border-box;
      display: flex;
      flex-direction: column;
      padding: 13px 15px 0 15px;
    }
    .widget-header {
      margin-bottom: 12px;
    }
    .widget-title {
      margin-bottom: 10px;
      display: flex;
      flex-direction: row;
    }
    .mat-card-header .mat-card-subtitle:not(:first-child), .mat-card>.mat-card-xl-image:first-child {
      margin-top: -10px;
    }    
    .widget-content {
        flex: 1;
        height:100%;
        margin-bottom:45px;
    }   
    .widget-content-area {
        height:100%;
    }
    .widget-icon {
        cursor: pointer;
        color: rgba(0, 0, 0, 0.54);
    }   
    .widget-icon-menu {
      margin-left: -20px;
      font-size: 20px;
      margin-top: 3px;
      width: 20px;
    }
    .widget-spacer {
      flex: 1 1 auto;
    }    
    .widget-hr {
        margin-top:0;
        margin-bottom:5px;
        border-style: solid;
        border: 0;
        border-top: 1px solid #eee;
    }
    .widget-footer {
      padding: 10px 10px 5px 10px;
      box-sizing: border-box;
      }
    .widget-footer-elements {
      display: flex;
      flex-direction: row;
      }
    .widget-timestamp {
        font-family:Courier New, Courier, monospace;
        font-size:12px;
        padding-left:10px;
    }
    .widget-spinner {
      margin:auto;
      width:50px;
    }
    h4 {
      font-family: "Roboto", "Helvetica", "Arial", sans-serif;
      font-weight: 300;
      font-size: 22px;   
      color: inherit; 
      margin: 0;
    }
  `]
})
export class WidgetComponent implements AfterContentInit {
  @Input() widget: Widget;
  @ViewChild('cardContent') cardContent: ElementRef;
  ContentHeight: number;
  ContentWidth: number;

  isLoading: boolean = false;

  constructor(private widgetsService: WidgetsService) { }

  ngAfterContentInit() {
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

}
