import { Component, OnInit, Input } from '@angular/core';
import { Widget } from './widgets.service';

@Component({
  selector: 'tb-widget-stats',
  templateUrl: './widget-stats.component.html',
  styles: [`
    .widget-stats {
      margin-top:20px;
      display: flex;
      flex-direction: row;
    }
    .widget-stats-spacer {
      flex: 1 1 auto;
    }    
    .widget-stats-icon {
      font-size:36px;
      height :56px;
      width: 56px;
      line-height: 56px;      
    }
    .widget-stats-icon-box {
      border-radius: 3px;
      padding: 15px;
      text-align: center;
      color: #ffffff;
      height: 56px;
      width: 56px;
    }
    .widget-stats-box {
      padding-right: 15px;
      text-align: right;
      display: flex;
      flex-direction: column;
   }
   .widget-stats-title {
      font-weight: 300;
      font-size: 22px;   
      color: inherit; 
   }
   .widget-stats-subtitle {
      font-weight: 300;
      font-size: 16px;   
      color: color: rgba(0, 0, 0, 0.54);; 
   }
   .widget-stats-value {
      font-weight: 400;
      font-size: 35px;   
      color: inherit; 
   }

    :host .green {
      background: linear-gradient(60deg, #66bb6a, #43a047);
    }
    :host .purple {
      background: linear-gradient(60deg, #ab47bc, #8e24aa);
    }
    :host .blue {
      background: linear-gradient(60deg, #03A2FF, #0277BD);
    }
    :host .orange {
      background: linear-gradient(60deg, #ffa726, #fb8c00);
    }
    :host .red {
      background: linear-gradient(60deg, #ef5350, #e53935);
    }
    :host .cyan {
      background: linear-gradient(60deg, #26c6da, #00acc1);
    }

    :host .w-shadow{
      box-shadow: 0 3px 1px -2px rgba(0,0,0,.2), 0 2px 2px 0 rgba(0,0,0,.14), 0 1px 5px 0 rgba(0,0,0,.12);
    }
  `]
})
export class WidgetStatsComponent implements OnInit {
  @Input() widget: Widget;

  constructor() {
  }

  ngOnInit() {
  }

  formatMoney(value: number,
        currencySign: string = '€ ',
        decimalLength: number = 2,
        chunkDelimiter: string = '.',
        decimalDelimiter: string = ',',
        chunkLength: number = 3): string {

        const result = '\\d(?=(\\d{' + chunkLength + '})+' + (decimalLength > 0 ? '\\D' : '$') + ')';
        const num = value.toFixed(decimalLength);

        return  currencySign +
                (decimalDelimiter ? num.replace('.', decimalDelimiter) : num).replace(new RegExp(result, 'g'), '$&' + chunkDelimiter);
    }

  getValue(): string {
    if  (
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

  statsIconColorClass() {
    if (this.widget.layout.statsFormat && this.widget.layout.statsFormat.color) {
      return 'w-shadow ' + this.widget.layout.statsFormat.color;
    } else {
      return 'w-shadow blue';
    }
  }
}
