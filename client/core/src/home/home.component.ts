import { UtilsService } from './../core/services/utils.service';
import { Component, OnInit, Output, EventEmitter, ViewChild, OnDestroy, HostListener, ElementRef, AfterContentInit, ViewEncapsulation } from '@angular/core';
import { Subscription } from 'rxjs';

import { TabStripComponent } from "@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component";

import { ComponentInfo, ComponentInfoService } from './../shared/models/component-info.model';
import { MessageDlgArgs } from './../shared/models';

import { MessageDialogComponent } from './../shared/containers/message-dialog/message-dialog.component';
import { EnumsService } from './../core/services/enums.service';
import { TabberService } from './../core/services/tabber.service';
import { LayoutService } from './../core/services/layout.service';
import { ComponentService } from './../core/services/component.service';
import { LoginSessionService } from './../core/services/login-session.service';
import { SidenavService } from './../core/services/sidenav.service';
import { SettingsService } from './../menu/services/settings.service';
import { LocalizationService } from './../menu/services/localization.service';
import { MenuService } from './../menu/services/menu.service';

@Component({
  selector: 'tb-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  encapsulation: ViewEncapsulation.None,
  providers:[ComponentInfoService]
})
export class HomeComponent implements OnDestroy, AfterContentInit {

  @ViewChild('sidenav') sidenav;
  subscriptions: Subscription[] = [];

  @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;
  @ViewChild('tabberContainer') tabberContainer: ElementRef;
  @ViewChild(MessageDialogComponent) messageDialog: MessageDialogComponent;
  viewHeight: number;

  constructor(
    private sidenavService: SidenavService,
    private loginSession: LoginSessionService,
    private componentService: ComponentService,
    private layoutService: LayoutService,
    private tabberService: TabberService,
    private menuService: MenuService,
    private localizationService: LocalizationService,
    private settingsService: SettingsService,
    private enumsService: EnumsService,
  ) {


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
    this.localizationService.loadLocalizedElements(true);
    this.settingsService.getSettings();
    this.enumsService.getEnumsTable();

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
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  closeTab(info: ComponentInfo) {
    event.stopImmediatePropagation();
    info.document.close();
  }

}
