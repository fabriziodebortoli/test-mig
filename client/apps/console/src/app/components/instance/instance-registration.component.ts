import {Credentials} from '../../authentication/credentials';
import { OperationResult } from './../../services/operationResult';
import { Observable } from 'rxjs/Observable';
import { ModelService } from './../../services/model.service';
import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { Instance } from '../../model/instance';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { retry } from 'rxjs/operator/retry';
import { transition } from '@angular/core/src/animation/dsl';
import { COMPONENT_VARIABLE } from '@angular/platform-browser/src/dom/dom_renderer';

@Component({
  selector: 'app-instance',
  templateUrl: './instance-registration.component.html',
  styleUrls: ['./instance-registration.component.css']
})

//================================================================================
export class InstanceRegistrationComponent implements OnInit, OnDestroy {

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

  // credentials form data
  fields: Array<{label:string, value:string, hide: boolean}>;
  openToggle: boolean;
  credentials: Credentials;

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
    this.fields = [
      { label: 'username', value:'', hide: false},
      { label: 'password', value:'', hide: true}
    ];
    this.openToggle = false;
    this.credentials = new Credentials();
  }

  //--------------------------------------------------------------------------------
  ngOnInit(): void {
    this.openToggle = true;
  }  

  //--------------------------------------------------------------------------------
  onCloseCredentialsDialog() {
    this.credentials = this.getCredentials(this.fields);
    this.accountName = this.credentials.accountName;
    this.password = this.credentials.password;    
  }

  //--------------------------------------------------------------------------------
  getCredentials(formFields: Array<{label:string, value:string, hide: boolean}>) {

    let credentials = new Credentials();

    for (let i=0;i<formFields.length;i++) {
      let item = formFields[i];

      if (item.label === 'username') {
        credentials.accountName = item.value;
        continue;
      }

      if (item.label === 'password') {
        credentials.password = item.value;
        continue;
      }
    }

    return credentials;
  }

  //--------------------------------------------------------------------------------
  getPermission() {

    if (this.credentials.accountName === '' || this.credentials.password === '') {
      alert('Invalid credentials.');
    }

    this.busy = true;

    this.modelService.getPermissionToken(this.credentials, "newinstance").subscribe(
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
