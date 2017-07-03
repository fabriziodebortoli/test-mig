import { ControlComponent } from './../control.component';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-section-title',
  templateUrl: './section-title.component.html',
  styleUrls: ['./section-title.component.scss']
})
export class SectionTitleComponent extends ControlComponent implements OnInit   {

  ngOnInit() {
  }

}
