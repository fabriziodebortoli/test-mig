<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="//File">
    <p style="font-family:Verdana; font-size: 18px;color: darkgreen;">
      Database Disconnections Log File On
      <xsl:value-of select="@creationdate"/>
      <table style="font-family:Verdana; font-size: 12px;">
      <xsl:apply-templates select="Messages/Message" />
      </table>
    </p>
  </xsl:template>
  <xsl:template match="Messages/Message">
    <xsl:if test="ExtendedInfos">
        <br></br>
        <xsl:apply-templates select="ExtendedInfos" />
      </xsl:if>
      <p style="font-family:Verdana; font-size: 12px; color: black">
        <tr>
          <td>
          </td>
          <td>
            <xsl:choose>
              <xsl:when test="@type[.='Information']">
                <div style="font-family:Verdana; font-size: 12px;color: darkgreen;">
                  [<xsl:value-of select="@time"/>]  <xsl:value-of select="@type"/>
                </div>
              </xsl:when>
              <xsl:when test="@type[.='Error']">
                <div style="font-family:Verdana; font-size: 12px;color: crimson;">
                  [<xsl:value-of select="@time"/>]  <xsl:value-of select="@type"/>
                </div>
              </xsl:when>
              <xsl:when test="@type[.='Warning']">
                <div style="font-family:Verdana; font-size: 12px;color: darkgoldenrod;">
                  [<xsl:value-of select="@time"/>]  <xsl:value-of select="@type"/>
                </div>
              </xsl:when>
            </xsl:choose>
          </td>
          <td>
            <div style="font-family:Verdana; font-size: 12px;color: black"><xsl:value-of select="MessageText/@text"/></div>
          </td>
        </tr>
      </p>
  </xsl:template>

  <xsl:template match="ExtendedInfos">
    <table style="font-family:Verdana; font-size: 14px;color: sienna;">
      <xsl:for-each select="ExtendedInfo">
        <xsl:choose>
            <xsl:when test="@name[.='dbDisconnectTitle']">
              <p style="font-family:Verdana; font-size: 16px;color: green;">
                <xsl:value-of select="@value"/> 
              </p>
            </xsl:when>
            <xsl:when test="@name[.='sourceTitle']">
              <p style="font-family:Verdana; font-size: 14px;color: darkolivegreen;">
                <xsl:value-of select="@value"/>
              </p>
            </xsl:when>
            <xsl:when test="@name[.='open error']">
            <p style="font-family:Verdana; font-size: 12px;color: black;">
              <xsl:value-of select="@value"/>
            </p>
          </xsl:when>
          <xsl:when test="@name[.='detailTitle']">
          <br></br>
          <tr>
          <td style="font-family:Verdana; font-size: 14px;color: darkolivegreen;">
            <xsl:value-of select="@value"/>
          </td>
          </tr>
          </xsl:when>
      <xsl:otherwise>
        <tr>
          <td >
          </td>
          <td >
          </td>
          <td >
            <xsl:value-of select="@name"/>
          </td>
          <td >
            <xsl:value-of select="@value"/>
          </td>
        </tr>
      </xsl:otherwise>
      </xsl:choose>
      </xsl:for-each>
    </table>
  </xsl:template>

</xsl:stylesheet>
