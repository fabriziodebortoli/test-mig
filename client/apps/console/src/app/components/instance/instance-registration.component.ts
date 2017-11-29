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
  currentStep: number;
  clusterStep: number;
  busy: boolean;

  //--------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) {
    this.model = new Instance();
    this.activationCode = '';
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

    this.busy = true;

    this.subscriptionSaveInstance = this.modelService.registerInstance(this.model, this.accountName, this.activationCode).subscribe(
      res => {

        if (!res.Result) {
          alert(res.Message);
          this.busy = false;
          return;
        }

        this.securityValue = res['Content'].securityValue;
        let instanceCluster = res['Content'].dataCluster;

        if (instanceCluster === null || instanceCluster === undefined) {
          this.clusterStep = 0;
          this.busy = false;
          return;
        }        

        this.currentStep++;

        this.clusterStep = 1;

        this.modelService.saveCluster(instanceCluster, this.activationCode).retry(3).subscribe(
          res => { 
            this.clusterStep = 2;
            this.busy = false;

            this.modelService.setData({}, true, this.activationCode, this.model.InstanceKey, this.accountName).retry(3).subscribe(
              res => {
                this.clusterStep = 3;
                this.busy = false;
              },
              err => {
                this.clusterStep = 0;
                this.busy = false;
              }
            )

          },
          err => { 
            alert('Registration Failed'); 
            this.clusterStep = 0;
            this.busy = false;
          }
        )        


      },
      err => {
        alert(err);
        this.clusterStep = 0;
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
