import { NgModule, ModuleWithProviders } from '@angular/core';

import { TbIconsModule } from './icons/icons.module';
export * from './icons/icons.module';

const TB_MODULES = [
  TbIconsModule
];

import {
  BOService, BOHelperService, ComponentService, CoreGuard, DataService, DocumentService, EnumsService, EventDataService,
  ExplorerService, HttpService, InfoService, LayoutService, Logger, LoginSessionService, OperationResult, SidenavService,
  TabberService, UtilsService, WebSocketService
} from './services';

export * from './services/logger.service';

const TB_SERVICES = [
  BOService, BOHelperService, ComponentService, CoreGuard, DataService, DocumentService, EnumsService, EventDataService,
  ExplorerService, HttpService, InfoService, LayoutService, Logger, LoginSessionService, OperationResult, SidenavService,
  TabberService, UtilsService, WebSocketService
];

@NgModule({
  imports: [TB_MODULES],
  exports: [TB_MODULES]
})
export class TaskbuilderCoreModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: TaskbuilderCoreModule,
      providers: [TB_SERVICES]
    };
  }
}
