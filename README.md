# Harvest
Harvest Farm Project Tracking Website

[![Node Version](https://img.shields.io/badge/dynamic/json?color=green&label=node&query=%24.engines.node&url=https%3A%2F%2Fraw.githubusercontent.com%2Fucdavis%2FHarvest%2Fmain%2FHarvest.Web%2FClientApp%2Fpackage.json)](https://img.shields.io/badge/dynamic/json?color=green&label=node&query=%24.engines.node&url=https%3A%2F%2Fraw.githubusercontent.com%2Fucdavis%2FHarvest%2Fmain%2FHarvest.Web%2FClientApp%2Fpackage.json)
![CodeQL](https://github.com/ucdavis/Harvest/workflows/CodeQL/badge.svg)
[![Build Status](https://dev.azure.com/ucdavis/Harvest/_apis/build/status/Harvest%20Web%20Build?branchName=main)](https://dev.azure.com/ucdavis/Harvest/_build/latest?definitionId=25&branchName=main)

# Local Setup

Requires .net 5 SDK from https://dotnet.microsoft.com/download

Requires nodeJS, recommended version 14.x

In the `Harvest.Web/ClientApp` folder, run `npm install --legacy-peer-deps`.  Technically this step is optional but it's useful to do to get things started.

# Run locally

Get the user-secrets file and store it [in the correct location](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows#how-the-secret-manager-tool-works)

In the `Harvest.Web` folder, run:

`npm start`

# Development

Make sure to invoke "Prettier" before committing JS changes.  If using VSCode consider [using the plugin](https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode).

If making large JS changes, run `npm test` inside the `Harvest.Web/ClientApp` directory and it will automatically re-run affected tests.
