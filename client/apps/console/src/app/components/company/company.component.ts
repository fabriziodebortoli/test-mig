import { OperationResult } from './../../services/operationResult';
import { CompanyService } from './../../services/company.service';
import { Component, OnInit } from '@angular/core';
import { Company } from '../../model/company';
import { Observable } from 'rxjs/Rx';

@Component({
  selector: 'app-company',
  templateUrl: './company.component.html',
  styleUrls: ['./company.component.css']
})

export class CompanyComponent implements OnInit {

  model: Company;
  editing: boolean = false;
  useDMS: boolean = false;

  constructor(private companyService: CompanyService) {
    this.model = new Company();
  }

  ngOnInit() {
  }

  onUseDMSChange(event) {
    // test this.model.useDMS instead of this.useDMS
    this.useDMS = event.target.checked;
  }

  submitCompany() {
    if (this.model.name == undefined) {
      alert('Company name is mandatory!');
      return;
    }

    // da togliere quando valorizzeremo la Suscriptionid con quella corrente
    if (this.model.subscriptionId == undefined)
      this.model.subscriptionId = 1;

    let companyOperation: Observable<OperationResult>;
    if (!this.editing)
      companyOperation = this.companyService.addCompany(this.model);
    else {
      //companyOperation = this.companyService.updateCompany(this.model);   
    }

    let subs = companyOperation.subscribe(
      companyResult => 
      {
        this.model = new Company();
        alert(companyResult.Message);
        subs.unsubscribe();
      },
      err => 
      { 
        console.log(err); 
        alert(err); 
        subs.unsubscribe();
      }
    )
  }
}
