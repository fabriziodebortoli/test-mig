import { AuthorizationInfo} from '../../authentication/auth-info';
import { RoleNames, RoleLevels} from '../../authentication/auth-helpers';
import { ModelService} from '../../services/model.service';
import { SubscriptionAccount } from '../../model/subscriptionAccount';
import { AccountRole } from 'app/model/accountRole';
import { Component, OnInit, Input, NgZone, ChangeDetectorRef, ApplicationRef, ViewChild, ElementRef } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/operator/debounceTime';
import { Subscription } from "rxjs/Rx";
import { Observable } from "rxjs/Observable";
import { AuthorizationProperties } from "app/authentication/auth-info";

@Component({
  selector: 'app-account-subscriptions',
  templateUrl: './account-subscriptions.component.html',
  styleUrls: ['./account-subscriptions.component.css']
})
export class AccountSubscriptionsComponent {

  @Input() subscriptions: Array<SubscriptionAccount>;
  @Input() editAccountName: string;
  subscriptionsResult: Array<AccountRole>;
  columnNames:string[] = ['SubscriptionKey'];
  columnNamesAccountRoles:string[] = ['EntityKey'];

  searchString: string;
  readingData: boolean;
  readingDataLinked: boolean;
  
  //--------------------------------------------------------------------------------
  constructor(private modelService: ModelService) { 
    this.subscriptions = new Array<SubscriptionAccount>();
    this.subscriptionsResult = new Array<AccountRole>();
  }

  //--------------------------------------------------------------------------------
  doSearchSubscriptions() {

    let authorizationStored = localStorage.getItem('auth-info');

    if (authorizationStored === null) {
      return;
    }

    //@@TODO: better to put parentAccount as a Component Property!
    //        put this code under a centralized interface to AuthorizationInfo on localStorage

    let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);
    this.readingData = true;

    this.modelService.query("accountroles", 
      { 
        MatchingFields: { AccountName : authorizationProperties.accountName, Level : RoleLevels.Subscription, RoleName : RoleNames.Admin },
        LikeFields: { EntityKey : this.searchString }
      })
      .subscribe(res => { 
        this.subscriptionsResult = res['Content']; 
        this.readingData = false;
      },
    err => {
      alert('An error occurred while searching subscriptions');
      this.readingData = false;
    });
  }

  //--------------------------------------------------------------------------------
  associateSubscription(item: AccountRole) {

    if(!confirm('This command will associate the subscription ' + item.EntityKey + ' to this account (' + this.editAccountName + '). Confirm?')) {
      return;
    }

    let subs: string[] = [item.EntityKey];
    this.readingData = true;

    this.modelService.addAccountSubscriptionAssociation(this.editAccountName, subs).subscribe(
      res => {
        alert('Association regularly saved.');
        let subAcc:SubscriptionAccount = new SubscriptionAccount();
        subAcc.accountName = this.editAccountName;
        subAcc.subscriptionKey = item.EntityKey;
        this.subscriptions.push(subAcc);
        this.readingData = false;
      },
      err => {
        alert('Oops, something went wrong with your request: ' + err);
        this.readingData = false;
      }
    );
  }

  //--------------------------------------------------------------------------------
  deleteAssociation(item: SubscriptionAccount) {

    if (!confirm('Delete association between the subscription ' + item.subscriptionKey + ' and this account (' + this.editAccountName + ')?')) {
      return;
    }

    let subs: string[] = [item.subscriptionKey];
    this.readingData = true;

    this.modelService.queryDelete(
        'subscriptionaccounts', 
        { MatchingFields : { AccountName : this.editAccountName , SubscriptionKey : item.subscriptionKey } } )
      .subscribe(
        res => {
          alert('Association has been deleted.');
          let index:number = this.subscriptions.findIndex(
            p => p.accountName == this.editAccountName && p.subscriptionKey == item.subscriptionKey);
          if (index > -1) {
            this.subscriptions.splice(index, 1);
          }
          this.readingData = false;
        },
        err => {
          alert('Oops, something went wrong with your request: ' + err);
          this.readingData = false;
        }
      );
  }
}
