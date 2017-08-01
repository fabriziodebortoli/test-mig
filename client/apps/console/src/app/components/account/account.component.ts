import {AuthorizationInfo} from '../../authentication/auth-info';
import { Router, ActivatedRoute } from '@angular/router';
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
  loggedAccountName:string;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) { 
    this.model = new Account();
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {

    if (this.route.snapshot.queryParams['accountNameToEdit'] !== undefined){
      this.editing = true;
      let accountName:string = this.route.snapshot.queryParams['accountNameToEdit'];
      this.modelService.getAccounts({ AccountName: accountName })
        .subscribe(
          res => {
            let accounts:Account[] = res['Content'];

            if (accounts.length == 0) {
              return;
            }
            
            this.model = accounts[0];
          },
          err => {
            alert(err);
          }
        )
    } 

    let authorizationStored = localStorage.getItem('auth-info');

    if (authorizationStored !== null) {
      let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);

      let authorizationInfo: AuthorizationInfo = new AuthorizationInfo(
        authorizationProperties.jwtEncoded,
        authorizationProperties.accountName);

      authorizationInfo.authorizationProperties = authorizationProperties;

      this.loggedAccountName = authorizationInfo.authorizationProperties.accountName;
    }

    if (this.loggedAccountName == '') {
      return;
    }

    this.modelService.getSubscriptions({ AccountName: this.loggedAccountName })
      .subscribe(
        res => {
          let accounts:Account[] = res['Content'];

          if (accounts.length == 0) {
            return;
          }
          
          this.model = accounts[0];
        },
        err => {
          alert(err);
        }
      )
    

  }

  //--------------------------------------------------------------------------------------------------------
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

    // now I save the account

    accountOperation = this.modelService.saveAccount(this.model)

    let subs = accountOperation.subscribe(
      accountResult => 
      {
        this.model = new Account();
        if (this.editing) 
          this.editing = !this.editing;
        subs.unsubscribe();
        // after save I return to parent page
        this.router. navigateByUrl('/accountsHome');
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