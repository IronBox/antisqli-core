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
Whenever a dynamic SQL query is constructed using data from an untrusted source and then processed by a SQL interpreter, the potential for an attacker to execute unauthorized commands through the interpreter is created. This is because the untrusted data itself could contain executable SQL statements. Application code that contains this pattern is said to be vulnerable to [SQL injection (SQLi)](https://owasp.org/www-community/attacks/SQL_Injection) attacks.

### Automatic Query Parameterization
#### Understanding Query Parameterization
A common pattern for creating a dynamic SQL query is to express the query as a string with placeholders for variables. For example, the class method `String.Format` is a convenient way to implement this pattern.

```csharp
// Vulnerable SQLi application code example, username is untrusted data
SqlCommand cmd = new SqlCommand();
cmd.Connection = new SqlConnection("connection_string");
cmd.CommandText = String.Format("SELECT * FROM UserTable WHERE uname = '{0}'", username);
var dataReader = await cmd.ExecuteReaderAsync();
...
```
An approach to remediate the above vulnerable code is to use [SQL parameters](https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlcommand.parameters). When using SQL parameters, a unique identifier for the parameter is inserted into the query. Then a SQL parameter object is created with that unique identifier and the value of the untrusted data is read into the parameter. Other settings such as the parameter type and length might be set, but that is not always the case depending on the database technology or platform. The SQL interpreter processes the query and treats the parameter as non-executable code which mitigates any risk from SQLi attacks.

```csharp
SqlCommand cmd = new SqlCommand();
cmd.Connection = new SqlConnection("connection_string");
cmd.CommandText = "SELECT * FROM UserTable WHERE uname = '@username'";

SqlParameter parameter = new SqlParameter();
parameter.ParameterName = "@username";
parameter.Value = username;
parameter.DbType = SqlDbType.VarChar;
cmd.Parameters.Add(parameter);

var dataReader = await cmd.ExecuteReaderAsync();
...
```
Using parameters is an effective way to mitigate risk from SQLi attacks; however, it can become tedious and error-prone especially in scenarios with large numbers of query variables and query changes. Certainly, the convenience to the developer of using a single line of code to create a SQL query is lost.

#### How the AntiSQLi Library Helps
The **AntiSQLi library provides class extensions that automatically parameterizes and loads dynamic queries**. This means that developers can continue to use same single line pattern that they are used to, and the AntiSQLi library handles the task of query parameterization. Let's take a brief look at how this works:

```csharp
using IronBox.AntiSQLi.Core.Sql;
...
SqlCommand cmd = new SqlCommand();
cmd.Connection = new SqlConnection("connection_string");
cmd.LoadQuerySecure("SELECT * FROM UserTable WHERE uname = '{0}'", username);
var dataReader = await cmd.ExecuteReaderAsync();
...
```
At runtime, the class extension `.LoadQuerySecure(String queryText, params Object[] args)` performs two important tasks for the developer. The first is it analyzes the `args` object parameters provided and generates SQL parameter objects (assigns IDs, set types and values) for each.
```csharp
// These operations are automatically performed by the AntiSQLi library at runtime
SqlParameter parameter = new SqlParameter();
parameter.ParameterName = "@AntiSQLiParam1";
parameter.Value = username;
parameter.DbType = SqlDbType.VarChar;
cmd.Parameters.Add(parameter);

// Repeat for any additional arg objects, with new IDs (@AntiSQLiParam2, @AntiSQLiParam3 ...)
...
```
The second task performed by the library is it replaces the original format-items in the query to match the IDs of the generated parameters. The original query:
````csharp
cmd.CommandText = "SELECT * FROM UserTable WHERE uname = '{0}'";
````
would be replaced at runtime with:
````csharp
cmd.CommandText = "SELECT * FROM UserTable WHERE uname = '@AntiSQLiParam1'";
````
With the AntiSQLi library, developers can continue using the SQL coding patterns that they are already familiar with and let the library take care applying best practices to mitigate risks in an easy-to-use and repeatable way.

## About
In 2012, Kevin Lam ([IronBox](https://www.ironbox.io)) and Joe Basirico ([Security Innovation](https://www.securityinnovation.com)) were thinking of ways to help .NET developers more easily defend their applications against SQL injection (SQLi) attacks, the #1 web application attack then. The [initial version of the AntiSQLi Library](https://github.com/IronBox/AntiSQLi) was developed and released in 2013.

In 2020, SQLi continued to be the #1 web application attack; however, the surface area for this attack had greatly expanded. In response, the AntiSQLi library was completely re-written with the goals of improving ease-of-use for developers, integration and protection coverage.
