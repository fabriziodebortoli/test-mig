import { LocalizationService } from '@taskbuilder/core';
import { Component, OnInit, ViewChild, ElementRef, Input, OnDestroy, ViewEncapsulation } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs/Rx';

import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';

import { SettingsService } from './../../../services/settings.service';
import { MenuService } from './../../../services/menu.service';

@Component({
  selector: 'tb-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class SearchComponent implements OnInit, OnDestroy {
  public selected: string = '';
  public inputControl: FormControl;
  public filteredElements: any;

  @ViewChild('myInput') myInput: ElementRef;

  valueChangesSubscription: any;
  constructor(
    public menuService: MenuService,
    public settingsService: SettingsService,
    public localizationService: LocalizationService

  ) {
    this.inputControl = new FormControl();
    this.filteredElements = this.inputControl.valueChanges
      .startWith(null)
      .map(name => this.filteredElements(name));
  }

  ngOnInit() {
    this.filteredElements = this.inputControl.valueChanges
      .startWith(null)
      .map(val => val ? this.filter(val) : this.menuService.searchSources.slice(0, (val && val.length > 0) ? this.settingsService.nrMaxItemsSearch : 0));

    this.valueChangesSubscription = this.inputControl.valueChanges.subscribe(data => {
      if (this.isObject(data))
        this.onSelect(data);
    });

  }

  ngOnDestroy() {
    this.valueChangesSubscription.unsubscribe();
  }

  onSelect(val) {
    //commentato perchè autocomplete kendo non ritorna l'object selezionato, ma solo la stringa, e con solo il text (ad esempio customers)
    //non ho gli elementi per fare una runfunction sensata
    this.menuService.runFunction(val);
    this.selected = undefined;
    this.myInput.nativeElement.value = "";
  }

  filter(val: string): string[] {
    return this.menuService.searchSources.filter(option => new RegExp(val, 'gi').test(option.title)).slice(0, (val && val.length > 0) ? this.settingsService.nrMaxItemsSearch : 0);
  }

  displayElement(element: any): string {
    return element ? element.title : '';
  }

  isObject(val) {
    return val instanceof Object;
  }

}
