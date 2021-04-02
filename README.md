# DEPRECATED [![No Maintenance Intended](http://unmaintained.tech/badge.svg)](http://unmaintained.tech/)
This version is no longer supported and will not receive any updates nor bug fixes, please consider using the [new version](https://github.com/Avanade/Liquid-Application-Framework) instead.
We've made significative breaking changes on the new version of Liquid Application Framework. So, the old and new versions are not compatible and, also, there is no easy way to migrate from this to the new one.
This repo will be keeped only for historical purposes and to allow anyone, relying on this version, to fork the code and maintain it from there, if needed.


# Enter Liquid ![CI Badge](https://github.com/Avanade/Liquid-Application-Framework/workflows/CI/badge.svg)
Liquid is a **multi-cloud** framework designed to **accelerate the development** of cloud-native microservices while avoiding coupling your code to specific cloud providers. 

When writing Liquid applications, you stop worrying about the technology and focus on your business - Liquid abstracts most of the boilerplate and let you just write domain code that looks great and gets the job done.

## Features

- Abstracts a number of services from cloud providers such as Azure, AWS and Google Cloud to enable you to write code that will run anywhere.
- Brings a directed programming model that will save you time on thinking how to structure your application, allowing you to focus on writing business code.

# Getting Started

To use Liquid, you create a new base ASP.Net application and then download and install the following nuget packages:

- LiquidApplication.Base
- LiquidApplication.Activation
- LiquidApplication.Domain
- LiquidApplication.Middleware
- LiquidApplication.Repository
- LiquidApplication.Runtime

And then choose what implementation cartridge you need to run your environment:

- If you'll deploy your application to Azure, install `LiquidApplication.OnAzure`
- If you'll deploy your application to Google, install `LiquidApplication.OnGoogle`
- If you'll deploy your application to AWS, install `LiquidApplication.OnAWS`

# Contribute
Some of the best ways to contribute are to try things out, file issues, and make pull-requests.

- You can provide feedback by filing issues on GitHub. We accept issues, ideas and questions. 
- You can contribute by creating pull requests for the issues that are listed. Look for issues marked as _good first issue_ if you are new to the project.

In any case, be sure to take a look at [the contributing guide](CONTRIBUTING.md) before starting.
