import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-grid',
  templateUrl: './grid.component.html',
  styleUrls: ['./grid.component.scss']
})
export class GridComponent implements OnInit {

  @Input('gridData') gridData: any[];

  constructor() { }

  ngOnInit() {
  }

}
