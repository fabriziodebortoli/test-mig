import { NgModule } from '@angular/core';

import { DialogModule, WindowModule } from '@progress/kendo-angular-dialog';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { GridModule, GridComponent } from '@progress/kendo-angular-grid';
import { ChartsModule } from '@progress/kendo-angular-charts';
import { GaugesModule  } from '@progress/kendo-angular-gauges';

import '@progress/kendo-angular-intl/locales/bg/numbers';
import '@progress/kendo-angular-intl/locales/bg/calendar';
import '@progress/kendo-angular-intl/locales/de-CH/numbers';
import '@progress/kendo-angular-intl/locales/de-CH/calendar';
import '@progress/kendo-angular-intl/locales/de/numbers';
import '@progress/kendo-angular-intl/locales/de/calendar';
import '@progress/kendo-angular-intl/locales/el/numbers';
import '@progress/kendo-angular-intl/locales/el/calendar';
import '@progress/kendo-angular-intl/locales/en/numbers';
import '@progress/kendo-angular-intl/locales/en/calendar';
import '@progress/kendo-angular-intl/locales/es-CL/numbers';
import '@progress/kendo-angular-intl/locales/es-CL/calendar';
import '@progress/kendo-angular-intl/locales/es/numbers';
import '@progress/kendo-angular-intl/locales/es/calendar';
import '@progress/kendo-angular-intl/locales/hu/numbers';
import '@progress/kendo-angular-intl/locales/hu/calendar';
import '@progress/kendo-angular-intl/locales/it-CH/numbers';
import '@progress/kendo-angular-intl/locales/it-CH/calendar';
import '@progress/kendo-angular-intl/locales/it/numbers';
import '@progress/kendo-angular-intl/locales/it/calendar';
import '@progress/kendo-angular-intl/locales/pl/numbers';
import '@progress/kendo-angular-intl/locales/pl/calendar';
import '@progress/kendo-angular-intl/locales/ro/numbers';
import '@progress/kendo-angular-intl/locales/ro/calendar';
import '@progress/kendo-angular-intl/locales/si/numbers';
import '@progress/kendo-angular-intl/locales/si/calendar';
import '@progress/kendo-angular-intl/locales/tr/numbers';
import '@progress/kendo-angular-intl/locales/tr/calendar';
import '@progress/kendo-angular-intl/locales/zh/numbers';
import '@progress/kendo-angular-intl/locales/zh/calendar';
import '@progress/kendo-angular-intl/locales/hr/numbers';
import '@progress/kendo-angular-intl/locales/hr/calendar';
import '@progress/kendo-angular-intl/locales/pt/numbers';
import '@progress/kendo-angular-intl/locales/pt/calendar';
import '@progress/kendo-angular-intl/locales/fr/numbers';
import '@progress/kendo-angular-intl/locales/fr/calendar';
import '@progress/kendo-angular-intl/locales/sr/numbers';
import '@progress/kendo-angular-intl/locales/sr/calendar';

import '@progress/kendo-angular-intl/locales/sr-Cyrl/numbers';
import '@progress/kendo-angular-intl/locales/sr-Cyrl/calendar';

import '@progress/kendo-angular-intl/locales/sr-Latn/numbers';
import '@progress/kendo-angular-intl/locales/sr-Latn/calendar';

import '@progress/kendo-angular-intl/locales/sl/numbers';
import '@progress/kendo-angular-intl/locales/sl/calendar';

import '@progress/kendo-angular-intl/locales/ru/numbers';
import '@progress/kendo-angular-intl/locales/ru/calendar';

const KENDO_UI_MODULES = [
    GridModule,
    GaugesModule,
    ChartsModule,
    WindowModule,
    DialogModule,
    DateInputsModule,
    DropDownsModule,
    InputsModule,
    LayoutModule,
    PopupModule,
    ButtonsModule
];

@NgModule({
    imports: [KENDO_UI_MODULES],
    exports: [KENDO_UI_MODULES]
})
export class TbKendoModule { }