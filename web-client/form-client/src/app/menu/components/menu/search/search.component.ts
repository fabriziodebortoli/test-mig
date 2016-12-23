import { LocalizationService } from './../../../services/localization.service';
import { MenuService } from './../../../services/menu.service';
import { Component, OnInit } from '@angular/core';


@Component({
  selector: 'tb-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent implements OnInit {
  public selected:string = '';
    constructor(
    private menuService: MenuService,
    private localizationService: LocalizationService
  ) {
  }

  ngOnInit() {
  }

  onSelect(selected) {
    this.menuService.runFunction(selected);
    this.selected = undefined;
  }

  getSearchItems(){

    return this.menuService.searchSources; 
  }
  
}
