import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'admin-toolbar',
  templateUrl: './admin-toolbar.component.html',
  styleUrls: ['./admin-toolbar.component.css']
})
export class AdminToolbarComponent implements OnInit {

    title:string = 'M4 Cloud Provisioning';

    constructor() { }

    ngOnInit() {
    }

}
