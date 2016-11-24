import { ReportStudioService, Message, CommandType } from './../report-studio.service';
import { Component, OnInit } from '@angular/core';

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

  test() {
    let message: 'TOOLBAR';
    this.reportService.sendTestMessage(message);
  }

}
