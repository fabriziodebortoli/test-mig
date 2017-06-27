import { Component, OnInit, Input } from '@angular/core';

import { TextComponent } from './../text/text.component';

@Component({
  selector: 'tb-label-static',
  template: "<div> <tb-text [model]=\"model\" [width]=\"width\"></tb-text> </div>",
  styles: [""]
})
export class LabelStaticComponent extends TextComponent implements OnInit {

  @Input() caption = '';
  @Input() width: number;

  ngOnInit() { }

}
