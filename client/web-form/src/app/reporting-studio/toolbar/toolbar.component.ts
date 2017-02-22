import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'rs-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss']
})
export class ToolbarComponent implements OnInit {

  private running: boolean = false;

  constructor() { }

  ngOnInit() {
  }

  play() {
    this.running = true;
    // this.reportService.sendRun();
  }

  stop() {
    this.running = false;
    // this.reportService.sendStop();
  }

  pause() {
    this.running = false;
    // this.reportService.sendPause();
  }

  prev() {

  }

  next() {

  }
}
