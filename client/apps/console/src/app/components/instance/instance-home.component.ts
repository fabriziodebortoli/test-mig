import { AuthorizationProperties } from './../../authentication/auth-info';
import { Router } from '@angular/router';
import { ModelService } from './../../services/model.service';
import { Instance } from './../../model/instance';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-instance-home',
  templateUrl: './instance-home.component.html',
  styleUrls: ['./instance-home.component.css']
})

export class InstanceHomeComponent implements OnInit {

  instances: Instance[];

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router) {
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
    let authorizationStored = localStorage.getItem('auth-info');

    if (authorizationStored == undefined) {
      alert('User must be logged in.');
      return;
    }

    let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);

    // ask to GWAM the list of instances

    this.modelService.getInstances()
    .subscribe(
      instances => {
        this.instances = instances['Content'];
      },
        err => {
          alert(err);
        }
    )
  }
  
  //--------------------------------------------------------------------------------------------------------
  openInstance(item: object) {
    // route to edit instance
    this.router.navigate(['/instance'], { queryParams: { instanceToEdit: item['InstanceKey'] } });
  }
}
