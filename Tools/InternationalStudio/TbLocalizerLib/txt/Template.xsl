<?xml version="1.0"?>
<xsl:stylesheet version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:tbl="urn:TBLocalizer">
	<xsl:output method="xml" encoding="UTF-8"/>	
	
	<xsl:template match="/">
		<xsl:apply-templates/>
	</xsl:template>	
	
	<xsl:template match="mtf">
		<glossary>
			<xsl:apply-templates/>
		</glossary>
	</xsl:template>	
	
	<xsl:template match="conceptGrp">
		<xsl:variable name="base" select="languageGrp/termGrp[tbl:ToLower(../language/@lang)=tbl:GetBaseLanguage()]/term/text()"/>
		<xsl:variable name="target" select="languageGrp/termGrp[tbl:ToLower(../language/@lang)=tbl:GetTargetLanguage()]/term/text()"/>
		<xsl:if test="$base!='' and $target!=''" >
			<string>
				<xsl:attribute name="base"> 
					<xsl:value-of select="tbl:TreatForGlossary($base)"/>
				</xsl:attribute>
				<xsl:attribute name="target"> 
					<xsl:value-of select="tbl:TreatForGlossary($target)"/>
				</xsl:attribute>
			</string>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>

  