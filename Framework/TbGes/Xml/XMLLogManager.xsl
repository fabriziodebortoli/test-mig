<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1">
  <xsl:template match="/">
	<html>
	  <script>
		function ShowAll()
		{
		var obj = document.getElementById('Mostra');
		obj.bShow = !eval(obj.bShow);
		obj.value = obj.bShow ? obj.expandText : obj.collapseText;

		var nodes = document.getElementsByTagName('td');
		for (i=0; i!=nodes.length; i++)
		{
		if(nodes[i].name == 'MessageNode')
		{
		nodes[i].isVisible = obj.bShow;
		nodes[i].fireEvent('onclick');
		}
		}
		obj.scrollIntoView(true);
		}

		function Filter()
		{
		var bError = document.getElementById('Error').checked;
		var bWarning = document.getElementById('Warning').checked;
		var bInfo = document.getElementById('Info').checked;

		var nodes = new Array();
		GetDescendantsByName(document.documentElement, 'MessageLine', nodes)
		for (i=0; i!=nodes.length; i++)
		{
		var n = nodes[i];
		if (typeof(n.getAttribute) == "undefined")
		continue;
		switch(n.getAttribute('type'))
		{
		case 'Info':
		displayObject(n, bInfo);
		break;
		case 'Warning':
		displayObject(n, bWarning);
		break;
		case 'Error':
		displayObject(n, bError);
		break;
		}
		}
		delete nodes;

		nodes = new Array();
		GetDescendantsByName(document.documentElement, 'MessageGroup', nodes)
		for (j=0; j!=nodes.length; j++)
		ShowGroupNode(nodes[j]);
		delete nodes;

		event.srcElement.scrollIntoView(true);
		}

		function GetDescendantsByName(obj, aName, aList)
		{
		if(!aName || !aList || !obj) return;

		var nodes = obj.childNodes;
		for (var i=0; i!=nodes.length; i++)
		{
		var n = nodes[i];
		if (typeof(n.getAttribute) == "undefined")
		continue;
		if (n.getAttribute('name') == aName)
		aList.push(n);

		GetDescendantsByName(n, aName, aList);
		}
		}

		function ShowGroupNode(obj)
		{
		displayObject(obj, hasVisibleChilds(obj))
		}

		function hasVisibleChilds(obj)
		{
		if(!obj) return false;

		var nodes = obj.childNodes;
		for (var i=0; i!=nodes.length; i++)
		{
		if(nodes[i].name=='MessageLine')
		if(nodes[i].isVisible)
		return true;

		if(hasVisibleChilds(nodes[i]))
		return true;
		}

		return false;
		}

		function ShowNode(obj)
		{
		obj.isVisible = !eval(obj.isVisible);
		if(obj.isVisible)
		{
		displayObject(obj.parentNode.childNodes[3],false);
		displayObject(obj.parentNode.childNodes[5], true);
		obj.innerText='-';
		}
		else
		{
		displayObject(obj.parentNode.childNodes[3], true);
		displayObject(obj.parentNode.childNodes[5], false);
		obj.innerText='+';
		}
		}

		function displayObject(obj, bShow)
		{
		if(bShow)
		{
		obj.style.display='block';
		obj.style.visible='true';
		}
		else
		{
		obj.style.display='none';
		obj.style.visible='false';
		}

		obj.isVisible = bShow;
		}

	  </script>
	  <body onload="ShowAll();">
		<h2 style="text-align:center">
		  <xsl:value-of select="//LocalizedStrings/@messageList"/>
		</h2>
		<xsl:apply-templates/>
		<P/>
		<div align="center">

		  <table style="width:100%">
			<tr>
			  <td style="width:10%">
				<table border="1" style="border-color:#000000; border-width:1;">
				  <tr style="font-family:Arial; font-size: 12px;">
					<xsl:value-of select="//LocalizedStrings/@messageFilter"/>
				  </tr>
				  <tr style="font-family:Wingdings; font-size: 20px; font-weight:bolder;">
					<td style="color: blue; background-color: lime">
					  <INPUT id="Info" checked="true" type="checkbox" onclick="Filter();"/>J
					</td>
					<td style="color: blue; background-color: yellow">
					  <INPUT id="Warning" checked="true" type="checkbox" onclick="Filter();"/>K
					</td>
					<td style="color: blue; background-color: red">
					  <INPUT id="Error" checked="true" type="checkbox" onclick="Filter();"/>L
					</td>
				  </tr>
				</table>
			  </td>
			  <td style="width:90%; text-align:center">
				<INPUT id="Chiudi" type="button" onclick="window.close();">
				  <xsl:attribute name="value">
					<xsl:value-of select="//LocalizedStrings/@close"/>
				  </xsl:attribute>
				</INPUT>
				<INPUT bShow="false" id="Mostra" type="button" onclick="ShowAll();" >
				  <xsl:attribute name="expandText">
					<xsl:value-of select="//LocalizedStrings/@expandText"/>
				  </xsl:attribute>
				  <xsl:attribute name="collapseText">
					<xsl:value-of select="//LocalizedStrings/@collapseText"/>
				  </xsl:attribute>
				</INPUT>
			  </td>
			</tr>
		  </table>
		</div>
	  </body>
	</html>
  </xsl:template>

  <xsl:template match="Messages">
	<xsl:if test="@Previous">
	  <h3 style="text-align:center; color:red">
		<xsl:value-of select="//LocalizedStrings/@previousFile"/>
		<a>
		  <xsl:attribute name="href">
			<xsl:value-of select="@Previous"/>
		  </xsl:attribute>
		  <xsl:value-of select="@Previous"/>
		</a>
	  </h3>
	</xsl:if>
	<div align="center">
	  <table width="100%" border="1" style="background-color: silver; font-size: 12px; " >
		<xsl:if test="@User">
		  <tr>
			<td colspan="3">
			  <B>
				<xsl:value-of select="//LocalizedStrings/@user"/>
				<xsl:value-of select="@User"/>
			  </B>
			</td>
		  </tr>
		</xsl:if>
		<tr style="text-align:center">
		  <td>
			<B>
			  <xsl:value-of select="//LocalizedStrings/@type"/>
			</B>
		  </td>
		  <td>
			<B>
			  <xsl:value-of select="//LocalizedStrings/@timestamp"/>
			</B>
		  </td>
		  <td>
			<B>
			  <xsl:value-of select="//LocalizedStrings/@message"/>
			</B>
		  </td>
		</tr>
		<xsl:for-each select="Message">
		  <tr name="MessageLine">
			<xsl:attribute name="type">
			  <xsl:value-of select="@Type"/>
			</xsl:attribute>
			<td style="text-align:center; font-family:Wingdings; font-size: 26px; font-weight:bolder;">
			  <xsl:choose>
				<xsl:when test="@Type[.='Info']">
				  <div style="color: blue; background-color: lime">J</div>
				</xsl:when>
				<xsl:when test="@Type[.='Error']">
				  <div style="color: blue; background-color: red">L</div>
				</xsl:when>
				<xsl:when test="@Type[.='Warning']">
				  <div style="color: blue; background-color: yellow">K</div>
				</xsl:when>
			  </xsl:choose>
			</td>
			<td>
			  <xsl:value-of select="@Timestamp"/>
			</td>
			<td>
			  <xsl:value-of select="node()"/>
			</td>
		  </tr>
		  <xsl:apply-templates select="Detail"/>
		  <xsl:if test="Messages">
			<tr name="MessageGroup">
			  <td
				  name="MessageNode"
				  style="text-align:center; vertical-align:top; font-size: 26px; font-weight:bolder;"
				  onclick="ShowNode(this);">+</td>
			  <td colspan="2">
				<xsl:value-of select="//LocalizedStrings/@details"/>
			  </td>
			  <td colspan="2" style="display:none; visible:false;">
				<xsl:apply-templates select="Messages"/>
			  </td>
			</tr>
		  </xsl:if>
		</xsl:for-each>
	  </table>
	</div>
	<xsl:if test="@Next">
	  <h3 style="text-align:center; color:red">
		<xsl:value-of select="//LocalizedStrings/@nextFile"/>
		<a>
		  <xsl:attribute name="href">
			<xsl:value-of select="@Next"/>
		  </xsl:attribute>
		  <xsl:value-of select="@Next"/>
		</a>
	  </h3>
	</xsl:if>
  </xsl:template>

  <xsl:template match="Detail">
	<tr name="MessageLine">
	  <xsl:attribute name="type">
		<xsl:value-of select="@Type"/>
	  </xsl:attribute>
	  <td colspan="2"> </td>
	  <td >
		<xsl:value-of select="."/>
	  </td>
	</tr>
  </xsl:template>

</xsl:stylesheet>
