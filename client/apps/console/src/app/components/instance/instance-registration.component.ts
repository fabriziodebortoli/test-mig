import {SubscriptionAccount} from '../../model/subscriptionAccount';
import { OperationResult } from './../../services/operationResult';
import { Observable } from 'rxjs/Observable';
import { ModelService } from './../../services/model.service';
import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { Instance } from '../../model/instance';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { Credentials } from 'app/authentication/credentials';

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
  subscriptionSaveInstance: Subscription;
  subscriptionReadSubscriptions: Subscription;
  @Input() subscriptions: Array<SubscriptionAccount>;
  
  currentStep: number;
  readingData: boolean;

  //--------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) {
    this.model = new Instance();
    this.subscriptions = new Array<SubscriptionAccount>();
    this.currentStep = 1;
    this.securityValue = '';
    this.accountName = '';
    this.password = '';
  }

  //--------------------------------------------------------------------------------
  getPermission() {

    if (this.accountName === '' || this.password === '') {
      return;
    }

    let credentials = new Credentials();
    credentials.accountName = this.accountName;
    credentials.password = this.password;

    this.modelService.getPermissionToken(credentials, "newinstance").subscribe(
      res => {
        this.activationCode = res['Content'];
      },
      err => {
        alert('Cannot get a permission :(');
        this.activationCode = '';
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

    this.subscriptionSaveInstance = this.modelService.registerInstance(this.model, this.activationCode).subscribe(
      res => {

        if (!res.Result) {
          alert(res.Message);
          return;
        }

        alert('Instance has been registered.');
        this.currentStep++;
        
        this.securityValue = res['Content'].securityValue;

        this.modelService.query('subscriptionaccounts', { MatchingFields : { AccountName: "francesco.ricceri@microarea.it" } }, this.activationCode).subscribe(
          res => {
            this.subscriptions = res['Content'];
            this.readingData = false;
          },
          err => {
            alert(err);
            this.readingData = false;
          }
        );

      },
      err => {
        alert(err);
        this.readingData = false;
      }
    );

  }

  //--------------------------------------------------------------------------------
  associateInstanceToSubscription(subAcc) {

    let instanceKey: string = this.model.InstanceKey;

    if (!confirm('This command will associate the instance ' + instanceKey + ' to this subscription: ' + subAcc.SubscriptionKey + '). Confirm?')) {
      return;
    }

    this.readingData = true;

    this.modelService.addInstanceSubscriptionAssociation(instanceKey, subAcc.SubscriptionKey).subscribe(
      res => {
        alert('Association regularly saved.');
        this.currentStep++;

        this.model.Activated = true;

        this.modelService.setData({}, true, this.activationCode, instanceKey, this.accountName).retry(3).subscribe(
          res => {

            this.modelService.getInstances(instanceKey, this.activationCode).subscribe(
              res => {

                let instances:Instance[] = res['Content'];
                
                if (instances.length == 0) {
                  return;
                }
                
                this.model = instances[0];
                this.model.SecurityValue = this.securityValue;

                //we read the instance, now we pass it to the admin console

                this.modelService.saveInstance(this.model, false, this.activationCode).retry(3).subscribe(
                  res => { alert('Registration complete');},
                  err => { alert('Registration Failed'); }
                )
              },
              err => {}
            )
          },
          err => { alert('An error occurred while updating the Instance on GWAM');}
        )
        
      },
      err => {
        alert('Oops, something went wrong with your request: ' + err);
        this.readingData = false;
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
