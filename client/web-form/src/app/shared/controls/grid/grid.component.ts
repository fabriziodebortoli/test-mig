import { GridParams } from './grid-params.model';
import { DataService } from './../../../core/data.service';
import { Subscription } from 'rxjs';
import { Component, OnInit, Input } from '@angular/core';



@Component({
  selector: 'tb-grid',
  templateUrl: './grid.component.html',
  styleUrls: ['./grid.component.scss']
})
export class GridComponent implements OnInit {

  private gridData: any[];

  @Input('gridParams') gridParams: GridParams;

  constructor(private dataService: DataService) { }

  ngOnInit() {
    let subs = this.dataService.getData(this.gridParams.nameSpace, this.gridParams.selectionType, this.gridParams.params).subscribe(data => {
      this.gridData = data;
      subs.unsubscribe();
    });
  }

}
