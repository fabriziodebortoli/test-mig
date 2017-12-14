<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">
  <xsl:template match="wix:Wix">
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>
  <!-- Copy all attributes and elements to the output. -->
  <xsl:template match="@*|*">
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>
  <xsl:output method="xml" indent="yes" />
  <xsl:key name="pdb-search" match="wix:Component[contains(wix:File/@Source, '.pdb')]" use="@Id" />
  <xsl:template match="wix:Component[key('pdb-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('pdb-search', @Id)]" />
  <xsl:key name="xml-search" match="wix:Component[contains(wix:File/@Source, '.xml')]" use="@Id" />
  <xsl:template match="wix:Component[key('xml-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('xml-search', @Id)]" />
  <xsl:key name="locked-search" match="wix:Component[contains(wix:File/@Source, '.locked')]" use="@Id" />
  <xsl:template match="wix:Component[key('locked-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('locked-search', @Id)]" />
  <xsl:key name="vshost-search" match="wix:Component[contains(wix:File/@Source, '.vshost.')]" use="@Id" />
  <xsl:template match="wix:Component[key('vshost-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('vshost-search', @Id)]" />
  
  <xsl:key name="resources-search" match="wix:Component[contains(wix:File/@Source, '.resources.')]" use="@Id" />
  <xsl:template match="wix:Component[key('resources-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('resources-search', @Id)]" />
  
  <xsl:template match='wix:Component'>
    <xsl:copy>
      <xsl:apply-templates select="@*"/>
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>
  <xsl:template match='wix:File'>
    <xsl:copy>
      <xsl:apply-templates select="@*"/>
      <xsl:attribute name="ProcessorArchitecture">
        <xsl:choose>
          <xsl:when test="contains(@Source, 'x64')">
            <xsl:text>x64</xsl:text>
          </xsl:when>
          <xsl:when test="contains(@Source, 'x86')">
            <xsl:text>x86</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>msil</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>