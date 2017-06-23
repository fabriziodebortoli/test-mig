import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-card-subtitle',
  templateUrl: './tb-card-subtitle.component.html',
  styleUrls: ['./tb-card-subtitle.component.scss']
})
export class TbCardSubtitleComponent implements OnInit {
@Input() subTitle: string;
  constructor() { }

  ngOnInit() {
  }

}
