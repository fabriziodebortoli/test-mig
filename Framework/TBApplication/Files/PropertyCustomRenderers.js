function getStyleValue(value, oData, sPropName) {
    var oStyled = value;
    if (DefaultConfigHelper.isReadOnly(oData, sPropName)) {
        oStyled = "<div style=\"background-color:lightgrey\">" + value + "</div>";
    }
    return oStyled;
};

function readOnlyRenderer(value, oData, sProp) {
    return getStyleValue(value, oData, sProp);
};

function textAlign(v) {
    switch (v) {
    case Alignment_Left.value:
        return Alignment_Left.text;
    case Alignment_Center.value:
        return Alignment_Center.text;
    case Alignment_Right.value:
        return Alignment_Right.text;
    }
    return v;
};

function comboType(ct) {
    switch (ct) {
    case comboType_Simple.value:
        return comboType_Simple.text;
    case comboType_Dropdown.value:
        return comboType_Dropdown.text;
    case comboType_DropdownList.value:
        return comboType_DropdownList.text;
    }

    return ct;
};

function vertAlign(v) {
    switch (v) {
    case Vertical_Alignment_Top.value:
        return Vertical_Alignment_Top.text;
    case Vertical_Alignment_Center.value:
        return Vertical_Alignment_Center.text;
    case Vertical_Alignment_Bottom.value:
        return Vertical_Alignment_Bottom.text;
    }
    return v;
};

function ownerDraw(value, oData, sProp) {

    var objectType = getTypeDescription(oData.type);

    if (objectType === 'Combo') {
        var out = null;
        switch (value) {
        case ownerDraw_No.value:
            out = ownerDraw_No.text;
        case ownerDraw_Fixed.value:
            out = ownerDraw_Fixed.text;
        case ownerDraw_Variable.value:
            out = ownerDraw_Variable.text;
        }
        return out;
    }

    return value.toString();
};

function selection(value, oData, sProp) {
    switch (value) {
    case selection_Single.value:
        return selection_Single.text;
    case selection_Multiple.value:
        return selection_Multiple.text;
    case selection_Extended.value:
        return selection_Extended.text;
    case selection_None.value:
        return selection_None.text;
    }
    return sel;
};