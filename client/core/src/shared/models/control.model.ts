export function createEmptyModel(): any {
    return {
        _status: 0,
        _value: null,
        _type: 0
    };
}
export function addModelBehaviour(model: any) {
    if (model._status) {//solo se è un dataobj
        addControlModelBehaviour(model);
    } else {
        for (const prop in model)
        addModelBehaviour(model[prop]);
    }
}
export function addControlModelBehaviour(model: any) {

    Object.defineProperty(model, "value", {
        get: function enabled(): any {
            return this._value;
        },
        set: function enabled(val: any) {
            this._value = val;
        }
    });

    Object.defineProperty(model, "type", {
        get: function enabled(): number {
            return this._type;
        }
    });

    Object.defineProperty(model, "length", {
        get: function enabled(): number {
            return this._length;
        }
    });
    Object.defineProperty(model, "enabled", {
        get: function enabled(): boolean {
            const flags = DataStatus.READONLY | DataStatus.OSL_READONLY | DataStatus.ALWAYS_READONLY | DataStatus.BPM_READONLY;
            return ((this._status & flags) == 0) || ((this._status & DataStatus.ALWAYS_EDITABLE) != 0);
        },
        set: function enabled(val: boolean) {
            this.setStatus(!val, DataStatus.READONLY);

            if (!val)
                this.setStatus(false, DataStatus.FINDABLE);
        }
    });

    Object.defineProperty(model, "uppercase", {
        get: function uppercase(): boolean {
            return (this._status & DataStatus.UPPERCASE) == DataStatus.UPPERCASE;
        }
    });
    model.setStatus = function (bSet: boolean, aStatusFlag: number) {
        this._status = bSet ? this._status | aStatusFlag : this._status & ~aStatusFlag;
    }
}


enum DataStatus {
    DBCASE_COMPLIANT = 0x0001,
    UPPERCASE = 0x0002,

    FULLDATE = 0x0004,	// usato come attributo del DATA_DATE_TYPE 
    // per indicare l'uso del Time (DataType::DateTime)
    TIME = 0x0008,	// usato come attributo del DATA_DATE_TYPE 
    // per indicare che e` un Ora 
    // usato come attributo del DATA_LONG_TYPE
    // per indicare che e` un Tempo
    ACCOUNTABLE = 0x0008,   // usato nel DATA_MON_TYPE per definire se l'importo è Accountable o Not Accountable
    TB_HANDLE = 0x0004,	// usato come attributo del DATA_LONG_TYPE (DataType::Long) 
    // per indicare che il contenuto è un handle (DataType::Object)
    TB_VOID = 0x0008,	// usato come attributo del DATA_NULL_TYPE (DataType::Null)
    // per indicare un valore di ritorno void (DataType::Void)

    READONLY = 0x00010,	// per gestire il readonly nei controls dipendentemente allo stato del documento
    HIDE = 0x00020,	// per gestire il hide/show dei controls
    FINDABLE = 0x00040,	// abilita la ricerca nei documenti
    VALUE_CHANGED = 0x00080,	// riservato ed utilizzabile dal programmatore
    VALID = 0x00100,	// usato dal report engine
    MODIFIED = 0x00200,	// riservato dalla gestione interna del documento
    DIRTY = 0x00400,	// usato per ottimizzare i/o su database
    UPDATE_VIEW = 0x00800,	// usato per forzare la rivisualizzazione del dato
    OSL_READONLY = 0x01000,	// OSL: per gestire il readonly nei controls
    OSL_HIDE = 0x02000,	// OSL: per gestire il hide/show dei controls
    ALWAYS_READONLY = 0x04000,	// per gestire il readonly nei controls indipendentemente dallo stato del documento
    VALUE_LOCKED = 0x08000,	// per impedire l'assegnazione di un nuovo valore al DataObj
    PRIVATE = 0x10000,   // è un dato privato. Per la visualizzazione viene utilizzato il formattatore CPrivacyFormatter //utilizzato dalla RowSecurity.
    // @@BAUZI: non ho potuto usare OSL_HIHE poichè legato al control e non al dato come invece serve per la RowSecurity
    ALWAYS_EDITABLE = 0x20000,	// @@PERA: per rendere il campo editabile anche se il documento è in stato di browse
    WEB_BOUND = 0x40000,	// @@PERA: per segnalare che il campo è utilizzato nell'interfaccia web e quindi deve essere inserito nel model
    BPM_READONLY = 0x80000	// readonly perchè gestito da processi di BPM
};