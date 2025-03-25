<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:ps="http://sdl.com/projectserver/2010"
    xmlns:ProjectApiExtensions="http://www.sdl.com/ProjectApiExtensions"
    exclude-result-prefixes="msxsl ProjectApiExtensions">
	<xsl:output method="xml" indent="yes" encoding="utf-8"/>

	<xsl:param name="packageGuid" />
	<xsl:param name="metaDataOnly" />

	<xsl:template match="Project">
		<ps:PackageProject>
			<xsl:attribute name="PackageGuid">
				<xsl:value-of select="$packageGuid"/>
			</xsl:attribute>

			<xsl:apply-templates select="@* | node()"/>
		</ps:PackageProject>
	</xsl:template>

	<!-- Do not include aggregated LanguageDirection analysis statistics -->
	<xsl:template match="LanguageDirection/AnalysisStatistics" >
	</xsl:template>

	<!-- Do not include aggregated LanguageDirection confirmation statistics -->
	<xsl:template match="LanguageDirection/ConfirmationStatistics" >
	</xsl:template>

	<!-- do not include LanguageIndexMappings without a Language and an Index -->
	<xsl:template match="LanguageIndexMappings[Language][not(Index)]">
	</xsl:template>

	<!-- strip unsupported elements  -->
	<xsl:template match="InitialTaskTemplate|ManualTaskTemplates|Tasks|SpecificTargetLanguages|PreviousBilingualFiles|PackageOperations|PackageLicenseInfo|Locked|TQAData">
	</xsl:template>

	<!-- remove unsupported attributes -->
	<xsl:template match="@ParentProjectFileGuid | @ReferenceProjectGuid | @ProjectTemplateGuid | @Owner | @IsCloudBased">
	</xsl:template>

	<!-- Do not include LanguageDirection translation provider cascades that do not override the default cascade -->
	<xsl:template match="LanguageDirection/CascadeItem[@OverrideParent = 'false']" >
	</xsl:template>

	<!-- Do not include LanguageDirection translation provider cascades that do not override the default cascade -->
	<xsl:template match="LanguageDirection/CascadeItem[@OverrideParent = 'false']" >
	</xsl:template>

	<!-- Do not include AutoSuggestDictionaries that point to local files -->
	<xsl:template match="LanguageDirection/AutoSuggestDictionaries/AutoSuggestDictionary[not(starts-with(@FilePath, '\\'))]" >
	</xsl:template>

	<!-- Do not include file-based language resource templates -->
	<xsl:template match="@LanguageResourceFilePath" >
	</xsl:template>

	<!-- Do not include file-based project translation providers -->
	<xsl:template match="ProjectTranslationProviderItem[starts-with(@Uri,'sdltm.file:')]">
	</xsl:template>

	<!-- Do not include AnyTm: Any file-based project translation providers -->
	<xsl:template match="ProjectTranslationProviderItem[starts-with(@Uri,'anytm.sdltm.file:')]">
	</xsl:template>

	<!-- Do not include Cascade Entry items with file based main translation providers  -->
	<xsl:template match="CascadeEntryItem[MainTranslationProviderItem[starts-with(@Uri,'sdltm.file:')]]">
	</xsl:template>
	
	<!-- Do not include Cascade Entry items with Any TM file based main translation providers  -->
    <xsl:template match="CascadeEntryItem[MainTranslationProviderItem[starts-with(@Uri,'anytm.sdltm.file:')]]">
    </xsl:template>

	<!-- Strip Enabled attribute from translation providers -->
	<xsl:template match="ProjectTranslationProviderItem/@Enabled">
	</xsl:template>

	<!-- Do not include Cascade Entry items with file based main translation providers  -->
	<xsl:template match="MainTranslationProviderItem/@Enabled">
	</xsl:template>

	<!-- Do not include local termbases (use extension object to check 'Local' property in escaped XML inside SettingsXml element) -->
	<xsl:template match="Termbases[ProjectApiExtensions:IsLocalTermbase(SettingsXml)]">
	</xsl:template>

	<!-- Strip Enabled attribute from termbases -->
	<xsl:template match="Termbases/@Enabled">
	</xsl:template>

	<!-- Strip EmailType attribute from User -->
	<xsl:template match="User/@EmailType">
	</xsl:template>

	<!-- Strip IsAutoUpload attribute from FileVersion as it is used only locally-->
	<xsl:template match="FileVersion/@IsAutoUpload">
	</xsl:template>

	<!-- Do not include settings bundles with just LanguageFileServerStateSettings, because these are purely local settings -->
	<xsl:template match="SettingsBundle[@Guid][SettingsBundle[count(SettingsGroup) = 1][SettingsGroup[@Id='LanguageFileServerStateSettings']]]">
	</xsl:template>

	<!-- Do not include LanguageFileServerStateSettings, because these are purely local settings -->
	<xsl:template match="SettingsGroup[@Id='LanguageFileServerStateSettings']">
	</xsl:template>

	<!-- Do not include ServerUserName -->
	<xsl:template match="SettingsGroup[@Id='PublishProjectOperationSettings']/Setting[@Id='ServerUserName']">
	</xsl:template>

	<!-- Do not include ServerUserType -->
	<xsl:template match="SettingsGroup[@Id='PublishProjectOperationSettings']/Setting[@Id='ServerUserType']">
	</xsl:template>

	<!-- If metaDataOnly=true, do not include the sections below -->
	<xsl:template match="Users|ProjectFiles">
		<xsl:if test="$metaDataOnly='false'">
			<xsl:element name="ps:{local-name(.)}">
				<xsl:apply-templates select="@* | node()"/>
			</xsl:element>
		</xsl:if>
	</xsl:template>

	<xsl:template match="*">
		<xsl:choose>
			<xsl:when test="namespace-uri() = ''">
				<xsl:element name="ps:{local-name(.)}">
					<xsl:apply-templates select="@* | node()"/>
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy>
					<xsl:apply-templates select="@* | node()"/>
				</xsl:copy>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@*">
		<xsl:copy/>
	</xsl:template>
</xsl:stylesheet>