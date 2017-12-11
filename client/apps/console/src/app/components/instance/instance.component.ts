import { OperationResult } from './../../services/operationResult';
import { Observable } from 'rxjs/Observable';
import { ModelService } from './../../services/model.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Instance } from '../../model/instance';
import { Router, ActivatedRoute } from '@angular/router';
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
  editing: boolean = false;
  subscription: Subscription;

  // dialog
  fields: Array<{label:string, value:string, hide: boolean}>;
  openToggle: boolean;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) {
    this.model = new Instance();
    this.serverURLs = [];
    this.openToggle = false;
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
      { label: 'appName', value: '', hide: false },
      { label: 'url', value: '', hide: false }
    ];    
  }

  //--------------------------------------------------------------------------------
  openAddServerDialog() {
    this.openToggle = true;
  }

  //--------------------------------------------------------------------------------
  onCloseCredentialsDialog() {
    let serverUrl: ServerUrl = this.getServerInfoFromDialog(this.fields);
    this.serverURLs.push(serverUrl);
    this.initDialogFields();
  }

  //--------------------------------------------------------------------------------
  getServerInfoFromDialog(formFields: Array<{label:string, value:string, hide: boolean}>) {
  
    let appName: string;
    let url: string;

    for (let i=0;i<formFields.length;i++) {
      let item = formFields[i];

      if (item.label === 'appName') {
        appName  = item.value;
        continue;
      }

      if (item.label === 'url') {
        url = item.value;
        continue;
      }
    }

    return new ServerUrl(UrlType.TBLOADER, url, appName);
  }

  //--------------------------------------------------------------------------------
  ngOnDestroy() {
    if (this.subscription === undefined)
      return;

    this.subscription.unsubscribe;
  }
}
