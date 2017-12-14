import { OperationResult } from './../../services/operationResult';
import { Observable } from 'rxjs/Observable';
import { ModelService } from './../../services/model.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Instance } from '../../model/instance';
import { Router, ActivatedRoute } from '@angular/router';
import { AppSubscription } from '../../model/subscription';
import { Subscription } from 'rxjs';
import { ServerUrl, UrlType } from 'app/authentication/server-url';

@Component({
  selector: 'app-instance',
  templateUrl: './instance.component.html',
  styleUrls: ['./instance.component.css']
})

export class InstanceComponent implements OnInit, OnDestroy {

  model: Instance;
  serverURLs: ServerUrl[];
  instanceSubscriptions: AppSubscription[];
  editing: boolean = false;
  subscription: Subscription;

  // dialog
  fields: Array<{label:string, value:string, hide: boolean}>;
  openToggle: boolean;
  result: boolean;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) {
    this.model = new Instance();
    this.serverURLs = [];
    this.instanceSubscriptions = [];
    this.openToggle = false;
    this.result = false;
    this.initDialogFields();
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
    let instanceKey: string = this.route.snapshot.queryParams['instanceToEdit'];
    
    if (instanceKey !== undefined) {
      
      this.editing = true;
      this.modelService.getInstances(instanceKey)
      .subscribe(
          res => {
            let instances:Instance[] = res['Content'];

            if (instances.length == 0) {
              return;
            }
            
            this.model = instances[0];

            // now we read its subscriptions

            this.modelService.query("subscriptioninstances", { MatchingFields : { InstanceKey: this.model.InstanceKey } }).subscribe(
              res => {
                this.instanceSubscriptions = res['Content'];
              },
              err => {
                alert('Error occurred while querying GWAM to obtain subscriptions for this instance ' + err);
              }
            );

            this.modelService.query("serverurls", { MatchingFields: { InstanceKey : this.model.InstanceKey} }).subscribe(
              res => {
                this.serverURLs = res['Content'];
              },
              err => {
                alert('Error occurred while querying GWAM to obtain instance servers ' + err);
              }
            )
          },
          err => {
            alert(err);
          }
        )
    }
  }

  //--------------------------------------------------------------------------------
  initDialogFields() {
    this.fields = [
      { label: 'url', value: '', hide: false },
      { label: 'urlType', value: '0', hide: false }
    ];    
  }

  //--------------------------------------------------------------------------------
  openAddServerDialog() {
    this.openToggle = true;
  }

  //--------------------------------------------------------------------------------
  onCloseCredentialsDialog() {

    if (!this.result) {
      return;
    }

    let serverUrl: ServerUrl = this.getServerInfoFromDialog(this.fields);
    this.serverURLs.push(serverUrl);
    this.initDialogFields();
  }

  //--------------------------------------------------------------------------------
  getServerInfoFromDialog(formFields: Array<{label:string, value:string, hide: boolean}>) {
  
    let url: string;
    let urlType: string;

    for (let i=0;i<formFields.length;i++) {
      let item = formFields[i];

      if (item.label === 'Url') {
        url = item.value;
        continue;
      }

      if (item.label === 'UrlType') {
        urlType = item.value;
        continue;
      }      
    }

    return new ServerUrl(this.model.InstanceKey, UrlType.TBLOADER, url);
  }

  //--------------------------------------------------------------------------------
  ngOnDestroy() {
    if (this.subscription === undefined)
      return;

    this.subscription.unsubscribe;
  }
}
