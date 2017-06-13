import { Component, OnInit } from '@angular/core';
import {Observable} from 'rxjs/Rx';
import { NgForm }    from '@angular/forms';
import { Account } from './../model/account';
import { ModelService } from './../services/model.service';

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

  submitAccount(){
    let accountOperation:Observable<Account[]>;

    if (!this.editing){
      accountOperation = this.modelService.addAccount(this.model)
    } else {
      //accountOperation = this.modelService.updateAccount(this.model)
    }

    accountOperation.subscribe(
      accounts => {
        this.model = new Account();
        if (this.editing) this.editing = !this.editing;
      },
      err => {
        console.log(err);
      }
    )
  }

}
