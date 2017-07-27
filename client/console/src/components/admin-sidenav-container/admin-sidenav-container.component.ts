import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'admin-sidenav-container',
  templateUrl: './admin-sidenav-container.component.html',
  styleUrls: ['./admin-sidenav-container.component.css']
})
export class AdminSidenavContainerComponent implements OnInit {

    @Input() appTitle:string = '';

    constructor() { }

    ngOnInit() {
      console.log(this.appTitle);
    }

}
