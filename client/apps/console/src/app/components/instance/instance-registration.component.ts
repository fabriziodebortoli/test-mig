import {SubscriptionAccount} from '../../model/subscriptionAccount';
import { OperationResult } from './../../services/operationResult';
import { Observable } from 'rxjs/Observable';
import { ModelService } from './../../services/model.service';
import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { Instance } from '../../model/instance';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { Credentials } from 'app/authentication/credentials';
import { retry } from 'rxjs/operator/retry';

@Component({
  selector: 'app-instance',
  templateUrl: './instance-registration.component.html',
  styleUrls: ['./instance-registration.component.css']
})

//================================================================================
export class InstanceRegistrationComponent implements OnDestroy {

  model: Instance;
  accountName: string;
  password: string;
  activationCode: string;
  securityValue: string;
  subscriptionKey: string;
  subscriptionSaveInstance: Subscription;
  subscriptionReadSubscriptions: Subscription;
  @Input() subscriptions: Array<SubscriptionAccount>;
  
  currentStep: number;
  readingData: boolean;
  clusterStep: number;
  busy: boolean;

  //--------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) {
    this.model = new Instance();
    this.activationCode = '';
    this.subscriptions = new Array<SubscriptionAccount>();
    this.currentStep = 1;
    this.securityValue = '';
    this.accountName = '';
    this.password = '';
    this.subscriptionKey = '';
    this.clusterStep = 0;
    this.busy = false;
  }

  //--------------------------------------------------------------------------------
  getPermission() {

    if (this.accountName === '' || this.password === '') {
      return;
    }

    let credentials = new Credentials();
    credentials.accountName = this.accountName;
    credentials.password = this.password;

    this.busy = true;

    this.modelService.getPermissionToken(credentials, "newinstance").subscribe(
      res => {
        this.activationCode = res['Content'];
        this.busy = false;
      },
      err => {
        alert('Cannot get a permission :(');
        this.activationCode = '';
        this.busy = false;
      }
    )

  }

  //--------------------------------------------------------------------------------
  submitInstance() {

    if (this.model.InstanceKey == '') {
      alert('To proceed, an Instance key is required.');
      return;
    }

    this.readingData = true;
    this.busy = true;

    this.subscriptionSaveInstance = this.modelService.registerInstance(this.model, this.accountName, this.activationCode).subscribe(
      res => {

        if (!res.Result) {
          alert(res.Message);
          this.readingData = false;
          this.busy = false;
          return;
        }

        this.currentStep++;
        
        this.securityValue = res['Content'].securityValue;

        this.modelService.query('subscriptionaccounts', { MatchingFields : { AccountName: this.accountName } }, this.activationCode).subscribe(
          res => {
            this.subscriptions = res['Content'];
            this.readingData = false;
            this.busy = false;
          },
          err => {
            alert(err);
            this.readingData = false;
            this.busy = false;
          }
        );

      },
      err => {
        alert(err);
        this.readingData = false;
        this.busy = false;
      }
    );

  }

  //--------------------------------------------------------------------------------
  associateInstanceToSubscription(subAcc) {

    let instanceKey: string = this.model.InstanceKey;
    this.subscriptionKey = subAcc.SubscriptionKey;

    if (this.activationCode === '') {
      alert('Permission token is missing, please ask one.');
      return;
    }

    if (!confirm('This command will associate the instance ' + instanceKey + ' to this subscription: ' + subAcc.SubscriptionKey + '). Confirm?')) {
      return;
    }

    this.readingData = true;

    this.modelService.addInstanceSubscriptionAssociation(instanceKey, subAcc.SubscriptionKey, this.activationCode).subscribe(
      res => {
        this.currentStep++;
        this.model.Activated = true;
        this.busy = true;

        this.modelService.setData({}, true, this.activationCode, instanceKey, this.accountName).retry(3).subscribe(
          res => {

            let apiQuery = {
              matchingField : null,
              likeFields : null,
              addDependencies : true,
              accountName : this.accountName,
              subscriptionKey : this.subscriptionKey
            }

            this.clusterStep = 1;

            this.modelService.getObjectCluster('instances', instanceKey, "0", apiQuery, this.activationCode).subscribe(
              res => {

                let instanceCluster = res['Content'];
                
                if (instanceCluster === null || instanceCluster === undefined) {
                  this.clusterStep = 0;
                  this.busy = false;
                  return;
                }
                
                this.model = instanceCluster['instance'];
                this.model.SecurityValue = this.securityValue;

                // we got the instance, now we pass it to the admin console

                this.clusterStep = 2;

                this.modelService.saveCluster(instanceCluster, this.activationCode).retry(3).subscribe(
                  res => { 
                    this.clusterStep = 3;
                    this.busy = false;
                  },
                  err => { 
                    alert('Registration Failed'); 
                    this.clusterStep = 0;
                    this.busy = false;
                  }
                )
              },
              err => { 
                this.clusterStep = 0; 
                this.busy = false;
              }
            )

          },
          err => { 
            alert('An error occurred while updating the Instance on GWAM');
            this.clusterStep = 0;
            this.busy = false;
          }
        )
        
      },
      err => {
        alert('Oops, something went wrong with your request: ' + err);
        this.clusterStep = 0;
        this.readingData = false;
        this.busy = false;
      }
    );
  }

  //--------------------------------------------------------------------------------
  ngOnDestroy() {

    if (this.subscriptionSaveInstance === undefined)
      return;

    this.subscriptionSaveInstance.unsubscribe();

    if (this.subscriptionReadSubscriptions === undefined)
      return;

    this.subscriptionReadSubscriptions.unsubscribe();
  }
}
