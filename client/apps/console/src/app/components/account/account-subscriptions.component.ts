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
  subscriptionsResult: Array<AccountRole>;
  columnNames:string[] = ['SubscriptionKey'];
  columnNamesAccountRoles:string[] = ['EntityKey'];

  searchString: string;

  
  constructor(private modelService: ModelService) { 
    this.subscriptions = new Array<SubscriptionAccount>();
    this.subscriptionsResult = new Array<AccountRole>();
  }

  doSearchSubscriptions() {

    let authorizationStored = localStorage.getItem('auth-info');

    if (authorizationStored === null) {
      return;
    }

    //@@TODO: put this code under a centralized interface to AuthorizationInfo on localStorage

    let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);

    this.modelService.query("accountroles", 
      { 
        MatchingFields: { AccountName : authorizationProperties.accountName, Level : RoleLevels.Subscription, RoleName : RoleNames.Admin },
        LikeFields: { EntityKey : this.searchString }
      })
      .subscribe(res => { this.subscriptionsResult = res['Content']; });
  }
}
