import { Component, OnInit, Directive } from '@angular/core';
import { ToolbarButtonComponent } from './toolbarbutton.component' 

@Component({
  selector: 'tb-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.css']
})

export class ToolbarComponent implements OnInit {

  buttons: ToolbarButtonComponent[] = [];

  constructor() {

  }

  ngOnInit() {
  }

  toggleSidenav() {
   //this.sidenavService.toggleSidenav();
  }

}
