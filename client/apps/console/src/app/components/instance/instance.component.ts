import { OperationResult } from './../../services/operationResult';
import { Observable } from 'rxjs/Observable';
import { ModelService } from './../../services/model.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Instance } from '../../model/instance';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { ServerUrl } from 'app/authentication/server-url';

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

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) {
    this.model = new Instance();
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

   //--------------------------------------------------------------------------------------------------------
   ngOnDestroy() {
    if (this.subscription === undefined)
      return;

    this.subscription.unsubscribe;
  }
}
