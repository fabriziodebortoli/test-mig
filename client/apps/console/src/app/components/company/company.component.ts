import { ModelService } from './../../services/model.service';
import { OperationResult } from './../../services/operationResult';
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

  constructor(private modelService: ModelService) {
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

    // da togliere quando valorizzeremo la SubscriptionKey con quella corrente
    if (this.model.subscriptionKey == undefined)
      this.model.subscriptionKey = '1';

    let companyOperation: Observable<OperationResult>;
    if (!this.editing)
      companyOperation = this.modelService.addCompany(this.model);
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
