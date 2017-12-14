<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:tbl="urn:TBLocalizer">
	
	<xsl:output method="xml" encoding="UTF-8"/>	
	
	<xsl:template match="/ | node() | @*">	
			<xsl:copy>
				<xsl:apply-templates select="@* | node()"/>
			</xsl:copy>
	</xsl:template>
	

	<xsl:template match="@base">
			<xsl:attribute name="base">
				<xsl:value-of select="tbl:ToLower(string(.))"/>
			</xsl:attribute>	
	</xsl:template>
	
	<xsl:template match="@target">
			<xsl:attribute name="target">
				<xsl:value-of select="tbl:ToLower(string(.))"/>
			</xsl:attribute>	
	</xsl:template>
	
	<xsl:template match="@support">
			<xsl:attribute name="support">
				<xsl:value-of select="tbl:ToLower(string(.))"/>
			</xsl:attribute>	
	</xsl:template>
	
</xsl:stylesheet>

  