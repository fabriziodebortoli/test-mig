import { ModelService } from '../../services/model.service';
import { Account } from '../../model/account';
import { Component, OnInit } from '@angular/core';


@Component({
  selector: 'app-accounts-home',
  templateUrl: './accounts-home.component.html',
  styleUrls: ['./accounts-home.component.css']
})
export class AccountsHomeComponent implements OnInit {

  //accounts: Array<Account> = new Array<Account>();
  accounts: Account[];
  //columnNames: Array<string> = new Array<string>('AccountName', 'FullName');

  constructor(private modelService: ModelService) { 

  }

  ngOnInit() {
    this.modelService.getAccounts({ parentAccount: 'fricceri@m4.com'})
      .subscribe(
        accounts => {
          this.accounts = accounts['Content'];
        },
        err => {
          alert(err);
        }
      )
  }

}
