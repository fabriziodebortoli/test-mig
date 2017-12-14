Ext.define('UndoHelper', {

    singleton: true,

    mixins: {
        observable: 'Ext.mixin.Observable'
    },

    // undo state array 
    undoStateHistory: [],
    redoStateHistory: [],
    currentState: undefined,

    constructor: function () {        
        this.mixins.observable.constructor.call(this);        
    },

    addNewState: function (oNewState, aoSelected) {
        if (this.currentState) {
            // store selected items.
            this.currentState.selected = aoSelected;
            this.undoStateHistory.push(this.currentState);
            this.redoStateHistory = [];
        }
        if (!oNewState) {
            oNewState = window.rootObject;
        }
        this.currentState = Ext.clone(oNewState);
        this.fireEvent(EVENT_UNDO_REDO, null, null);
    },

    undo: function (aoSelected) {
        if (this.undoStateHistory.length) {
            this.currentState.selected = aoSelected;
            this.redoStateHistory.push(this.currentState);            
            this.currentState = this.undoStateHistory.pop();
            // set selected items back
            var aoSelected = this.currentState.selected;
            delete this.currentState.selected;
            this.fireEvent(EVENT_UNDO_REDO, Ext.clone(this.currentState), aoSelected);
        }
    },

    redo: function (aoSelected) {
        if (this.redoStateHistory.length) {
            this.currentState.selected = aoSelected;
            this.undoStateHistory.push(this.currentState);
            this.currentState = this.redoStateHistory.pop();
            // set selected items back           
            var aoSelected = this.currentState.selected;
            delete this.currentState.selected;
            this.fireEvent(EVENT_UNDO_REDO, Ext.clone(this.currentState), aoSelected);
        }
    },

    getRedoLength: function () {
        return this.redoStateHistory.length;
    },

    getUndoLength: function () {
        return this.undoStateHistory.length;
    }
});