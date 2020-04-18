# Anti-SQL Injection (AntiSQLi) Library
The AntiSQLi libray provides developers with convenient methods and class extensions to reduce the risk from SQL injection (SQLi) attacks in their .NET applications.

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



## Usage


#### `System.Data.SqlClient.SqlCommand`


#### `Microsoft.Data.SqlClient.SqlCommand`



#### `Microsoft.Azure.Documents.SqlQuerySpec`


#### `Microsoft.Azure.Cosmos.QueryDefinition`

## How this library works
When a dynamic SQL query is constructed using data from an untrusted source and then processed by a SQL interpreter, the potential for an attacker to execute unauthorized commands through the interpreter is created. Application code that contains this pattern is known to be vulnerable to [SQL injection (SQLi)](https://owasp.org/www-community/attacks/SQL_Injection) attacks.

```csharp
SqlCommand cmd = new SqlCommand();
cmd.Connection = new SqlConnection("connection_string");
cmd.CommandText = String.Format("SELECT * FROM UserTable WHERE un = '{0}'", username);
var dataReader = await cmd.ExecuteReaderAsync();
...
```

## About
In 2012, Kevin Lam ([IronBox](https://www.ironbox.io)) and Joe Basirico ([Security Innovation](https://www.securityinnovation.com)) were thinking of ways to help .NET developers more easily defend their applications against SQL injection attacks, the #1 web application attack then. The [initial version of the AntiSQLi Library](https://github.com/IronBox/AntiSQLi) was developed and released in 2013.

In 2020, SQLi continued to be the #1 web application attack; however, the surface area for this attack had greatly expanded. In response, a new version of the AntiSQLi library was written with the goals of improving usability, integration and protection coverage.
