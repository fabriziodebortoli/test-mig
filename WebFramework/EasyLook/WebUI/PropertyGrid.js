
function PropertyGrid() {
	var tb$ = jQuery.noConflict();
	var _me = this;
	//output che vine incollato al Wiget padre
	_propertyGridOutput = null;

	


	PropertyGrid.prototype.create = function (parent) {
		//Crea area di disegno
		_propertyGridOutput.appendTo(parent);
	}

	PropertyGrid.prototype.fillAndRender = function (object) {
		_propertyGridOutput = getPropertyGrid(object);
		
	}

	function getPropertyGrid(obj) {
		//creates the html output from properties Object
		var table = tb$('<table class="pgGridContainer"><tbody/></table>');

		function createItem(propName, propValue) {
			try {
				var row = tb$('<tr class="pgGroupItem"><td class="pgGroupItemName">' + propName + '</td><td class="pgGroupItemValue" /></tr>');
				tb$('tbody:first', table).append(row);
				var itemValue = tb$('.pgGroupItemValue', row);

				if (typeof propValue != 'object') {
					tb$('<input value="' + propValue + '" class="pgInput"></input></div>')
					.appendTo(itemValue)
					.each(function () { this.ownerObj = obj; this.propName = propName; })
					.change(function () {
						this.ownerObj[this.propName] = typeof(this.ownerObj[this.propName]) == 'number' ? Number(this.value) : this.value;
						this.ownerObj.objectRoot.refresh();
					});
				}
				else
					itemValue.append(getPropertyGrid(propValue));
			}
			catch (e) { 
			
			}
		}

		for (var propName in obj) {
			var propVal = obj[propName];
			if (isVisibleProperty()) {
				createItem(propName, propVal);
			}
		}

		function isVisibleProperty() {
			return typeof propVal != 'function' && propName != "objectRoot";
		}

		return table;
	}
}