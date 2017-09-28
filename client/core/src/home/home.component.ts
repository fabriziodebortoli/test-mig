import { Component, OnInit, Output, EventEmitter, ViewChild, OnDestroy, HostListener, ElementRef, AfterContentInit, ViewEncapsulation } from '@angular/core';
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";
import { Subscription } from 'rxjs';

import { ComponentInfo } from './../shared/models/component-info.model';
import { MessageDlgArgs } from './../shared/models';

import { TabStripComponent } from "@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component";
import { MessageDialogComponent } from './../shared/containers/message-dialog/message-dialog.component';

import { InfoService } from './../core/services/info.service';
import { UtilsService } from './../core/services/utils.service';
import { ComponentInfoService } from './../core/services/component-info.service';
import { EnumsService } from './../core/services/enums.service';
import { TabberService } from './../core/services/tabber.service';
import { LayoutService } from './../core/services/layout.service';
import { ComponentService } from './../core/services/component.service';
import { TaskbuilderService } from './../core/services/taskbuilder.service';
import { SidenavService } from './../core/services/sidenav.service';
import { SettingsService } from './../menu/services/settings.service';
import { LocalizationService } from './../menu/services/localization.service';
import { MenuService } from './../menu/services/menu.service';

@Component({
  selector: 'tb-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  animations: [
    trigger(
      'fadeInOut', [
        transition(':enter', [style({ opacity: 0 }), animate('100ms', style({ 'opacity': 1 }))]),
        transition(':leave', [style({ 'opacity': 1 }), animate('500ms', style({ 'opacity': 0 }))])
      ]
    )
  ],
  encapsulation: ViewEncapsulation.None,
  providers: [ComponentInfoService]
})
export class HomeComponent implements OnDestroy, AfterContentInit, OnInit {

  @ViewChild('sidenav') sidenav;
  subscriptions: Subscription[] = [];

  @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;
  @ViewChild('tabberContainer') tabberContainer: ElementRef;
  @ViewChild(MessageDialogComponent) messageDialog: MessageDialogComponent;
  viewHeight: number;

  connected: boolean = false;
  isDesktop: boolean;

  constructor(
    private sidenavService: SidenavService,
    private taskbuilderService: TaskbuilderService,
    private componentService: ComponentService,
    private layoutService: LayoutService,
    private tabberService: TabberService,
    private menuService: MenuService,
    private localizationService: LocalizationService,
    private settingsService: SettingsService,
    private enumsService: EnumsService,
    private infoService: InfoService
  ) {

    this.isDesktop = infoService.isDesktop;

    this.subscriptions.push(sidenavService.sidenavOpened$.subscribe(() => this.sidenav.toggle()));

    this.subscriptions.push(componentService.componentInfoCreated.subscribe(arg => {
      if (arg.activate) {
        this.kendoTabStripInstance.selectTab(arg.index + 2);
      }
      this.subscriptions.push(tabberService.tabSelected$.subscribe((index: number) => this.kendoTabStripInstance.selectTab(index)));

    }));

    this.subscriptions.push(componentService.componentInfoRemoved.subscribe(cmp => {
      this.kendoTabStripInstance.selectTab(0);
    }));

    this.subscriptions.push(componentService.componentCreationError.subscribe(reason => {
      const args = new MessageDlgArgs();
      args.ok = true;
      args.text = reason;

      this.messageDialog.open(args);
    }));

    this.menuService.getMenuElements();
    this.localizationService.loadLocalizedElements();
    this.settingsService.getSettings();
    this.enumsService.getEnumsTable();

    // sottoscrivo la connessione TB e WS e, se non attiva, la apro tramite il servizio TaskbuilderService
    this.subscriptions.push(this.taskbuilderService.connected.subscribe(connected => {
      this.connected = connected;
    }));

  }

  ngOnInit() {
    this.taskbuilderService.openConnection();
  }

  ngAfterContentInit() {
    setTimeout(() => this.calcViewHeight(), 0);

    this.layoutService.detectDPI();
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    this.calcViewHeight();
  }
  calcViewHeight() {
    this.viewHeight = this.tabberContainer ? this.tabberContainer.nativeElement.offsetHeight - 31 : 0;
    this.layoutService.setViewHeight(this.viewHeight);
  }

  ngOnDestroy() {
    this.connected = false;
    this.taskbuilderService.dispose();
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  closeTab(info: ComponentInfo) {
    event.stopImmediatePropagation();
    info.document.close();
  }

}
