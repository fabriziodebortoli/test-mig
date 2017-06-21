import { Company } from './../../model/company';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-database-info',
  templateUrl: './database-info.component.html',
  styleUrls: ['./database-info.component.css']
})

export class DatabaseInfoComponent implements OnInit {

  @Input() companyModel: Company;
  @Input() isDMS: boolean;

  constructor() { }

  ngOnInit() {
  }
}
