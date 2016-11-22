import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';

@Component({
  selector: 'rs-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.css']
})
export class ToolbarComponent implements OnInit {

  @ViewChild('toolbar') toolbar: ElementRef;

  private running: boolean = false;

  constructor() { }

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

  getHeight() {
    return this.toolbar.nativeElement.offsetHeight;
  }

}
