import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-topbar-menu-test',
  templateUrl: './topbar-menu-test.component.html',
  styleUrls: ['./topbar-menu-test.component.css']
})
export class TopbarMenuTestComponent implements OnInit {

  private title: string = "Test menu";

  constructor() { }

  ngOnInit() {
  }

}
