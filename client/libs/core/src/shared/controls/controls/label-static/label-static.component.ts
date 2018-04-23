import { Component, OnInit, Input, ChangeDetectorRef } from '@angular/core';

import { TextComponent } from './../text/text.component';

@Component({
  selector: 'tb-label-static',
  templateUrl: '../text/text.component.html'
})
export class LabelStaticComponent extends TextComponent {
  readonly = true;
  multiline = true;
}