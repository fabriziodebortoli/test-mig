import { WidgetComponent } from './widget.component';
import { Component, OnInit, Input, AfterViewInit, AfterContentInit } from '@angular/core';
import { Widget, WidgetsService } from './widgets.service';

@Component({
  selector: 'tb-widget-stats',
  templateUrl: './widget-stats.component.html',
  styleUrls: ['./widget-stats.component.scss']
})
export class WidgetStatsComponent implements AfterContentInit {
  @Input() widget: Widget;

  statClass: string;
  constructor(public widgetComponent: WidgetComponent, public widgetsService: WidgetsService) { }

  ngAfterContentInit() {
    this.statClass = this.statsIconColorClass();
  }

  formatMoney(
    value: number,
    currencySign: string = '€ ',
    decimalLength: number = 2,
    chunkDelimiter: string = '.',
    decimalDelimiter: string = ',',
    chunkLength: number = 3): string {

    const result = '\\d(?=(\\d{' + chunkLength + '})+' + (decimalLength > 0 ? '\\D' : '$') + ')';
    const num = value.toFixed(decimalLength);

    return currencySign +
      (decimalDelimiter ? num.replace('.', decimalDelimiter) : num).replace(new RegExp(result, 'g'), '$&' + chunkDelimiter);
  }

  getValue(): string {
    if (this.widget.data) {
      if (
        !this.widget.data.grid.rows ||
        this.widget.data.grid.rows.length === 0 ||
        !this.widget.data.grid.rows[0][this.widget.layout.statsFormat.value]
      ) {
        return 'N/A';
      }
      const val = this.widget.data.grid.rows[0][this.widget.layout.statsFormat.value];
      if (this.widget.layout.statsFormat.format && this.widget.layout.statsFormat.format === 'money') {
        return this.formatMoney(parseFloat(val));
      } else {
        return val;
      }
    }

  }

  statsIconColorClass() {
    if (this.widget.layout.statsFormat && this.widget.layout.statsFormat.color) {
      return 'w-shadow ' + this.widget.layout.statsFormat.color;
    } else {
      return 'w-shadow blue';
    }
  }
}
