import { DocumentComponent } from './../../../../core/components/document.component';
import { ComponentService } from './../../../../core/services/component.service';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';

@Component({
  selector: 'tb-languages',
  templateUrl: './languages.component.html',
  styleUrls: ['./languages.component.css']
})
export class LanguagesComponent extends DocumentComponent implements OnInit {

  constructor() { 
    super();
  }

  ngOnInit() {
  }

}


@Component({
  template: ''
})
export class LanguagesFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
    componentService.createComponent(LanguagesComponent, resolver);
  }
} 
