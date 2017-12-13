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
import { SubscriptionInstance } from '../../model/subscriptionInstance';

@Component({
  selector: 'app-instance-subscriptions',
  templateUrl: './instance-subscriptions.component.html',
  styleUrls: ['./instance-subscriptions.component.css']
})
export class InstanceSubscriptionsComponent {

  @Input() subscriptions: Array<SubscriptionInstance>;
  @Input() instanceKey: string;
  subscriptionsResult: Array<AccountRole>;
  columnNames:string[] = ['SubscriptionKey'];
  columnNamesAccountRoles:string[] = ['EntityKey'];

  searchString: string;
  readingData: boolean;
  readingDataLinked: boolean;

  // dialog box
  openToggle: boolean;
  openYesNoToggle: boolean;
  dialogTitle: string;
  dialogMessage: string;
  dialogResult: boolean;
  dialogYesNoResult: boolean;
  subscriptionKey: string;

  //--------------------------------------------------------------------------------
  constructor(private modelService: ModelService) { 
    this.subscriptions = new Array<SubscriptionInstance>();
    this.subscriptionsResult = new Array<AccountRole>();
    this.instanceKey = '';
    this.openToggle = false;
    this.openYesNoToggle = false;
    this.dialogTitle = '';
    this.dialogMessage = '';
    this.dialogResult = false;
    this.dialogYesNoResult = false;
    this.subscriptionKey = '';
  }

  //--------------------------------------------------------------------------------
  messageDialog(title: string, message: string) {
    this.dialogTitle = title;
    this.dialogMessage = message;
    this.openToggle = true;
  }

  //--------------------------------------------------------------------------------
  confirmDialog(title: string, message: string) {
    this.dialogTitle = title;
    this.dialogMessage = message;
    this.openYesNoToggle = true;
  }

  //--------------------------------------------------------------------------------
  dialogYesNoOnClose() {

    if (!this.dialogYesNoResult) {
      return;
    }

    this.readingData = true;

    this.modelService.addInstanceSubscriptionAssociation(this.instanceKey, this.subscriptionKey).subscribe(
      res => {
        let subIns:SubscriptionInstance = new SubscriptionInstance();
        subIns.instanceKey = this.instanceKey;
        subIns.subscriptionKey = this.subscriptionKey;
        this.subscriptions.push(subIns);
        this.readingData = false;
        this.subscriptionKey = '';
      },
      err => {
        this.messageDialog('Error', 'Oops, something went wrong with your request: ' + err);
        this.readingData = false;
        this.subscriptionKey = '';
      }
    );
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
      this.messageDialog('Error', 'An error occurred while searching subscriptions');
      this.readingData = false;
    });
  }

  //--------------------------------------------------------------------------------
  associateSubscription(item: AccountRole) {
    this.confirmDialog('Confirm', 'Would you like to associate the subscription ' + item.EntityKey + ' to this instance?');
    this.subscriptionKey = item.EntityKey;
  }

  //--------------------------------------------------------------------------------
  deleteAssociation(item: SubscriptionInstance) {

    if (!confirm('Delete association between the instance ' + this.instanceKey + ' and this subscription (' + item.subscriptionKey + ')?')) {
      return;
    }

    let subs: string[] = [item.subscriptionKey];
    this.readingData = true;

    this.modelService.queryDelete(
        'subscriptioninstances', 
        { MatchingFields : { InstanceKey : this.instanceKey , SubscriptionKey : item.subscriptionKey } } )
      .subscribe(
        res => {
          let index:number = this.subscriptions.findIndex(
            p => p.instanceKey == this.instanceKey && p.subscriptionKey == item.subscriptionKey);
          if (index > -1) {
            this.subscriptions.splice(index, 1);
          }
          this.readingData = false;
        },
        err => {
          this.messageDialog('Error', 'Oops, something went wrong with your request: ' + err);
          this.readingData = false;
        }
      );
  }
}
