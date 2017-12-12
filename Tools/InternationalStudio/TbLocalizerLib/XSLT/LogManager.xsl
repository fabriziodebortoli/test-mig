<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <xsl:template match="/">
    <script>
      <xsl:text disable-output-escaping="yes">
  <![CDATA[

  function Filter()
  {
  var bError = document.getElementById('Error').checked;
  var bWarning = document.getElementById('Warning').checked;
  var bInfo = document.getElementById('Information').checked;

  var nodes = new Array();
  GetDescendantsByName(document.documentElement, 'messageline', nodes)
  for (i=0; i!=nodes.length; i++)
  {
  switch(nodes[i].getAttribute('type'))
  {
  case 'info':
  displayObject(nodes[i], bInfo);
  break;
  case 'warning':
  displayObject(nodes[i], bWarning);
  break;
  case 'error':
  displayObject(nodes[i], bError);
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
  if(n.getAttribute && n.getAttribute("name") == aName)
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
  displayObject(obj.parentNode.childNodes(1),false);
  displayObject(obj.parentNode.childNodes(2), true);
  obj.innerText='-';
  }
  else
  {
  displayObject(obj.parentNode.childNodes(1), true);
  displayObject(obj.parentNode.childNodes(2), false);
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

]]>
</xsl:text>
    </script>
    <html>
      <head>
        <style type="text/css">
          body {font-size:12px; font-family:verdana; color:black; text-align:justify;}
          p {font-size:12px; font-family:verdana; color:black; text-align:justify;}
          h2 {font-size:18px; font-family:verdana; color:black; font-weight:bolder; text-align:center;}
          table {background-image:url('../img/BackGround.jpg'); }
          td {font-size:10px; font-family:verdana; color:black;}
        </style>
      </head>
      <body>
        <h2>---Log Messages---</h2>
        <xsl:apply-templates/>
        <P/>
        <div align="center">

          <table style="width:100%">
            <tr>
              <td style="width:10%">
                <table border="1" style="border-color:#000000; border-width:1;">
                  <tr style="font-family:Arial; font-size: 12px;">Filter messages</tr>
                  <tr style="font-family:Wingdings; font-size: 20px; font-weight:bolder;">
                    <td style="font-family:Wingdings; font-size: 20px; font-weight:bolder; color: blue; background-color: lime">
                      <INPUT id="Information" checked="true" type="checkbox" onclick="Filter();"/>J
                    </td>
                    <td style="font-family:Wingdings; font-size: 20px; font-weight:bolder; color: blue; background-color: yellow">
                      <INPUT id="Warning" checked="true" type="checkbox" onclick="Filter();"/>K
                    </td>
                    <td style="font-family:Wingdings; font-size: 20px; font-weight:bolder; color: blue; background-color: red">
                      <INPUT id="Error" checked="true" type="checkbox" onclick="Filter();"/>L
                    </td>
                  </tr>
                </table>
              </td>
              <td style="width:90%; text-align:center">
                <INPUT id="Close" type="button" value="Close" onclick="window.close();"/>
              </td>
            </tr>
          </table>
        </div>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="messages">
    <div align="center">
      <table width="100%" border="1" style="background-color: silver; font-size: 12px; " >
        <xsl:if test="@user">
          <tr>
            <td colspan="3">
              <B>
                User: <xsl:value-of select="@user"/>
              </B>
            </td>
          </tr>
        </xsl:if>
        <tr style="text-align:center">
          <th>Type</th>
          <th>Timestamp</th>
          <th>Message</th>
        </tr>
        <xsl:for-each select="message">
          <tr name="messageline">
            <xsl:attribute name="type">
              <xsl:value-of select="@type"/>
            </xsl:attribute>
            <td style="text-align:center; font-family:Wingdings; font-size: 26px;">
              <xsl:choose>
                <xsl:when test="@type[.='info']">
                  <div style="color: blue; background-color: lime">J</div>
                </xsl:when>
                <xsl:when test="@type[.='error']">
                  <div style="color: blue; background-color: red">L</div>
                </xsl:when>
                <xsl:when test="@type[.='warning']">
                  <div style="color: blue; background-color: yellow">K</div>
                </xsl:when>
              </xsl:choose>
            </td>
            <td>
              <xsl:value-of select="@datetime"/>
            </td>
            <td>
              <xsl:value-of select="@text"/>
            </td>
          </tr>
          <xsl:apply-templates select="detail"/>
          <xsl:apply-templates select="response"/>
          <xsl:if test="messages">
            <tr name="messagegroup">
              <td
                name="messagenode"
                style="text-align:center; vertical-align:top; font-size: 26px; font-weight:bolder;"
                onclick="ShowNode(this);">+</td>
              <td colspan="2">Dettagli...</td>
              <td colspan="2" style="display:none; visible:false;">
                <xsl:apply-templates select="messages"/>
              </td>
            </tr>
          </xsl:if>
        </xsl:for-each>
      </table>
    </div>
  </xsl:template>

</xsl:stylesheet>
