import { Router } from '@angular/router';
import { ModelService } from '../../services/model.service';
import { Account } from '../../model/account';
import { Component, OnInit } from '@angular/core';
import { AuthorizationProperties } from "app/authentication/auth-info";

@Component({
  selector: 'app-accounts-home',
  templateUrl: './accounts-home.component.html',
  styleUrls: ['./accounts-home.component.css']
})
export class AccountsHomeComponent implements OnInit {

  accounts: Account[];
  readingData:boolean;

  constructor(private modelService: ModelService, private router: Router) {
    this.accounts = [];
  }

  ngOnInit() {

    let authorizationStored = localStorage.getItem('auth-info');

    if (authorizationStored === null) {
      alert('User must be logged in.');
      return;
    }

    let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);
    this.readingData = true;

    this.modelService.getAccounts({ MatchingFields: { parentAccount: authorizationProperties.accountName } })
      .subscribe(
        accounts => {
          this.accounts = accounts['Content'];
          this.readingData = false;
        },
        err => {
          alert(err);
          this.readingData = false;
        }
      )
  }

  openAccount(item:object){

    if (item === undefined) {
      this.router.navigate(['/account']);
      return;
    }
    
    this.router.navigate(['/account'], { queryParams: { accountNameToEdit: item['AccountName'], redirectOnSave: true } });
  }
}
