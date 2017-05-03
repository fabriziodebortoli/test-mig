import { ComponentInfo } from './../shared/models/component.info';
import { LayoutService } from 'app/core/layout.service';
import { Component, OnInit,Output,EventEmitter,  ViewChild, OnDestroy, HostListener, ElementRef, AfterContentInit } from '@angular/core';

import { environment } from './../../environments/environment';

import { ComponentService } from './../core/component.service';
import { LoginSessionService } from './../core/login-session.service';
import { SidenavService } from './../core/sidenav.service';
import { TabStripComponent } from "@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component";

@Component({
  selector: 'tb-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy, AfterContentInit {

  @ViewChild('sidenav') sidenav;
  sidenavSubscription: any;

  @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;
  @ViewChild('tabberContainer') tabberContainer: ElementRef;
  viewHeight: number;

  private appName = environment.appName;
  private companyName = environment.companyName;

  constructor(
    private sidenavService: SidenavService,
    private loginSession: LoginSessionService,
    private componentService: ComponentService,
    private layoutService: LayoutService
  ) {
    this.sidenavSubscription = sidenavService.sidenavOpened$.subscribe(() => this.sidenav.toggle());

  }

  ngOnInit() {

  }

  ngAfterContentInit() {
    setTimeout(() => this.calcViewHeight(), 0);
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    this.calcViewHeight();
  }
  calcViewHeight() {
    this.viewHeight = this.tabberContainer ? this.tabberContainer.nativeElement.offsetHeight : 0;
    this.layoutService.setViewHeight(this.viewHeight);
    console.log("viewHeight", this.viewHeight);
  }

  ngOnDestroy() {
    this.sidenavSubscription.unsubscribe();
  }

  toggleSidenav() {
    this.sidenavService.toggleSidenav();
  }

  closeTab(info: ComponentInfo) {
    event.stopImmediatePropagation();
    this.kendoTabStripInstance.selectTab(0);
    this.componentService.tryDestroyComponent(info);    
  }

}
