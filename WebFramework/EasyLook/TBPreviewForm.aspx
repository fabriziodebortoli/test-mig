<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TBPreviewForm.aspx.cs" Inherits="Microarea.Web.EasyLook.TBPreviewForm" %>
<%@ Register Assembly="Microarea.TaskBuilderNet.UI" Namespace="Microarea.TaskBuilderNet.UI.TBWebFormControl"
	TagPrefix="cc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <script type="text/javascript">
        function removeAllEventHandler(element) {

            element.removeAttribute("onfocus");
            element.onfocus = function() { return false; }
            
            element.removeAttribute("onclick");
            element.onclick = function() { return false; }
            
            element.removeAttribute("onkeydown");
            element.onkeydown = function() { return false; }
            
            element.removeAttribute("onkeypress");
            element.onkeypress = function() { return false; }
            
            element.removeAttribute("onkeyup");
            element.onkeyup = function() { return false; }
            
            element.removeAttribute("onmouseover");
            element.onmouseover = function() { return false; }

            element.removeAttribute("onchange");
            element.onchange = function() { return false; }

            element.removeAttribute("ondblclick");
            element.ondblclick = function() { return false; }

            element.removeAttribute("onmousedown");
            element.onmousedown = function() { return false; }

            element.removeAttribute("onmousemove");
            element.onmousemove = function() { return false; }

            element.removeAttribute("onmouseout");
            element.onmouseout = function() { return false; }

            element.removeAttribute("onmouseup");
            element.onmouseup = function() { return false; }

            element.removeAttribute("onselect");
            element.onselect = function() { return false; }

            element.removeAttribute("onsubmit");
            element.onsubmit = function() { return false; }
        }

        function doNothing(){
        }
        
        function makeReadOnly(obj) {
            
            __doPostBack == doNothing;
            
            if (obj == null)
                return;

            if (obj.nodeType == 1) {
                removeAllEventHandler(obj);
            }

            if (obj.hasChildNodes() && obj.nodeType == 1 /*element_node*/) {
                for (var i = 0; i < obj.childNodes.length; i++) {
                    var child = obj.childNodes[i];
                    if (child != null && child.nodeType == 1) /*element_node*/
                        removeAllEventHandler(child);

                    makeReadOnly(child);
                }
            }
        }
    </script>
</head>
<body style="background-image:url(Files/Images/SnapshotBackground.png); background-repeat:repeat" onload = "makeReadOnly(document.getElementById('wcfSnapshot'));">
    <form id="formPreview" runat="server" style="width: 100%; height: 100%">	
		<asp:ScriptManager ID="ScriptManager2" runat="server" >
		</asp:ScriptManager>
		<cc1:TBWebFormControl ID="wcfSnapshot" runat="server" OwnerApplication="WebFramework" OwnerModule="EasyLook"/>
	</form>
	
<div  style="position:absolute; z-index:500; background-color:#99CCFF; top:0px; left:0px; filter: alpha(opacity=10);-moz-opacity: .10; width: 100%; height: 100%;"/>

</body>
</html>