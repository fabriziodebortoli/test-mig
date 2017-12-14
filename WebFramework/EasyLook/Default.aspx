<%@ Page Language="c#" CodeBehind="Default.aspx.cs" AutoEventWireup="True" Inherits="Microarea.Web.EasyLook.Default" %>

<!DOCTYPE html 
PUBLIC "-//W3C//DTD XHTML 1.0 Frameset//EN" 
"http://www.w3.org/TR/xhtml1/DTD/xhtml1-frameset.dtd">
<html>
<head runat="server">
    <title></title>
    <meta content="C#" name="CODE_LANGUAGE" />
    <meta content="JavaScript" name="vs_defaultClientScript" />
    <meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema" />
    <meta http-equiv="X-UA-Compatible" content="IE=9" />
    <link rel="shortcut icon" href="Files/Images/EasyLookShortcut.ico" />
    <script type="text/javascript" language="javascript">
		var selectedObj = null;
		var openedChangeApplicationDate = null;
		var openedSearch = null;
		var isSearchPostBak = false;
		var openedDetailsForm = null;

		function SetLink(link) {
			selectedObj.href = link;
		}

		/*funzione che mi dice se il thread ha un corrispondente tbwebformControl 
		che lo renderizza sul browser*/
		function getBrowserWindow(threadID) {
		    if (topFrame.getBrowserWindow)
		        return topFrame.getBrowserWindow(threadID);
		    return null;
		}

		//funzione che chiude la finestra del browser associata a quel thread, se presente
		function closeBrowserWindow(threadID) {
			var docWindow = getBrowserWindow(threadID);
			if (docWindow != null)
				docWindow.close();
		}

		/* funzione per riagganciare un thread(documento e suoi figli) che non e' renderizzato sul browser perche la sua finestra browser e' stata chiusa,
		oppure per spostarsi (dare il fuoco) sulla finestra di browser che lo renderizza*/
		function attachToDocument(threadID) {
			var docWindow = getBrowserWindow(threadID);
			if (docWindow != null)
				docWindow.focus();
			else {//se non aveva una corrispondente finestra di browser aperta eseguo il reattach
				var url = 'TBWindowForm.aspx?DocumentHandle=' + threadID;
				topFrame.linkDocument(url, null);
			}
		}

		/* funzione per aprire una finestra di browser che mostra uno snapshot (html non editabile) del thread (docuemnto e suoi figli).
		Usato per mostrare i documenti di altre login se l'utente e' amministratore*/
		function showDocumentSnapshot(threadID) {
			var docWindow = getBrowserWindow(threadID);
			if (docWindow != null)
				docWindow.focus();
			else {
				var url = 'TBPreviewForm.aspx?DocumentHandle=' + threadID;
				topFrame.linkDocument(url, null);
			}
		}

		//funzione per riagganciare piu' threads(documenti) che non sono renderizzati sul browser perche' la loro finestra browser e' stata chiusa,
		function attachToDocuments(documentHandles) {
			for (docHandleIdx in documentHandles) {
				var docHandle = documentHandles[docHandleIdx];
				var docWindow = getBrowserWindow(docHandle);
				if (docWindow == null && docHandle > 0) //eseguo l'attach solo se non ha gia una finestra di browser associata
					topFrame.linkDocument('TBWindowForm.aspx?DocumentHandle=' + docHandle, null);
			}
		}

		//funzione per chiudere la finestra di browser che sta renderrizzando il thread
		function closeBrowserDocument(threadID) {
			var docWindow = getBrowserWindow(threadID);
			if (docWindow != null)
				docWindow.close();
		}
		
		
		function OpenPopupDocument(docNamespace, notHooked) {
			var url = 'tbloader/document.html?ns=' + docNamespace + '&session=' + new Date().getTime() + '&notHooked=' + (typeof(notHooked) === 'boolean' && notHooked);
		    //var url = 'TBWindowForm.aspx?ObjectNamespace=' + docNamespace;
			topFrame.linkDocument(url, null);
		}

		function OpenPopUpNewReportFromFilename(name, params) {
			OpenPopupGenericReport("filename", name, params);
		}

		function OpenPopUpNewReport(name, params) {
			OpenPopupGenericReport("namespace", name, params);
		}

		function OpenPopupGenericReport(type, name, params) {
			var queryString;
			if (name == null || name == "")
				queryString = "WoormWebForm.aspx?CurrentReport=true";
			else
				queryString = "WoormWebForm.aspx?" + type + "=" + name;
				//queryString = "WebUI/ReportForm.aspx?" + type + "=" + name;
			
			topFrame.linkDocument(queryString, params); //this script function is injected by WoormWebControl in the Page_Load function of topFrame
		}

		function OpenPopupApiReport(name, params) {
		    var queryString;
		    queryString = "~/rs/api/" + name;

		    var windowName = 'RS api - ' + name;
		    var windowStyle = 'height=700,	width=1024, status=yes, scrollbars=yes, resizable=yes';
		

		    var docWindow = window.open(queryString, windowName, windowStyle);
		    if (docWindow == null) {
		        alert('Cannot open a new popup window. Check your browser settings and retry.');
		        return;
		    }
        }

		function CloseApplicationDate() {
			openedChangeApplicationDate = null;
		}

		function PostBack() {
			isSearchPostBak = true;
			if (openedSearch != null)
				openedSearch.resizeTo(1104, 425);
		}

		function ChangeApplicationDate() {
			if (openedChangeApplicationDate && openedChangeApplicationDate.closed === false) {
				openedChangeApplicationDate.focus()
				return;
			}

			openedChangeApplicationDate = window.open('ChangeApplicationDate.aspx', '', 'width=550px, height=320px, scrollbars=no, resizable=no');

			topFrame.addOpenDoc(openedChangeApplicationDate);

			if (!openedChangeApplicationDate.opener) openedChangeApplicationDate.opener = self;
			if (openedChangeApplicationDate.focus != null) openedChangeApplicationDate.focus();
		}

		function ShowDetails() {
			if (openedDetailsForm != null && openedDetailsForm.closed === false) {
				openedDetailsForm.focus()
				openedDetailsForm.__doPostBack();
				return;
			}

			openedDetailsForm = window.open('DocumentsDetailForm.aspx', '', 'width=900px, height=800px, scrollbars=yes, resizable=no');

			topFrame.addOpenDoc(openedDetailsForm);

			if (!openedDetailsForm.opener) openedDetailsForm.opener = self;
			if (openedDetailsForm.focus != null) {
				openedDetailsForm.focus();
			}
		}

		function SearchPopUp() {
			if (openedSearch != null && openedSearch.closed === false) {
				openedSearch.focus()
				return;
			}

			openedSearch = window.open('SearchPage.aspx', '', 'width=328px, height=400px, scrollbars=no, resizable=no');
			topFrame.addOpenDoc(openedSearch);

			if (!openedSearch.opener) openedSearch.opener = self;
			if (openedSearch.focus != null) openedSearch.focus();

        }
        function OpenPrivateArea() {
            window.open('PrivateAreaBridge.aspx');
        }

		function SelectCommand(url, urlReportPage, title, nameSpace, menuTitle, isReport) {
			parent.MenuArea.location.href = "MenuArea.aspx?NameSpace=" + url;
			parent.MenuReportArea.location.href = "MenuItemsPage.aspx?NameSpace=" + urlReportPage + "&Title=" + menuTitle;

			if (isReport == "False") {
				parent.HistoryArea.location.href = "OpenDocumentsPage.aspx?NameSpace=" + nameSpace + "&Title=" + title;
			}
			else {
				parent.HistoryArea.location.href = "HistoryPage.aspx?NameSpace=" + nameSpace + "&Title=" + title;
			}
			focus();
		}

		function CloseSearchPopUp() {
			if (isSearchPostBak == false)
				openedSearch = null;

			isSearchPostBak = false;
		}

		function PerformClosingOperations() {
			topFrame.PerformClosingOperations();
		}
		

		window.onunload = PerformClosingOperations;

    </script>
