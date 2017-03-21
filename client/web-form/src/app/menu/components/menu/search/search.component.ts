import { LocalizationService } from './../../../services/localization.service';
import { MenuService } from './../../../services/menu.service';
import { Component, OnInit, ViewChild } from '@angular/core';
import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';

@Component({
  selector: 'tb-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent implements OnInit {
  @ViewChild('autocomplete') public autocomplete: AutoCompleteComponent;

  public selected: string = '';
  public selectedItem: any;
  data: any;
  constructor(
    private menuService: MenuService,
    private localizationService: LocalizationService
  ) {
    this.data = this.menuService.searchSources.slice();
  }

  ngOnInit() {
  }

  onSelect(val) {
    //commentato perchè autocomplete kendo non ritorna l'object selezionato, ma solo la stringa, e con solo il text (ad esempio customers)
    //non ho gli elementi per fare una runfunction sensata
    // this.menuService.runFunction(val);
     this.selected = undefined;
  }

  getSearchItems() {
    return this.menuService.searchSources;
  }

  handleFilter(value) {
    //if (value.length >= 3) {
      this.data = this.menuService.searchSources.filter((s) => s.title.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    //}
    //else {
     // this.autocomplete.toggle(false);
    //}
  }

}
