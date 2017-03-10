import { DataService } from './../../core/data.service';
import { Subscription } from 'rxjs/Subscription';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-kendo-page',
  templateUrl: './kendo-page.component.html',
  styleUrls: ['./kendo-page.component.scss']
})
export class KendoPageComponent implements OnInit {

  gridNamespace = 'Erp.Items.dbl.DS_ItemsSimple';
  gridSelectionType = 'Code';
  gridParams;

  private dataSubscription: Subscription;
  private gridColumns: string[];
  private gridData: any[];

  constructor(private dataService: DataService) { }

  ngOnInit() {
    this.dataSubscription = this.dataService.getData(this.gridNamespace, this.gridSelectionType, this.gridParams).subscribe(data => {
      this.gridColumns = data.titles;
      this.gridData = data.rows;
    });
  }

  ngOnDestroy() {
    this.dataSubscription.unsubscribe();
  }

}
