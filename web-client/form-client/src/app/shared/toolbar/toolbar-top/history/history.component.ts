import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-history',
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.scss']
})
export class HistoryComponent implements OnInit {

  private lblHistory: string = 'Recents'; //TODO localizzazione?

  constructor() { }

  ngOnInit() {
  }

}
