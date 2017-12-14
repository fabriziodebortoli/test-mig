<?xml version="1.0"?>
<!--Ripristina un file XML tradotto nel corrispondente file in lingua base; utilizza la 
stringa precedentemente salvata nell'attributo con namespace "urn:TBStringLoader" per
ripristinare la lingua base del nodo-->
<xsl:stylesheet version="1.0" 
	xmlns:xsl = "http://www.w3.org/1999/XSL/Transform"
	xmlns:sl = "urn:TBStringLoader">
	
<xsl:output method="xml" />	
	<xsl:template match="/ | node() | @*">	
		<xsl:choose>
			<xsl:when test="@localizable='true'">
				<xsl:variable name="name" select="concat('sl:', name())"/>
	
				<xsl:if test="not(@dictionary) and function-available('sl:AddCustomEntry')">
					<xsl:value-of select="sl:AddCustomEntry(string(@*[name()=$name]))"/>
				</xsl:if>
				
				<xsl:copy>
					<xsl:call-template name="processAttributes"/>					
					<xsl:value-of select="@*[name()=$name]"/>
					<xsl:apply-templates select="child::*"/>					
				</xsl:copy>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy>
					<xsl:call-template name="processAttributes"/>						
					<xsl:apply-templates select ="node()" /> 
				</xsl:copy>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
	<xsl:template name="processAttributes">
		<xsl:copy-of select="@*[not(starts-with(name(), 'sl:')) and name()!='localize']"/>
					
		<xsl:if test="@sl:baseLocalize">
			<xsl:attribute name="localize">
				<xsl:value-of select="@sl:baseLocalize"/>
			</xsl:attribute>
			<xsl:if test="function-available('sl:AddCustomEntry')">
				<xsl:value-of select="sl:AddCustomEntry(string(@sl:baseLocalize))"/>
			</xsl:if>
		</xsl:if>		
	</xsl:template>
	
</xsl:stylesheet>

  