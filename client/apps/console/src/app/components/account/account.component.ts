import { ModelService } from './../../services/model.service';
import { OperationResult } from './../../services/operationResult';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { NgForm }    from '@angular/forms';
import { Account } from './../../model/account';
import { AuthorizationProperties } from "app/authentication/auth-info";

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.css']
})

export class AccountComponent implements OnInit {

  model:Account;
  editing:boolean = false;

  constructor(private modelService: ModelService) { 
    this.model = new Account();
  }

  ngOnInit() {
  }

  submitAccount() {
    if (this.model.AccountName == undefined || this.model.Password == undefined)
    {
      alert('Mandatory fields are empty! Check email/password!');
      return;
    }

    let accountOperation:Observable<OperationResult>;

    // read parent account information

    let authorizationStored = localStorage.getItem('auth-info');

    if (authorizationStored !== null) {
      let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);    
      this.model.ParentAccount = authorizationProperties.accountName;
    }

    if (!this.editing){
      accountOperation = this.modelService.addAccount(this.model)
    } else {
      //accountOperation = this.modelService.updateAccount(this.model)
    }

    let subs = accountOperation.subscribe(
      accountResult => 
      {
        this.model = new Account();
        if (this.editing) this.editing = !this.editing;
        alert(accountResult.Message);
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