</head>
<frameset border="1" framespacing="0" rows="103px,*,22px" bordercolor="#000080" frameborder="yes"
    style="width: 100%; position: absolute">
		<frame border="2" id="topFrame" name="topFrame" src="TopPage.aspx" style = "width:100%; position:absolute" marginwidth="0" frameBorder="yes"
			noResize scrolling="no">
		<frameset id="ts" cols="269,346,*" scrolling="auto" framespacing="0" bordercolor="#000080"
			frameborder="no">
			<frame id="Tree" name="Tree" scrolling="auto" src="ApplicationPage.aspx" frameborder="no" width="40%"
				bordercolor="#000080"/>
			<frame id="MenuArea" name="MenuArea" scrolling="auto" src="MenuArea.aspx" frameborder="no" width="25%"
				DESIGNTIMEDRAGDROP="26"/>
			<frameset rows="50%,50%" id="ReportFrameset">
				<frame id="MenuReportArea" name="MenuReportArea" src="MenuItemsPage.aspx" frameborder="1" scrolling="auto"/>
				<frame id="HistoryArea" name="HistoryArea" src="tbloader/serverMonitor.html" frameborder="1" scrolling="yes"/>
			</frameset>
		</frameset>
		<frame frameborder="0" id="FooterArea" name="FooterArea" src="Footer.aspx" noresize="noResize" scrolling="no" DESIGNTIMEDRAGDROP="26"/>
	</frameset>
</html>
