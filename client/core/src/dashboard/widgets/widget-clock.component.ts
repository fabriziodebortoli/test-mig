import { WidgetsService } from './widgets.service';
import {Component} from "@angular/core";

@Component({
  selector: 'tb-clock',
  templateUrl: './widget-clock.component.html',
  styleUrls: ['./widget-clock.component.scss']
})
export class WidgetClockComponent {

  time: Date;

  constructor(private widgetsService: WidgetsService) {
  }

  ngOnInit() {
    this.widgetsService.getClock().subscribe(time => this.time = time);
  }

}