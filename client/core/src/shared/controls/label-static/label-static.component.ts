import { Component, OnInit, Input } from '@angular/core';

import { TextComponent } from './../text/text.component';
import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-label-static',
  templateUrl: './label-static.component.html',
  styleUrls: ['./label-static.component.scss']
})
export class LabelStaticComponent extends TextComponent implements OnInit {
  @Input() caption = '';
  @Input() width: number;
  ngOnInit() {
  }

}
