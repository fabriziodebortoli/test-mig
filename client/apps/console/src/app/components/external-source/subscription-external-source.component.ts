import { Component, OnInit } from '@angular/core';
import { SubscriptionExternalSource } from 'app/model/subscriptionExternalSource';
import { ModelService } from '../../services/model.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountInfo } from '../../authentication/account-info';
import { OperationResult } from '../../services/operationResult';
import { Observable } from 'rxjs/Observable';

@Component({
  selector: 'app-subscription-external-source',
  templateUrl: './subscription-external-source.component.html',
  styleUrls: ['./subscription-external-source.component.css']
})

export class SubscriptionExternalSourceComponent implements OnInit {

  model: SubscriptionExternalSource;
  editing: boolean = false;

  //--------------------------------------------------------------------------------------------------------
  constructor(
    private modelService: ModelService,
    private route: ActivatedRoute,
    private router: Router) {
  }

  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
    // I read queryparams
    let subscriptionKey: string = this.route.snapshot.queryParams['subscriptionToEdit'];
    if (subscriptionKey === undefined) {
      return;
    }

    let sourceName = this.route.snapshot.queryParams['sourceToEdit'];

    this.model = new SubscriptionExternalSource();

    this.model.SubscriptionKey = subscriptionKey;

    // I need the instanceKey where the currentAccount is logged
    let localAccountInfo = localStorage.getItem(this.modelService.currentAccountName);
    if (localAccountInfo != null && localAccountInfo != '') {
      let accountInfo: AccountInfo = JSON.parse(localAccountInfo);
      this.model.InstanceKey = accountInfo.instanceKey;
    }

    if (sourceName === undefined) {
      return;
    }

    this.editing = true;
    this.model.Source = sourceName;
  }

  //--------------------------------------------------------------------------------------------------------
  ngAfterContentInit(): void {

    this.loadData();
  }

  //--------------------------------------------------------------------------------------------------------
  loadData() {

    this.modelService.getExternalSource(this.model.SubscriptionKey, this.model.Source)
      .subscribe(
      res => {
        let sources: SubscriptionExternalSource[] = res['Content'];

        if (sources.length == 0)
          return;

        // for each field I have to assign each value!
        //this.model.assign(databases[0]);
        this.model = sources[0];
      },
      err => { alert(err); }
      )
  }

  //--------------------------------------------------------------------------------------------------------
  submitExternalSource() {
    
    if (this.model.SubscriptionKey === undefined || this.model.Source === undefined || this.model.Source === '') {
      alert('Mandatory fields are empty! Check subscription key / source name!');
      return;
    }

    let saveOperation: Observable<OperationResult> = this.modelService.saveExternalSource(this.model);

    let subs = saveOperation.subscribe(
      subscriptionResult => {

        subs.unsubscribe();
        this.editing = true;

        this.router.navigate(['/subscription'], { queryParams: { subscriptionToEdit: this.model.SubscriptionKey } });

        // after save I return to parent page
        //this.router.navigateByUrl('/subscriptionsHome');
      },
      err => {
        console.log(err);
        alert(err);
        subs.unsubscribe();
      }
    )
  }
}
