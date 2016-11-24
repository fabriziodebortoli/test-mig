import { ReportStudioService, Message, CommandType } from './../report-studio.service';
import { Component, OnInit } from '@angular/core';

import { reportTest } from '../test/report-test';

@Component({
  selector: 'rs-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.css']
})
export class ToolbarComponent implements OnInit {

  private running: boolean = false;

  constructor(private reportService: ReportStudioService) { }

  ngOnInit() {
  }

  play() {
    this.running = true;
  }

  stop() {
    this.running = false;
  }

  pause() {
    this.running = false;
  }

  prev() {

  }

  next() {

  }

  testSTRUCT() {
    let message: Message = {
      commandType: CommandType.STRUCT,
      message: JSON.stringify(reportTest)
    }
    this.reportService.send(message);
  }

  testDATA() {
    let message: Message = {
      commandType: CommandType.DATA,
      message: ''
    }
    this.reportService.send(message);
  }

}
