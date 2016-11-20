import { ComponentService } from './../../../core/';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})
export class MenuComponent implements OnInit {

  constructor(private componentService: ComponentService) { }

  ngOnInit() {
  }

  createComponent(url: string) {
    this.componentService.createComponentFromUrl(url);
  }
}
