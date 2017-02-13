# Cofoundry.Plugins.DependencyInjection.Autofac

[![Build status](https://ci.appveyor.com/api/projects/status/38avgn0152pq8ddm?svg=true)](https://ci.appveyor.com/project/Cofoundry/cofoundry-plugins-dependencyinjection-autofac)
[![NuGet](https://img.shields.io/nuget/v/Cofoundry.Plugins.SiteMap.svg)](https://www.nuget.org/packages/Cofoundry.Plugins.DependencyInjection.Autofac/)
[![Gitter](https://img.shields.io/gitter/room/cofoundry-cms/cofoundry.svg)](https://gitter.im/cofoundry-cms/cofoundry)


This library is a plugin for [Cofoundry](http://cofoundry.org/). For more information on getting started with Cofoundry check out the [Cofoundry repository](https://github.com/cofoundry-cms/cofoundry).

## Overview

> Autofac is an addictive Inversion of Control container for .NET Core, ASP.NET Core, .NET 4.5.1+, Universal Windows apps, and more.
>
> &mdash; [autofac.org](https://autofac.org/)

This library is an Autofac implementation of the [Cofoundry DI system](https://github.com/cofoundry-cms/cofoundry/wiki/Dependency-Injection). 

## Bootstrapping In a Website

The [Cofoundry.Plugins.DependencyInjection.AutoFac.Web](https://www.nuget.org/packages/Cofoundry.Plugins.DependencyInjection.Autofac.Web/) can be used to bootstrap your web application, which provides the following:

- Registers all Cofoundry and auto-registered dependencies
- Registers the MVC, WebApi and the common service locators
- Registers all MVC & WebApi controllers

Registration must be done before Cofoundry is initialized, i.e.

```csharp
using Microsoft.Owin;
using Owin;
using Cofoundry.Plugins.DependencyInjection.AutoFac.Web;
using Cofoundry.Web;

[assembly: OwinStartup(typeof(MySite.Startup))]

namespace MySite 
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCofoundryAutoFacIntegration();
            app.UseCofoundry();
        }
    } 
}
```

## Bootstrapping Manually

Outside of a web scenario, you can configure your Autofac container manually and add Cofoundry registrations using the `AutoFacContainerBuilder`

```csharp
using AutoFac;
using Cofoundry.Plugins.DependencyInjection.AutoFac;

var autoFacBuilder = new ContainerBuilder();
var cofoundryBuilder = new AutoFacContainerBuilder(autoFacBuilder);

cofoundryBuilder.Build();
```






