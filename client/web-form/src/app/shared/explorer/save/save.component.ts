import { ExplorerService } from './../../../core/explorer.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-save',
  templateUrl: './save.component.html',
  styleUrls: ['./save.component.css']
})
export class SaveComponent implements OnInit {

  constructor( private explorerService:ExplorerService) { }

  ngOnInit() {
  }

}
