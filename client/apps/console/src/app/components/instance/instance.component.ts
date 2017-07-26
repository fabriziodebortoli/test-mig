import { OperationResult } from './../../services/operationResult';
import { Observable } from 'rxjs/Observable';
import { ModelService } from './../../services/model.service';
import { Component, OnInit } from '@angular/core';
import { Instance } from '../../model/instance';

@Component({
  selector: 'app-instance-home',
  templateUrl: './instance.component.html',
  styleUrls: ['./instance.component.css']
})

export class InstanceComponent implements OnInit {

  model: Instance;
  editing: boolean = false;

  constructor(private modelService: ModelService) {
    this.model = new Instance();
   }

  ngOnInit() {
  }

  submitInstance() {
    if (this.model.InstanceKey == '') {
      alert('Mandatory fields are empty! Check Instance key!');
      return;
    }

     let instanceOperation: Observable<OperationResult>;

    if (!this.editing) {
      instanceOperation = this.modelService.addInstance(this.model)
    } else {
      //instanceOperation = this.modelService.updateInstance(this.model)
    }

    let instance = instanceOperation.subscribe(
      instanceResult => {
        this.model = new Instance();
        if (this.editing) this.editing = !this.editing;
        alert(instanceResult.Message);
        instance.unsubscribe();
      },
      err => {
        console.log(err);
        alert(err);
        instance.unsubscribe();
      }
    )
  }
}
