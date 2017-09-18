import {SubscriptionAccount} from '../../model/subscriptionAccount';
import {RoleNames, RoleLevels} from '../../authentication/auth-helpers';
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
  subscriptionsAccount:Array<SubscriptionAccount>;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) { 
    this.model = new Account();
    this.subscriptionsAccount = new Array<SubscriptionAccount>();
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {

    if (this.route.snapshot.queryParams['accountNameToEdit'] === undefined) {
      return;
    }
    
    this.editing = true;
    let accountName:string = this.route.snapshot.queryParams['accountNameToEdit'];
    this.modelService.getAccounts({ MatchingFields: { AccountName: accountName } })
      .subscribe(
        res => {
          let accounts:Account[] = res['Content'];

          if (accounts.length == 0) {
            return;
          }
          
          // setting the account model
          this.model = accounts[0];

          if (this.model.AccountName == '')
            return;

          // reading account roles
          this.modelService.query('subscriptionaccounts', { MatchingFields : { AccountName: this.model.AccountName } })
            .subscribe(
              res => {
                this.subscriptionsAccount = res['Content'];
                if (this.subscriptionsAccount.length == 0) {
                  return;
                }
              },
              err => {
                alert(err);
              }
            )          
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
      alert('Mandatory fields are empty: please check email and password.');
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
        
        let redirectAfterSave: boolean;
        redirectAfterSave = this.route.snapshot.queryParams['redirectOnSave'] === undefined ? false : this.route.snapshot.queryParams['redirectOnSave'] === 'true';

        if (!redirectAfterSave)
        {
          return;
        }
          
        this.router.navigateByUrl('/accountsHome');
      },
      err => 
      { 
        console.log(err); 
        alert(err); 
        subs.unsubscribe();
      }
    )
  }

  doChangePassword() {
    alert('Sorry, not implemented :(');
  }
}