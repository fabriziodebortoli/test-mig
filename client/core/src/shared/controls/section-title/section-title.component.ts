import { Component, OnInit, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-section-title',
  templateUrl: './section-title.component.html',
  styleUrls: ['./section-title.component.scss']
})
export class SectionTitleComponent extends ControlComponent {
  @Input() linePosition: boolean = true;
}
