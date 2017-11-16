import { Component, OnInit, Input } from '@angular/core';
import { ViewEncapsulation } from '../../../../../web-form/node_modules/@angular/core/src/metadata/view';

@Component({
  selector: 'tb-header-strip',
  templateUrl: './header-strip.component.html',
  styleUrls: ['./header-strip.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class HeaderStripComponent implements OnInit {

  @Input('title') title: string = '...';

  constructor() { }

  ngOnInit() {
  }

}
