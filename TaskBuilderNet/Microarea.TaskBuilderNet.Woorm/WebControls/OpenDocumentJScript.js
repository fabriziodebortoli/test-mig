
	if (!this.openedDocs)
		this.openedDocs = new Array();

    function linkDocument(action, parameters, menuBar) {													 
		var now = new Date();

		if (action.substring(0, 1) == '/')
			action = action.substring(1, action.length);

		var	windowName = 'doc' + now.getHours() + now.getMinutes() + now.getSeconds() + now.getMilliseconds(); 
		var windowStyle = 'height=700,	width=1024, status=yes, scrollbars=yes, resizable=yes';
		if (menuBar == 'true')
		    windowStyle += ', menubar=yes';
 
		var docWindow = window.open('', windowName, windowStyle);
		if (docWindow == null) {
		    alert('##PopupDeniedMessage##');
		    return;
		}
		
		addOpenDoc(docWindow);
		
		if (!docWindow.opener) docWindow.opener =	self;
		if (docWindow.focus != null)	docWindow.focus();
		
		var	form = docWindow.document.createElement('form');
		docWindow.document.body.appendChild(form);
		
		form.target	= windowName;
		
		var url = new String(window.location);
		var idx = url.lastIndexOf("/");
		url = url.substring(0, idx + 1);
		
		form.action	= url + action;
		form.method	= 'post';
		var	input =	docWindow.document.createElement('input');
		input.type = 'hidden';
		input.name = '##InputName##';
		input.value	= parameters;
		form.appendChild(input);
		
		var div = docWindow.document.createElement('div');
		div.id = "progressContent";
		div.style.backgroundColor = "#3560D0";
		div.style.padding = "0px";
		div.style.width = "0%";
		div.style.height = "10px";
		div.style.border = "1px solid";
		div.style.position = "absolute";
		div.style.top = "50%";
		div.style.left = "20%";
		div.style.visibility = "hidden";
		
		var borderDiv = docWindow.document.createElement('div');
		borderDiv.id = "progressBorder";
		borderDiv.style.padding = "0px";
		borderDiv.style.width = "60%";
		borderDiv.style.height = "10px";
		borderDiv.style.border = "1px solid";
		borderDiv.style.position = "absolute";
		borderDiv.style.top = "50%";
		borderDiv.style.left = "20%";
			
		var textDiv = docWindow.document.createElement('div');
		textDiv.style.position = "absolute";
		textDiv.style.fontFamily = "Verdana, Arial, Helvetica, sans-serif";
		textDiv.style.width = "60%";
		textDiv.style.top = "55%";
		textDiv.style.left = "20%";
		textDiv.style.textAlign = "center";
		textDiv.innerText = "##LoadingMessage##";

		docWindow.document.body.appendChild(div);
		docWindow.document.body.style.backgroundColor = "#E6E6FA";
		docWindow.document.body.className = "waitingWindow";
		docWindow.document.body.appendChild(borderDiv);
		docWindow.document.body.appendChild(textDiv);
		
		docWindow.progressDiv = div;
		docWindow.progressDiv.current = 0;
		docWindow.progressDiv.ticks = 0;
		var interval = docWindow.setInterval(function(){ updateProgress(div);}, 100);
		
		try {
		    form.submit();
		}
		catch(e)
		{			
		    alert(e);
		}
		return false; //avoids round-trip
	}
	
	function updateProgress(progressDiv)
	{
		if (progressDiv.current >= 60) return; 
		progressDiv.current = 60 - Math.pow(0.98, progressDiv.ticks) * 60; 
		progressDiv.ticks++; 
		progressDiv.style.width = progressDiv.current + '%'; 
		progressDiv.style.visibility = 'visible';
	}

	/*funzione che mi dice se il thread ha un corrispondente tbwebformControl 
	che lo renderizza sul browser*/
	function getBrowserWindow(threadID) {
		if (this.openedDocs == null)
			return;
		for (var i = 0; i < this.openedDocs.length; i++) {
			try {
				var doc = this.openedDocs[i];
				if (this.openedDocs[i].closed === false && doc.threadID == threadID)  //allora il documento aperto lato server ha una corrispondente finestra di browser
				{
					return doc;
				}
			}
			catch (ex) {
				continue;
			}
		}
		return null;
	}
	function closeOpenedDocs() {
		for (var i = 0; i < this.openedDocs.length; i++) {
			var doc = this.openedDocs[i];
			try
			{
				if (doc.closed === false)
					doc.close();
			}
			catch (ex) {
				continue;
			}
		}
	}
	function addOpenDoc(docWindow) {
		this.openedDocs.push(docWindow);
	}