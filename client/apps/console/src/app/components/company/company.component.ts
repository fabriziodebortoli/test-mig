import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-company',
  templateUrl: './company.component.html',
  styleUrls: ['./company.component.css']
})

export class CompanyComponent implements OnInit {

  useDMS = false;

  constructor() { }

  ngOnInit() {
  }

  onUseDMSChange(event) {
    this.useDMS = event.target.checked;
  }
}
