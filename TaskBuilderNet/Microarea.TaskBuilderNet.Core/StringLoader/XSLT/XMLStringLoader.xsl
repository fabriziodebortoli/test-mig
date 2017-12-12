<?xml version="1.0"?>
<!--Gestisce la traduzione dei file XML; traduce il testo contenuto in tutti i nodi
con l'attributo <localizable> uguale a true, eventualmente utilizzando il dizionario
indicato mediante l'attributo <dictionary>, il cui contenuto sarà: <application.module.fileName>;
il valore ante traduzione viene salvato come attributo del nodo con namespace "urn:TBStringLoader"
e nome dell'attributo uguale al nome del nodo-->
<xsl:stylesheet version="1.0" 
		xmlns:xsl = "http://www.w3.org/1999/XSL/Transform" 
		xmlns:sl = "urn:TBStringLoader"
		xmlns:pf = "urn:TBPathFinder">
		
<xsl:output method="xml" encoding="UTF-8"/>	
	
	<xsl:template match="/ | node() | @*">	
		<xsl:choose>
			<xsl:when test="@localizable='true'">
				<xsl:copy>
					<xsl:call-template name="processAttributes"/>
					<xsl:choose>
						<xsl:when test="function-available('sl:Translate')">
							<xsl:attribute name="{concat('sl:', name())}" namespace="urn:TBStringLoader">
								<xsl:value-of select="text()"/>
							</xsl:attribute>
							<xsl:choose>
								<xsl:when test="@dictionary and
												function-available('pf:GetDictionaryFile') and
												function-available('pf:GetDictionaryPath')">
									
									<xsl:value-of select="sl:Translate (
																	string(text()), 
																	pf:GetDictionaryFile(string(@dictionary)), 
																	pf:GetDictionaryPath(string(@dictionary))
																	)"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="sl:Translate (string(text()), '', '')"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="text()"/>
						</xsl:otherwise>
					</xsl:choose>
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
		<xsl:copy-of select="@*[name()!='localize']"/>
		<xsl:if test="@localize">
			<xsl:attribute name="localize">
				<xsl:choose>
					<xsl:when test="function-available('sl:Translate')">
						<xsl:value-of select="sl:Translate (string(@localize), '', '')"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="@localize"/>
					</xsl:otherwise>
				</xsl:choose>				
			</xsl:attribute>
			<xsl:attribute name="sl:baseLocalize" namespace="urn:TBStringLoader">
					<xsl:value-of select="@localize"/>
			</xsl:attribute>
		</xsl:if>				
	</xsl:template>
</xsl:stylesheet>

  