import { DataService } from '@taskbuilder/core';
import { environment } from 'environments/environment';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { URLSearchParams, Http, Response } from '@angular/http';

export class Widget {
  title: string;
  link: string;
  linkedNamespace: string;
  recordKeys: string;
  layout: WidgetLayout;
  provider: WidgetProvider;
  data?: WidgetData;
}

export class WidgetRow {
  widgets: Widget[];
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

  constructor(private http: Http, private dataService: DataService) {
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
    const url: string = environment.baseUrl + 'widgets-service/getActiveWidgets';

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

  refreshContent(wdg: Widget): Observable<WidgetData> {
    const data = new WidgetData;
    data.lastExecuted = this.getExecutionTime();
    if (wdg.provider && wdg.provider.type === 'dataservice') {
      let subs = this.dataService.getData(wdg.provider.namespace, wdg.provider.selection, wdg.provider.params).subscribe((dsData: any) => {
        if (wdg.provider.maxRows) {
          data.grid.rows = dsData.rows.slice(0, wdg.provider.maxRows);
        } else {
          data.grid.rows = dsData.rows;
        }
        if (wdg.layout.gridFormat && wdg.layout.gridFormat.columns) {
          data.grid.columns = wdg.layout.gridFormat.columns;
        } else {
          data.grid.columns = dsData.columns;
        }

        subs.unsubscribe();
      });
    }
    return Observable.of(data);
  }

}
