import { ModelService } from './../../../services/model.service';
import { AccountService } from './../../../services/account.service';
import { Component, OnInit } from '@angular/core';
import { Account } from '../../../model/account';

@Component({
  selector: 'app-account-list',
  templateUrl: './account-list.component.html',
  styleUrls: ['./account-list.component.css']
})
export class AccountListComponent implements OnInit {

  //accountService: AccountService;
  accountArray: Account[];
  errText: string;

  constructor(private modelService: ModelService) {
  }

  ngOnInit() {
    this.getAccounts();
  }

  getAccounts() {
    this.modelService.getAccounts()
      .subscribe(
      ar => this.accountArray = ar,
      error => this.errText);
  }
}
