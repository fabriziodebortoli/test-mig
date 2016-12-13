import { TileManagerComponent } from './tilemanager.component';
import { TabComponent } from './tab.component';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-tilegroup',
  templateUrl: './tilegroup.component.html',
  styleUrls: ['./tilegroup.component.css']
})
export class TileGroupComponent extends TabComponent implements OnInit {

  constructor(tabs: TileManagerComponent) {
    super(tabs);
  }
  ngOnInit() {
  }

}
