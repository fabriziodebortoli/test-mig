import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-topbar-search',
  templateUrl: './topbar-search.component.html',
  styleUrls: ['./topbar-search.component.css']
})
export class TopbarSearchComponent implements OnInit {

  private searchText: string = "Search"; //TODO localizzazione

  constructor() { }

  ngOnInit() {
  }

}
