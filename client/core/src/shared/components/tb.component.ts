import { InfoService } from './../../core/services/info.service';
import { TbComponentService } from './../../core/services/tbcomponent.service';
import { Input, OnInit, ChangeDetectorRef } from '@angular/core';

export abstract class TbComponent implements OnInit {
  @Input()
  public cmpId = '';

  public dictionaryId = '';
  public translations = [];

  constructor(
    public tbComponentService: TbComponentService,
    protected changeDetectorRef: ChangeDetectorRef) {
    this.dictionaryId = "";
  }
  protected enableLocalization() {
    this.dictionaryId = this.tbComponentService.calculateDictionaryId(this);
  }
  _TB(baseText: string, ...args: any[]) {
    return this.tbComponentService.translate(this.translations, baseText, args);
  }
  ngOnInit() {
    if (this.dictionaryId) {
      const ids = this.dictionaryId.split('.');
      ids.forEach(id => {
        const subs = this.tbComponentService.readFromLocal(id).subscribe(tn => {
          if (subs) {
            subs.unsubscribe();
          }

          if (tn) {
            this.translations = this.translations.concat(tn);
            this.changeDetectorRef.detectChanges();
            this.onTranslationsReady();
          } else {
            this.readTranslationsFromServer(id);
          }
        });
      });
    }
  }
  protected onTranslationsReady() {

  }
  public readTranslationsFromServer(dictionaryId: string) {
    const subs = this.tbComponentService.readTranslationsFromServer(dictionaryId).subscribe(
      tn => {
        if (subs) {
          subs.unsubscribe();
        }
        if (tn) {
          this.translations = this.translations.concat(tn);
          this.changeDetectorRef.detectChanges();
          this.onTranslationsReady();
        }
        this.translations = tn;
        this.tbComponentService.saveToLocal(dictionaryId, this.translations);
      },
      err => {
        if (subs) {
          subs.unsubscribe();
        }
        //dictionary file may not exist on server
        if (err && err.status === 404) {
          this.tbComponentService.saveToLocal(dictionaryId, []);
        }
      });
  }
}
