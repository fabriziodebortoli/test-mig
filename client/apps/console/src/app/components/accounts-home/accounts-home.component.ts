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

  constructor(private modelService: ModelService, private router: Router) {}

  ngOnInit() {

    let authorizationStored = localStorage.getItem('auth-info');

    if (authorizationStored === null) {
      alert('User must be logged in.');
      return;
    }

    let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);

    this.modelService.getAccounts({ parentAccount: authorizationProperties.accountName })
      .subscribe(
        accounts => {
          this.accounts = accounts['Content'];
        },
        err => {
          alert(err);
        }
      )
  }

  openAccount(item:object){
    // route to edit account
    this.router.navigate(['/account'], { queryParams: { accountNameToEdit: item['AccountName'] } });
  }
}
