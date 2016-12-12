import { TbComponent } from '..';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-tile',
  templateUrl: './tile.component.html',
  styleUrls: ['./tile.component.css']
})
export class TileComponent  extends TbComponent implements OnInit {

  ngOnInit() {
  }

}
