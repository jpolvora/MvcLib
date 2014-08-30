this is a dummy readme file.

add a global.asax file to the root:

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

include this web.config entries

<?xml version="1.0" encoding="utf-8"?>
<!--
  For more information on how to configure your ASP.NET application, please visit
  http://go.microsoft.com/fwlink/?LinkId=169433
  -->
<configuration>
  <configSections>

    <sectionGroup name="system.web.webPages.razor" type="System.Web.WebPages.Razor.Configuration.RazorWebSectionGroup, System.Web.WebPages.Razor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35">
      <section name="host" type="System.Web.WebPages.Razor.Configuration.HostSection, System.Web.WebPages.Razor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" requirePermission="false" />
      <section name="pages" type="System.Web.WebPages.Razor.Configuration.RazorPagesSection, System.Web.WebPages.Razor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" requirePermission="false" />
    </sectionGroup>
    
    <section name="entityFramework" type="System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false" />
    <!-- For more information on Entity Framework configuration, visit http://go.microsoft.com/fwlink/?LinkID=237468 -->
  </configSections>
  <appSettings>

    <add key="TracerHttpModule" value="True" />
    <add key="TracerHttpModuleEvents" value="dummy" />

    <add key="CustomErrorHttpModule" value="True" />
    <add key="CustomErrorViewPath" value="~/views/shared/customerror.cshtml" />
    <add key="CustomErrorController" value="" />

    <add key="DbFileContextKey" value="DbFileContext" />
    <add key="DbFileContextVerbose" value="True" />

    <add key="CustomVirtualPathProvider" value="False" />
    <add key="WebCacheWrapper" value="False" />

    <add key="DumpToLocal" value="True" />
    <add key="DumpToLocalFolder" value="~/dbfiles" />

    <add key="PluginLoader" value="True" />
    <add key="Kompiler" value="True" />
    <add key="KompilerForceRecompilation" value="True" />

    <add key="MvcTracerFilter" value="False" />

    <add key="webpages:Version" value="3.0.0.0" />
    <add key="webpages:Enabled" value="true" />
    <add key="enableSimpleMembership" value="false" />
    <add key="ClientValidationEnabled" value="true" />
    <add key="UnobtrusiveJavaScriptEnabled" value="true" />

    <add key="Environment" value="Debug" />

    <add key="aspnet:UseHostHeaderForRequestUrl" value="true" />

  </appSettings>

  <connectionStrings>
    <add name="DbFileContext" connectionString="Server=.\SQLEXPRESS;Database=MvcFromDb; Integrated Security=True;" providerName="System.Data.SqlClient" />
  </connectionStrings>

  <system.web>
    <compilation debug="true" targetFramework="4.5.1" />

    <trace enabled="false" localOnly="true" />
    <httpRuntime delayNotificationTimeout="120" waitChangeNotification="180" maxWaitChangeNotification="30" targetFramework="4.5.1" maxRequestLength="102400" executionTimeout="3600" />
    <globalization culture="pt-BR" uiCulture="pt-BR" enableClientBasedCulture="false" requestEncoding="utf-8" responseEncoding="utf-8" fileEncoding="utf-8" enableBestFitResponseEncoding="false" />
    <authentication mode="None">

    </authentication>
    <customErrors mode="RemoteOnly" />

    <!--<healthMonitoring enabled="true" heartbeatInterval="0">
      <providers>
        <add name="TraceEventProvider" type="System.Web.Management.TraceWebEventProvider,             System.Web,Version=4.0.0.0, Culture=neutral,PublicKeyToken=b03f5f7f11d50a3a" />
      </providers>
      <profiles>
        <add name="Trace" minInstances="1" maxLimit="Infinite" minInterval="00:00:00" />
      </profiles>
      <rules>
        <remove name="All Errors" />
        <add name="All Errors" eventName="All Errors" provider="TraceEventProvider" profile="Default" minInstances="1" maxLimit="Infinite" minInterval="00:00:05" custom="" />

        <remove name="Application Events" />
        <add name="Application Events" eventName="Application Lifetime Events" provider="TraceEventProvider" profile="Default" minInstances="1" maxLimit="Infinite" minInterval="00:00:05" custom="" />
      </rules>
    </healthMonitoring>-->

  </system.web>

  <system.webServer>
    <handlers>
      <add name="AspNetStaticFileHandler-CSS" path="*.css" verb="GET,HEAD" type="System.Web.StaticFileHandler" />
      <add name="AspNetStaticFileHandler-JS" path="*.js" verb="GET,HEAD" type="System.Web.StaticFileHandler" />
      <add name="AspNetStaticFileHandler-JPPG" path="*.jpg" verb="GET,HEAD" type="System.Web.StaticFileHandler" />
      <add name="AspNetStaticFileHandler-png" path="*.png" verb="GET,HEAD" type="System.Web.StaticFileHandler" />
      <add name="AspNetStaticFileHandler-txt" path="*.txt" verb="GET,HEAD" type="System.Web.StaticFileHandler" />
    </handlers>
    <staticContent>
      <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="7.00:00:00" />
      <remove fileExtension=".woff" />
      <mimeMap fileExtension=".woff" mimeType="application/x-font-woff" />
    </staticContent>

  </system.webServer>

  <system.web.webPages.razor>
    <host factoryType="System.Web.WebPages.Razor.WebRazorHostFactory" />
    <pages pageBaseType="MvcLib.Common.Mvc.CustomPageBase">
      <namespaces>
        <add namespace="System" />
        <add namespace="System.Web" />
        <add namespace="System.Web.Helpers" />
        <add namespace="System.Web.WebPages" />
        <add namespace="System.Web.WebPages" />
        <add namespace="System.Web.WebPages.Razor" />
        <add namespace="System.Web.WebPages.Html" />
        <add namespace="System.Web.Optimization" />
        <add namespace="System.Web.Routing" />
        <add namespace="MvcLib.Common" />
        <add namespace="MvcLib.Common.Mvc" />
      </namespaces>
    </pages>
  </system.web.webPages.razor>
  
  <runtime>
    <!--<probing privatePath="~/_Plugins/" />-->
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-6.0.0.0" newVersion="6.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="WebGrease" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.6.5135.21930" newVersion="1.6.5135.21930" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Web.Helpers" publicKeyToken="31bf3856ad364e35" />
        <bindingRedirect oldVersion="1.0.0.0-3.0.0.0" newVersion="3.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Web.WebPages" publicKeyToken="31bf3856ad364e35" />
        <bindingRedirect oldVersion="1.0.0.0-3.0.0.0" newVersion="3.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Web.Mvc" publicKeyToken="31bf3856ad364e35" />
        <bindingRedirect oldVersion="1.0.0.0-5.2.2.0" newVersion="5.2.2.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Antlr3.Runtime" publicKeyToken="eb42632606e9261f" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-3.5.0.2" newVersion="3.5.0.2" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>

  <entityFramework codeConfigurationType="MvcLib.DbFileSystem.DbFileContextConfig, MvcLib.DbFileSystem">
    <defaultConnectionFactory type="System.Data.Entity.Infrastructure.SqlConnectionFactory, EntityFramework" />
    <contexts>
      <context type="MvcLib.DbFileSystem.DbFileContext, MvcLib.DbFileSystem" disableDatabaseInitialization="false">
        <databaseInitializer type="MvcLib.DbFileSystem.DbFileContextMigrationInitializer, MvcLib.DbFileSystem" />
      </context>
    </contexts>
    <providers>
      <provider invariantName="System.Data.SqlClient" type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer" />
    </providers>
  </entityFramework>
</configuration>