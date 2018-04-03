import { Component, OnInit, ViewChild, ElementRef, Input, OnDestroy, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';

import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';

import { TbComponent, MenuService, SettingsService, TbComponentService } from '@taskbuilder/core';

@Component({
  selector: 'tb-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent extends TbComponent implements OnInit, OnDestroy {
  public selected: string = '';
  public inputControl: FormControl;
  public filteredElements: any;

  @ViewChild('myInput') myInput: ElementRef;

  valueChangesSubscription: any;
  constructor(
    public menuService: MenuService,
    public settingsService: SettingsService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();

    this.menuService.runFunctionStarted.subscribe(() => {
      this.selected = undefined;
      this.myInput.nativeElement.value = "";
      this.filteredElements = this.inputControl.valueChanges
        .startWith(null)
        .map(val => this.filter(val));
    });
    this.inputControl = new FormControl();
  }

  ngOnInit() {
    super.ngOnInit();
    this.filteredElements = this.inputControl.valueChanges
      .startWith(null)
      .map(val => this.filter(val));

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
  }

  filter(val: string): string[] {
    if (!val)
      return [];

    //fino ai 3 caratteri digitati, limito la ricerca a 20 (cablato)
    if (val.length >= 0 && val.length <= 3)
      return this.menuService.searchSources.filter(option =>
        new RegExp(val, 'gi').test(option.title)
      ).slice(0, 20);

    //dal 4 carattere in su, non limito, faccio vedere tutte le entries, sperando non siano millemila
    return this.menuService.searchSources.filter(option =>
      new RegExp(val, 'gi').test(option.title)
    ).slice(0, this.menuService.searchSources.length);
  }

  displayElement(element: any): string {
    return element ? element.title : '';
  }

  isObject(val) {
    return val instanceof Object;
  }

}
