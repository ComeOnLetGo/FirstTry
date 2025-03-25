<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:ProjectApiExtensions="http://www.sdl.com/ProjectApiExtensions"
    xmlns:ps="http://sdl.com/projectserver/2010"
    xmlns:neo="http://sdl.com/projectserver/neo"
    exclude-result-prefixes="msxsl ps ProjectApiExtensions">

  <xsl:output method="xml" indent="yes" encoding="utf-8"/>

  <xsl:param name="serverUri" />
  <xsl:param name="serverUserName" />
  <xsl:param name="serverUserType" />

  <xsl:template match="ps:PackageProject">
    <Project>
      <xsl:apply-templates select="@*"/>

      <!-- generate empty LanguageDirections element if not included -->
      <xsl:if test="not(ps:LanguageDirections)">
        <LanguageDirections/>
      </xsl:if>

      <!-- insert file settings bundles if no settings bundles element-->
      <xsl:if test="not(ps:SettingsBundles) and not(ps:LanguageDirections) and not(ps:TermbaseConfiguration)">
        <xsl:call-template name="FileOnlySettingsBundles" />
      </xsl:if>

      <xsl:apply-templates select="node()"/>
    </Project>
  </xsl:template>

  <xsl:template match="ps:LanguageDirections">
    <LanguageDirections>
      <xsl:apply-templates select="@* | node()"/>
    </LanguageDirections>

    <xsl:if test="not(following-sibling::ps:SettingsBundles) and not(following-sibling::ps:TermbaseConfiguration) ">
      <xsl:call-template name="FileOnlySettingsBundles" />
    </xsl:if>
  </xsl:template>

  <xsl:template match="ps:TermbaseConfiguration">
    <TermbaseConfiguration>
      <xsl:apply-templates select="@* | node()"/>
    </TermbaseConfiguration>
        
    <xsl:if test="not(following-sibling::ps:SettingsBundles) ">
      <xsl:call-template name="FileOnlySettingsBundles" />
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="ps:SourceLanguageCode">
    <xsl:if test="not(preceding-sibling::ps:GeneralProjectInfo)">
      <GeneralProjectInfo IsInPlace="false" DueDate="2013-07-22T16:00:00" IsImported="false" Name="$$__missing__$$" Status="Started" CreatedBy="none" CreatedAt="2013-07-19T09:22:18.887Z" />
    </xsl:if>

    <SourceLanguageCode>
      <xsl:value-of select="."/>
    </SourceLanguageCode>
  </xsl:template>
  
  <!-- Copy Settings from the Server & append with local setting info -->
  <xsl:template match="ps:SettingsGroup[@Id='PublishProjectOperationSettings']" >
    <SettingsGroup>
      <xsl:attribute name="Id">PublishProjectOperationSettings</xsl:attribute>
      <xsl:apply-templates select="@* | node()"/>
      <Setting Id="ServerUri">
        <xsl:value-of select="$serverUri"/>
      </Setting>
      <Setting Id="PublicationStatus">Published</Setting>
      <Setting Id="ServerUserName">
        <xsl:value-of select="$serverUserName"/>
      </Setting>
      <Setting Id="ServerUserType">
        <xsl:value-of select="$serverUserType"/>
      </Setting>
    </SettingsGroup>

    <!-- add phases settings-->
    <SettingsGroup Id="LanguageFileServerPhasesSettings">
      <Setting Id="Phases">
        <ArrayOfstring xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
          <xsl:for-each select="$ProjectPhasesSettingsNode/ProjectPhasesSettings/lf">
            <string>
              <xsl:value-of select="@Name"/>
            </string>
          </xsl:for-each>
        </ArrayOfstring>
      </Setting>
      <xsl:for-each select="$ProjectPhasesSettingsNode/ProjectPhasesSettings/lf">
        <Setting>
          <xsl:attribute name="Id">
            <xsl:text>ProjectPhaseId_</xsl:text>
            <xsl:value-of select="@Name"/>
          </xsl:attribute>
          <xsl:value-of select="@Id"/>
        </Setting>
        <Setting>
          <xsl:attribute name="Id">
            <xsl:text>Order_</xsl:text>
            <xsl:value-of select="@Name"/>
          </xsl:attribute>
          <xsl:value-of select="@Order"/>
        </Setting>
        <Setting>
          <xsl:attribute name="Id">
            <xsl:text>Description_</xsl:text>
            <xsl:value-of select="@Name"/>
          </xsl:attribute>
          <xsl:value-of select="@Description"/>
        </Setting>
      </xsl:for-each>
    </SettingsGroup>

  </xsl:template>


  <!-- Deal with missing confirmation and analysis stats-->
  <xsl:template match="ps:FileVersions">
    <FileVersions>
      <xsl:apply-templates select="@* | node()"/>
    </FileVersions>
    <xsl:choose>
      <xsl:when test="parent::node()/ps:AnalysisStatistics">
        <xsl:apply-templates mode="AnalysisStatistics" select="parent::node()/ps:AnalysisStatistics" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="EmptyAnalysisStatistics" />
      </xsl:otherwise>
    </xsl:choose>

    <xsl:choose>
      <xsl:when test="parent::node()/ps:ConfirmationStatistics">
        <xsl:apply-templates mode="ConfirmationStatistics" select="parent::node()/ps:ConfirmationStatistics" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="EmptyConfirmationStatistics" />
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template match="ps:AnalysisStatistics">
  </xsl:template>


  <xsl:template match="ps:AnalysisStatistics" mode="AnalysisStatistics">
    <xsl:element name="AnalysisStatistics">
      <xsl:attribute name="WordCountStatus">
        <xsl:value-of select="@WordCountStatus"/>
      </xsl:attribute>
      <xsl:attribute name="AnalysisStatus">
        <xsl:value-of select="@AnalysisStatus"/>
      </xsl:attribute>

      <xsl:choose>
        <xsl:when test="@WordCountStatus!='Complete'">
          <Total Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="ps:Total"/>
        </xsl:otherwise>
      </xsl:choose>

      <xsl:choose>
        <xsl:when test="@AnalysisStatus!='Complete'">
          <Repetitions Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
          <Perfect Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
          <InContextExact Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
          <Exact Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
          <Fuzzy>
            <xsl:for-each select="//ps:AnalysisBand">
              <CountData Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
            </xsl:for-each>
          </Fuzzy>
          <New Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="ps:Repetitions"/>
          <xsl:apply-templates select="ps:Perfect"/>
          <xsl:apply-templates select="ps:InContextExact"/>
          <xsl:apply-templates select="ps:Exact"/>
          <xsl:apply-templates select="ps:Fuzzy"/>
          <xsl:apply-templates select="ps:New"/>            
        </xsl:otherwise>
      </xsl:choose>

    </xsl:element>
  </xsl:template>

  <xsl:template name="EmptyAnalysisStatistics">
    <AnalysisStatistics WordCountStatus="None" AnalysisStatus="None">
      <Total Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
      <Repetitions Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
      <Perfect Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
      <InContextExact Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
      <Exact Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
      <Fuzzy>
        <xsl:for-each select="//ps:AnalysisBand">
          <CountData Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
        </xsl:for-each>
      </Fuzzy>
      <New Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />   
    </AnalysisStatistics>
  </xsl:template>


  <xsl:template match="ps:ConfirmationStatistics">
  </xsl:template>


  <xsl:template match="ps:ConfirmationStatistics" mode="ConfirmationStatistics">
    <xsl:element name="ConfirmationStatistics">
      <xsl:attribute name="FileTimeStamp">
        <xsl:value-of select="@FileTimeStamp"/>
      </xsl:attribute>
      <xsl:attribute name="Status">
        <xsl:value-of select="@Status"/>
      </xsl:attribute>
      <xsl:choose>
        <xsl:when test="@Status!='Complete'">
          <Unspecified Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
          <Draft Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
          <Translated Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
          <RejectedTranslation Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
          <ApprovedTranslation Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
          <RejectedSignOff Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
          <ApprovedSignOff Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="ps:Unspecified"/>
          <xsl:apply-templates select="ps:Draft"/>
          <xsl:apply-templates select="ps:Translated"/>
          <xsl:apply-templates select="ps:RejectedTranslation"/>
          <xsl:apply-templates select="ps:ApprovedTranslation"/>
          <xsl:apply-templates select="ps:RejectedSignOff"/>
          <xsl:apply-templates select="ps:ApprovedSignOff"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>

  <xsl:template name="EmptyConfirmationStatistics">
    <ConfirmationStatistics Status="None">
      <Unspecified Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
      <Draft Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
      <Translated Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
      <RejectedTranslation Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
      <ApprovedTranslation Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
      <RejectedSignOff Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
      <ApprovedSignOff Words="0" Characters="0" Segments="0" Placeables="0" Tags="0" />
    </ConfirmationStatistics>
  </xsl:template>

  <!-- Move CheckedOutAt and CheckedOutBy to a file settings bundle -->
  <xsl:variable name="CheckOutSettings">
    <CheckOutSettings>
      <xsl:for-each select="/ps:PackageProject/ps:ProjectFiles/ps:ProjectFile/ps:LanguageFiles/ps:LanguageFile[@CheckedOutTo or @neo:IsCheckedOutOnline]">
        <lf>
          <xsl:attribute name="ProjectFileGuid">
            <xsl:value-of select="../../@Guid"/>
          </xsl:attribute>
          <xsl:attribute name="LanguageFileGuid">
            <xsl:value-of select="@Guid"/>
          </xsl:attribute>
          <xsl:attribute name="SettingsBundleGuid">
            <xsl:value-of select="@Guid"/>
          </xsl:attribute>
          <xsl:attribute name="CheckedOutTo">
            <xsl:value-of select="@CheckedOutTo"/>
          </xsl:attribute>
          <xsl:attribute name="CheckedOutAt">
            <xsl:value-of select="@CheckedOutAt"/>
          </xsl:attribute>
          <xsl:attribute name="IsCheckedOutOnline">
            <xsl:value-of select="@neo:IsCheckedOutOnline"/>
          </xsl:attribute>
        </lf>
      </xsl:for-each>
    </CheckOutSettings>
  </xsl:variable>

  <xsl:variable name="CheckOutSettingsNode" select="msxsl:node-set($CheckOutSettings)" />

  <!-- phases for the project -->
  <xsl:variable  name="ProjectPhasesSettings">
    <ProjectPhasesSettings>
      <xsl:for-each select ="/ps:PackageProject/ps:Phases/ps:Phase">
        <lf>
          <xsl:attribute name="Description">
            <xsl:value-of select="@Description"/>
          </xsl:attribute>
          <xsl:attribute name="Name">
            <xsl:value-of select="@Name"/>
          </xsl:attribute>
          <xsl:attribute name ="Order">
            <xsl:value-of select="@Order"/>
          </xsl:attribute>
          <xsl:attribute name="Id">
            <xsl:value-of select="@ProjectPhaseId"/>
          </xsl:attribute>
        </lf>
      </xsl:for-each>
    </ProjectPhasesSettings>
  </xsl:variable>

  <xsl:variable name="ProjectPhasesSettingsNode" select="msxsl:node-set($ProjectPhasesSettings)"/>

  <!-- Define root settings for groupware server -->
  <xsl:variable name="ProjectServerSettings">
    <ProjectServerSettings>
      <xsl:for-each select="/ps:PackageProject/ps:ProjectFiles/ps:ProjectFile/ps:LanguageFiles/ps:LanguageFile[@CheckedOutTo or @neo:IsCheckedOutOnline or ps:LanguageFileAssignments/ps:LanguageFileAssignment ]">
        <lf>
          <xsl:attribute name="SettingsBundleGuid">
            <xsl:value-of select="@Guid"/>
          </xsl:attribute>
        </lf>
      </xsl:for-each>
    </ProjectServerSettings>
  </xsl:variable>

  <xsl:variable name="ProjectServerSettingsNode" select="msxsl:node-set($ProjectServerSettings)"/>

  <!-- file assigment -->
  <xsl:variable name="FileAssignmentSettings">
    <FileAssignmentSettings>
      <xsl:for-each select="/ps:PackageProject/ps:ProjectFiles/ps:ProjectFile/ps:LanguageFiles/ps:LanguageFile/ps:LanguageFileAssignments/ps:LanguageFileAssignment">
        <xsl:sort select="@Version" order="descending" data-type="number"/>
        <lf>
          <xsl:attribute name="SettingsBundleGuid">
            <xsl:value-of select="../../@Guid"/>
          </xsl:attribute>
          <xsl:attribute name="ProjectFileGuid">
            <xsl:value-of select="../../../../@Guid"/>
          </xsl:attribute>
          <xsl:attribute name="LanguageFileGuid">
            <xsl:value-of select="../../@Guid"/>
          </xsl:attribute>
          <xsl:attribute name="AssignedBy">
            <xsl:value-of select="@AssignedBy"/>
          </xsl:attribute>
          <xsl:attribute name="Version">
            <xsl:value-of select="@Version"/>
          </xsl:attribute>
          <xsl:attribute name="AssignedAt">
            <xsl:value-of select="@AssignedAt"/>
          </xsl:attribute>
          <xsl:attribute name="DueDate">
            <xsl:choose>
              <xsl:when test="not (@DueDate)">
                <xsl:value-of select="null"/>              
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="@DueDate"/>              
            </xsl:otherwise>
            </xsl:choose>            
          </xsl:attribute>
          <xsl:attribute name="IsCurrentAssignment">
            <xsl:choose>
              <xsl:when test="@IsCurrentAssignment = '1'">
                <xsl:value-of select="'True'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'False'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:attribute name="PhaseName">
            <xsl:value-of select="@PhaseName"/>
          </xsl:attribute>
          <Assignees>
            <xsl:for-each select ="ps:Assignees/ps:Assignee">
              <Assignee>
                <xsl:attribute name="AssignedTo">
                  <xsl:value-of select="@AssignedTo"/>
                </xsl:attribute>
              </Assignee>
            </xsl:for-each>
          </Assignees>
        </lf>
      </xsl:for-each>

    </FileAssignmentSettings>
  </xsl:variable>

  <xsl:variable name="FileAssignmentSettingsNode" select="msxsl:node-set($FileAssignmentSettings)" />

  <xsl:template match="ps:SettingsBundles">
    <SettingsBundles>
      <!-- Copy existing settings bundles-->
      <xsl:apply-templates select="ps:SettingsBundle" />

      <xsl:call-template name="FileSettingsBundles" />
    </SettingsBundles>
  </xsl:template>

  <!-- This template is called from various places to generate file settings bundles in case the main SettingsBundles element was not included in the package -->
  <xsl:template name="FileOnlySettingsBundles">
    <SettingsBundles>
      <xsl:call-template name="FileSettingsBundles" />
    </SettingsBundles>
  </xsl:template>
    
    <xsl:template name="FileSettingsBundles">
    <!-- Add file settings bundles -->
    <xsl:for-each select="$ProjectServerSettingsNode/ProjectServerSettings/lf">
      <xsl:variable name="ProjectFileGuid" select="@SettingsBundleGuid"/>
      <SettingsBundle>
        <xsl:attribute name="Guid">
          <xsl:value-of select="@SettingsBundleGuid"/>
        </xsl:attribute>
        <SettingsBundle>

          <xsl:for-each select ="$ProjectPhasesSettingsNode/ProjectPhasesSettings/lf">
            <xsl:sort data-type ="number" order="ascending" select="Order"/>
            <xsl:variable name ="phaseName" select="@Name"/>

            <xsl:for-each select="$FileAssignmentSettingsNode/FileAssignmentSettings/lf[@SettingsBundleGuid=$ProjectFileGuid and @PhaseName=$phaseName]">
              <xsl:variable name="mo" select ="position()" />
              <xsl:if test="position() = 1">
                <SettingsGroup>
                  <xsl:attribute name="Id">
                    <xsl:text>LanguageFileServerAssignmentsSettings_</xsl:text>
                    <xsl:value-of select="@PhaseName"/>
                  </xsl:attribute>

                  <Setting>
                    <xsl:attribute name="Id">
                      <xsl:text>AssignedAt</xsl:text>
                    </xsl:attribute>
                    <xsl:value-of select="@AssignedAt"/>
                  </Setting>
                  
                  <Setting>
                    <xsl:attribute name="Id">
                      <xsl:text>DueDate</xsl:text>
                    </xsl:attribute>
                    <xsl:value-of select="@DueDate"/>
                  </Setting>

                  <Setting>
                    <xsl:attribute name="Id">
                      <xsl:text>IsCurrentAssignment</xsl:text>
                    </xsl:attribute>
                    <xsl:value-of select="@IsCurrentAssignment"/>
                  </Setting>

                  <Setting>
                    <xsl:attribute name="Id">
                      <xsl:text>AssignedBy</xsl:text>
                    </xsl:attribute>
                    <xsl:value-of select="@AssignedBy"/>
                  </Setting>

                  <Setting>
                    <xsl:attribute name="Id">
                      <xsl:text>Version</xsl:text>
                    </xsl:attribute>
                    <xsl:value-of select="@Version"/>
                  </Setting>

                  <Setting>
                    <xsl:attribute name="Id">
                      <xsl:text>Assignees</xsl:text>
                    </xsl:attribute>
                    <ArrayOfstring xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
                      <xsl:for-each select="Assignees/Assignee">
                        <string>
                          <xsl:value-of select="@AssignedTo"/>
                        </string>
                      </xsl:for-each>
                    </ArrayOfstring>
                  </Setting>
                </SettingsGroup>
              </xsl:if>
            </xsl:for-each>
          </xsl:for-each>

          <xsl:for-each select="$CheckOutSettingsNode/CheckOutSettings/lf[@SettingsBundleGuid=$ProjectFileGuid]">
            <SettingsGroup Id="LanguageFileServerStateSettings">
              <Setting Id="CheckedOutTo">
                <xsl:value-of select="@CheckedOutTo"/>
              </Setting>
              <Setting Id="CheckedOutAt">
                <xsl:value-of select="@CheckedOutAt"/>
              </Setting>
              <Setting Id="IsCheckedOutOnline">
                <xsl:value-of select="@IsCheckedOutOnline"/>
              </Setting>
            </SettingsGroup>
          </xsl:for-each>
        </SettingsBundle>
      </SettingsBundle>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="/ps:PackageProject/ps:ProjectFiles/ps:ProjectFile/ps:LanguageFiles/ps:LanguageFile">
    <!-- Add SettingsBundleGuid attribute -->
    <LanguageFile>
      <!-- Copy language file attributes -->
      <xsl:apply-templates select="@*"/>

      <!-- Add SettingsBundleGuid attribute if necessary -->
      <xsl:variable name="LanguageFileGuid" select="@Guid"/>
      <xsl:variable name="SettingsBundleGuid" select="$ProjectServerSettingsNode/ProjectServerSettings/lf[@SettingsBundleGuid=$LanguageFileGuid]/@SettingsBundleGuid" />
      <xsl:if test="$SettingsBundleGuid">
        <xsl:attribute name="SettingsBundleGuid">
          <xsl:value-of select="$SettingsBundleGuid"/>
        </xsl:attribute>
      </xsl:if>

      <!-- Copy language file child nodes -->
      <xsl:apply-templates select="node()"/>
    </LanguageFile>
  </xsl:template>

  <!-- Strip out CheckedOutTo attribute, which is moved to a settingsbundle above -->
  <xsl:template match="/ps:PackageProject/ps:ProjectFiles/ps:ProjectFile/ps:LanguageFiles/ps:LanguageFile/@CheckedOutTo">
  </xsl:template>

  <!-- Strip out CheckedOutAt attribute, which is moved to a settingsbundle above -->
  <xsl:template match="/ps:PackageProject/ps:ProjectFiles/ps:ProjectFile/ps:LanguageFiles/ps:LanguageFile/@CheckedOutAt">
  </xsl:template>

  <!-- Strip out IsCheckedOutOnline attribute, which is moved to a settingsbundle above -->
  <xsl:template match="/ps:PackageProject/ps:ProjectFiles/ps:ProjectFile/ps:LanguageFiles/ps:LanguageFile/@neo:IsCheckedOutOnline">
  </xsl:template>

  <!-- strip out phases, which is moved to settingsbundle above -->
  <xsl:template match ="/ps:PackageProject/ps:Phases">
  </xsl:template>

  <!--strip out file assignment-->
  <xsl:template match ="/ps:PackageProject/ps:ProjectFiles/ps:ProjectFile/ps:LanguageFiles/ps:LanguageFile/ps:LanguageFileAssignments[ps:LanguageFileAssignment]">
  </xsl:template>
  <xsl:template match ="/ps:PackageProject/ps:ProjectFiles/ps:ProjectFile/ps:LanguageFiles/ps:LanguageFile/ps:LanguageFilePhases">
  </xsl:template>

  <!-- Add Enabled attribute to translation providers -->
  <xsl:template match="ps:ProjectTranslationProviderItem">
    <ProjectTranslationProviderItem>
      <xsl:attribute name="Enabled">true</xsl:attribute>
      <xsl:apply-templates select="@* | node()"/>
    </ProjectTranslationProviderItem>
  </xsl:template>

  <xsl:template match="ps:MainTranslationProviderItem">
    <MainTranslationProviderItem>
      <xsl:attribute name="Enabled">true</xsl:attribute>
      <xsl:apply-templates select="@* | node()"/>
    </MainTranslationProviderItem>
  </xsl:template>

  <!-- Generic templates to copy everything else from the projectserver 2010 namespace to the default namespace -->
  <xsl:template match="*">
    <xsl:choose>
      <xsl:when test="namespace-uri() = 'http://sdl.com/projectserver/2010'">
        <xsl:element name="{local-name(.)}">
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