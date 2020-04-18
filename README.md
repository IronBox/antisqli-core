# AntiSQLi Library
The AntiSQLi libray provides developers with convenient classes and extensions to reduce the risk from SQL injection (SQLi) attacks in their .NET applications.

The library provides support for:

- SQL Server
- Azure SQL Database
- Azure Cosmos DB/DocumentDB

## Installation

You don't need this source code unless you want to modify the library. If you just want to use the package, you can install it through the NuGet package gallery:

```powershell
Install-Package IronBox.AntiSQLi.Core
```

### Requirements

- .NET Standard 2.1+

## How this library works


## Usage


### System.Data.SqlClient


### Microsoft.Data.SqlClient



### Microsoft.Azure.Documents.SqlQuerySpec


### Microsoft.Azure.Cosmos.QueryDefinition

## About
In 2012, Kevin Lam ([IronBox](https://www.ironbox.io)) and Joe Basirico ([Security Innovation](https://www.securityinnovation.com)) were thinking of ways they could help .NET developers more easily defend their applications against SQL injection attacks, the #1 web application attack then. The [initial version of the AntiSQLi Library](https://github.com/IronBox/AntiSQLi) was developed and released in 2013.

In 2020, SQLi attacks continued to be the #1 web application attack and the vectors for this attack has also expanded. In response, a new version of the original AntiSQLi library was written with the goals of improving usability, integration and protection coverage.
