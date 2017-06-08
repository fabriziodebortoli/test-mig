import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'admin-sidenav-container',
  templateUrl: './admin-sidenav-container.component.html',
  styleUrls: ['./admin-sidenav-container.component.css']
})
export class AdminSidenavContainerComponent implements OnInit {

    title:string = 'M4 Cloud Provisioning';

    constructor() { }

    ngOnInit() {
    }

}
