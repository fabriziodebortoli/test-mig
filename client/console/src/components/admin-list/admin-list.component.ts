import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'admin-list',
  templateUrl: './admin-list.component.html',
  styleUrls: ['./admin-list.component.css']
})
export class AdminListComponent implements OnInit {

    @Input() items: Array<object>;

    constructor() { }

    ngOnInit() {
    }

}
