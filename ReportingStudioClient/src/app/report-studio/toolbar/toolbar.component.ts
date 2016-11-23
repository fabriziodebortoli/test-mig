import { ReportStudioService, Message, Command } from './../report-studio.service';
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
    let message: Message = {
      command: Command.TEST,
      message: 'toolbar'
    }
    this.reportService.sendTestMessage('from toolbar');
  }

}
