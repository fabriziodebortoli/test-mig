import { OperationResult } from './../../services/operationResult';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { NgForm }    from '@angular/forms';
import { Account } from './../../model/account';
import { AccountService } from './../../services/account.service';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.css']
})

export class AccountComponent implements OnInit {

  model:Account;
  editing:boolean = false;

  constructor(private modelService: AccountService) { 
    this.model = new Account();
  }

  ngOnInit() {
  }

  submitAccount() {
    if (this.model.accountName == undefined || this.model.password == undefined)
    {
      alert('Mandatory fields are empty! Check email/password!');
      return;
    }

    let accountOperation:Observable<OperationResult>;

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
