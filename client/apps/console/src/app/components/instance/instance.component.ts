import { OperationResult } from './../../services/operationResult';
import { Observable } from 'rxjs/Observable';
import { ModelService } from './../../services/model.service';
import { Component, OnInit } from '@angular/core';
import { Instance } from '../../model/instance';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-instance',
  templateUrl: './instance.component.html',
  styleUrls: ['./instance.component.css']
})

export class InstanceComponent implements OnInit {

  model: Instance;
  editing: boolean = false;

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

    let instanceOperation: Observable<OperationResult>;

    instanceOperation = this.modelService.saveInstance(this.model)

    let instance = instanceOperation.subscribe(
      instanceResult => {
        this.model = new Instance();
        if (this.editing) 
          this.editing = !this.editing;
        instance.unsubscribe();
        // after save I return to parent page
        this.router. navigateByUrl('/instancesHome');
      },
      err => {
        console.log(err);
        alert(err);
        instance.unsubscribe();
      }
    )
  }
}
