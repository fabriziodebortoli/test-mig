import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'admin-sidenav',
  templateUrl: './admin-sidenav.component.html',
  styleUrls: ['./admin-sidenav.component.css']
})
export class AdminSidenavComponent implements OnInit {

    title:string = 'M4 Cloud Provisioning';

    constructor() { }

    ngOnInit() {
    }

}
