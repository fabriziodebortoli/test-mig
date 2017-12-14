<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DocumentForm.aspx.cs" Inherits="Microarea.Web.EasyLook.WebUI.DocumentForm" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

    <link href="DocumentDesigner.css" rel="stylesheet" type="text/css">
    <link href="jquery-ui.css" rel="stylesheet" type="text/css">

    <script src="jquery.min.js" type="text/javascript"></script>
    <script src="jquery-ui.min.js" type="text/javascript"></script>
    <script src="PropertyGrid.js" type="text/javascript"></script>
    
    <script src="WndObjType.js" type="text/javascript"></script>
    <script src="Document.js" type="text/javascript"></script>
    <script src="DocumentRenderObject.js" type="text/javascript"></script>
    

    <script type="text/javascript">
        var tb$ = jQuery.noConflict();
        
        tb$(document).ready(function () {
            // get the first document
            getDocumentData();
        }); 

    </script> 
</head>

<body>
    <form id="form1" runat="server">
        
        <div class="container">  
            <div class="sidebar">
                <div class="document">
			    </div>
		    </div>
            
            <div class="content">
            </div>
	    </div>

        <div class="messagebox">
        </div>


		<div class="messageboxModal">
        </div>

        <div class="messageboxUpModal">
        </div>

        

    </form>
</body>
</html>
