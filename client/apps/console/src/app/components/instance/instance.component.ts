import { OperationResult } from './../../services/operationResult';
import { Observable } from 'rxjs/Observable';
import { ModelService } from './../../services/model.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Instance } from '../../model/instance';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-instance',
  templateUrl: './instance.component.html',
  styleUrls: ['./instance.component.css']
})

export class InstanceComponent implements OnInit, OnDestroy {

  model: Instance;
  editing: boolean = false;
  subscription: Subscription;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) {
    this.model = new Instance();
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {

    if (this.route.snapshot.queryParams['instanceToEdit'] !== undefined) {
      this.editing = true;
      let instanceKey: string = this.route.snapshot.queryParams['instanceToEdit'];
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
  submitInstance() {

    if (this.model.InstanceKey == '') {
      alert('Mandatory fields are empty! Check Instance key!');
      return;
    }

    this.subscription = this.modelService.saveInstance(this.model).subscribe(
      res => {

        if (!res.Result) {
          alert(res.Message);
          return;
        }

        this.model = new Instance();
        
        if (this.editing) {
          this.editing = !this.editing;
        }

        this.router. navigateByUrl('/instancesHome');
      },
      err => {
        alert(err);
      }
    );

  }

  ngOnDestroy() {
    if (this.subscription === undefined)
      return;

    this.subscription.unsubscribe;
  }
  
}
