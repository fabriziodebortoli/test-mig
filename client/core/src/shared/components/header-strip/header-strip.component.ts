import { Component, OnInit, Input, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'tb-header-strip',
  templateUrl: './header-strip.component.html',
  styleUrls: ['./header-strip.component.scss']
})
export class HeaderStripComponent implements OnInit {

  @Input('title') title: string = '...';

  constructor() { }

  ngOnInit() {
  }

}
