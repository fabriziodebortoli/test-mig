import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-header-strip',
  template: "<div class=\"header-strip\"> <div class=\"above\"> <h1 *ngIf=\"title\">{{title}}</h1> <div class=\"content\"> <ng-content select=\".header-strip-content\"></ng-content> </div> </div> <span class=\"line\"></span> <div class=\"under\"> <ng-content select=\".header-strip-under\"></ng-content> </div> </div>",
  styles: [".header-strip { margin: 15px .5rem; } .header-strip h1 { text-align: left; padding: 0 3px; font-size: 26px; margin: 0; } .header-strip .line { height: 1px; background: #ccc; display: block; background: #ccc; background: linear-gradient(to right, #0277bd 0%, #EEF1F5 100%); } .header-strip .above { display: flex; flex-direction: row; justify-content: space-between; align-items: center; } .header-strip .upper { display: flex; flex-direction: row; } "]
})
export class HeaderStripComponent implements OnInit {

  @Input('title') title: string = '...';

  constructor() { }

  ngOnInit() {
  }

}