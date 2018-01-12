import { Injectable } from '@angular/core';
import { URLSearchParams, Http, Response, Headers } from '@angular/http';
import { Observable } from '../../rxjs.imports';

import { InfoService } from './../../core/services/info.service';
import { DataService } from './../../core/services/data.service';

export class Widget {
  namespace: string;
  title: string;
  subtitle: string;
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
  public clock: Observable<Date>;

  constructor(public http: Http, public dataService: DataService, public infoService: InfoService) {
    this.clock = Observable.interval(1000).map(tick => new Date()).share();
  }

  public pad00(n): string {
    return ('0' + n).slice(-2);
  }
  public getExecutionTime() {
    const d = new Date();
    return this.pad00(d.getDate()) + '-' +
      this.pad00(d.getMonth() + 1) + '-' +
      this.pad00(d.getFullYear()) + ' ' +
      this.pad00(d.getHours()) + ':' +
      this.pad00(d.getMinutes()) + ':' +
      this.pad00(d.getSeconds());
  }

  getActiveWidgets(): Observable<WidgetRow[]> {
    const url: string = this.infoService.getBaseUrl() + '/widgets-service/getActiveWidgets';
    let headers = new Headers();
    headers.append("Authorization", this.infoService.getAuthorization());

    return this.http.get(url, { withCredentials: true, headers: headers }).map(
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
    const url: string = this.infoService.getBaseUrl() + '/widgets-service/getWidget/' + ns + '/';

    let headers = new Headers();
    headers.append("Authorization", this.infoService.getAuthorization());
    return this.http.get(url, { withCredentials: true, headers: headers }).map(
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

      let p: URLSearchParams = new URLSearchParams();
      p.set('page', "1"); // always paginated, default 10 rows if not otherwise stated 
      if (wdg.provider.maxRows && wdg.provider.maxRows > 0) {
        p.set('per_page', String(wdg.provider.maxRows));
      }
      let subs = this.dataService.getData(wdg.provider.namespace, wdg.provider.selection, p).subscribe((dsData: any) => {

        wdg.data.grid.rows = dsData.rows;
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
