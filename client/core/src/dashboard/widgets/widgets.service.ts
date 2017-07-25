import { Injectable } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { UrlService } from './../../core/services/url.service';
import { DataService } from './../../core/services/data.service';

export class Widget {
  namespace: string;
  title: string;
  link: string;
  linkedNamespace: string;
  recordKeys: string;
  layout: WidgetLayout;
  provider: WidgetProvider;
  data?: WidgetData;
  isLoading: boolean = true;

  constructor(ns: string = 'undefined') {
    this.namespace = ns;
    this.layout = new WidgetLayout(300, "void", null, null);
  }
}

export class WidgetRow {
  cols: Widget[];
}

export class WidgetLayout {
  height?: number = 300;
  style: string;
  statsFormat?: StatsFormat;
  gridFormat?: GridFormat;
  chartFormat?: ChartFormat;

  constructor(height: number = 300, style: string, statsFormat: StatsFormat, gridFormat: GridFormat) {
    this.height = height;
    this.style = style;
    this.statsFormat = statsFormat;
    this.gridFormat = gridFormat;
  }
}

export class WidgetData {
  lastExecuted: string;
  grid: WidgetDataGrid = new WidgetDataGrid;
}

export class WidgetDataGrid {
  columns: WidgetColumn[];
  rows: any[];
}

export class WidgetColumn {
  caption: string;
  id: string;
}

export class WidgetProvider {
  type: string;
  namespace: string;
  selection: string;
  maxRows: number;
  params: any;
}

export class StatsFormat {
  color: string;
  icon: string;
  value: string;
  format: string;
}

export class GridFormat {
  maxRows: number;
  columns?: WidgetColumn[];
  color?: string;
}

export class ChartFormat {
  type: string;
  categoryField: string;
  field: string;
}


@Injectable()
export class WidgetsService {

  public isFirstUse: boolean = false;
  private clock: Observable<Date>;
 
  constructor(private http: Http, private dataService: DataService, private urlService: UrlService) {
     this.clock = Observable.interval(1000).map(tick => new Date()).share();
  }

  private pad00(n): string {
    return ('0' + n).slice(-2);
  }
  private getExecutionTime() {
    const d = new Date();
    return this.pad00(d.getDate()) + '-' +
      this.pad00(d.getMonth() + 1) + '-' +
      this.pad00(d.getFullYear()) + ' ' +
      this.pad00(d.getHours()) + ':' +
      this.pad00(d.getMinutes()) + ':' +
      this.pad00(d.getSeconds());
  }

  getActiveWidgets(): Observable<WidgetRow[]> {
    const url: string = this.urlService.getBackendUrl() + '/widgets-service/getActiveWidgets';

    return this.http.get(url, { withCredentials: true }).map(
      (res: Response) => {

        this.isFirstUse = res.status === 203;
        return res.json();
      },
      (error) => {
        return error;
      }
    );
  }

  getWidget(ns: string): Observable<Widget> {
    const url: string = this.urlService.getBackendUrl() + '/widgets-service/getWidget/' + ns + '/';

    return this.http.get(url, { withCredentials: true }).map(
      (res: Response) => {
        return res.json();
      },
      (error) => {
        return error;
      }
    );
  }

  refreshContent(wdg: Widget) {

    try {
      wdg.isLoading = true;
      wdg.data = new WidgetData;

      if (wdg.provider == undefined || wdg.provider.type !== 'dataservice') {
        return;
      }

      let subs = this.dataService.getData(wdg.provider.namespace, wdg.provider.selection, wdg.provider.params).subscribe((dsData: any) => {

        wdg.data.grid.rows = wdg.provider.maxRows ? dsData.rows.slice(0, wdg.provider.maxRows) : dsData.rows;
        wdg.data.grid.columns = (wdg.layout.gridFormat && wdg.layout.gridFormat.columns) ? wdg.layout.gridFormat.columns : dsData.columns;
        //wdg.isLoading = false;
        subs.unsubscribe();
      });

    }
    finally {
      wdg.data.lastExecuted = this.getExecutionTime();
      wdg.isLoading = false;
    }
  }

  getClock(): Observable<Date> {
    return this.clock;
  }
}
