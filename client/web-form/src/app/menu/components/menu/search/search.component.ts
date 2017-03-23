import { Observable } from 'rxjs/Rx';
import { LocalizationService } from './../../../services/localization.service';
import { MenuService } from './../../../services/menu.service';
import { Component, OnInit, ViewChild, ElementRef, Input } from '@angular/core';
import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'tb-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent implements OnInit {
  public selected: string = '';
  inputControl: FormControl;
  filteredElements: any;

  @Input() maxElements: number = 20;
  @ViewChild('myInput') myInput:ElementRef;

  constructor(
    private menuService: MenuService,
    private localizationService: LocalizationService
  ) {
    this.inputControl = new FormControl();
    this.filteredElements = this.inputControl.valueChanges
      .startWith(null)
      .map(name => this.filteredElements(name));
  }

  ngOnInit() {
    this.filteredElements = this.inputControl.valueChanges
      .startWith(null)
      .map(val => val ? this.filter(val) : this.menuService.searchSources.slice(0, this.maxElements));

    this.inputControl.valueChanges.subscribe(data => {
      if (this.isObject(data))
        this.onSelect(data);
    });

  }

  onSelect(val) {
    //commentato perchè autocomplete kendo non ritorna l'object selezionato, ma solo la stringa, e con solo il text (ad esempio customers)
    //non ho gli elementi per fare una runfunction sensata
    this.menuService.runFunction(val);
    this.selected = undefined;
    this.myInput.nativeElement.value = "";
  }
  
  filter(val: string): string[] {
    return this.menuService.searchSources.filter(option => new RegExp(val, 'gi').test(option.title)).slice(0, this.maxElements);
  }

  displayElement(element: any): string {
    return element ? element.title : '';
  }

  isObject(val) {
    return val instanceof Object;
  }

}
