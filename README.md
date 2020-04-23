# Anti-SQL Injection (AntiSQLi) Library
The AntiSQLi library provides developers with convenient helper methods and class extensions to reduce the risk from SQL injection (SQLi) attacks in .NET applications.

It enables .NET developers to continue to use the following insecure coding patterns they are used, but without the risk of introducing SQLi vulnerabilities in their code:
````
cmd.CommandText = String.Format("SELECT * FROM MyTable WHERE uname = '{0}'", untrusted_data);
````
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
How you use the AntiSQLi Library depends on the provider class that you are using to perform your SQL queries. Here is a table to help guide you to the appropriate extensions and helper classes to use:

SQL Provider Class | Use AntiSQLi Class.Method 
--- | --- 
System.Data.SqlClient.SqlCommand | [SqlCommandExtensions.LoadQueryTextSecure](https://github.com/IronBox/antisqli-core/blob/master/README.md#systemdatasqlclientsqlcommand) 
Microsoft.Data.SqlClient.SqlCommand | [SqlCommandExtensions.LoadQueryTextSecure](https://github.com/IronBox/antisqli-core/blob/master/README.md#microsoftdatasqlclientsqlcommand)
Microsoft.Azure.Documents.Client.SqlQuerySpec | [CosmosDBExtensions.LoadQueryTextSecure](https://github.com/IronBox/antisqli-core/blob/master/README.md#microsoftazuredocumentssqlqueryspec)
Microsoft.Azure.Cosmos.QueryDefinition | [SecureQueryDefinition.Create](https://github.com/IronBox/antisqli-core/blob/master/README.md#microsoftazurecosmosquerydefinition)

More information and examples on using each is provided below:

### `System.Data.SqlClient.SqlCommand`
The [System.Data.SqlClient.SqlCommand](https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlcommand) class is the traditional mechanism for developers to execute Transact-SQL statements or stored procedures against a SQL database, such as SQL Server or Azure SQL Database. The AntiSQLi library extends this class with a method called `LoadQuerySecure` to automatically parameterize and load queries.
````csharp
LoadQuerySecure(this SqlCommand sqlCommandObj, String queryText, params Object[] queryTextArgs)
````
#### Example
````csharp
using IronBox.AntiSQLi.Core.Sql;
using System.Data.SqlClient;

using (var connection = new SqlConnection("connectionstring"))
{
    var cmd = new SqlCommand();
    cmd.Connection = connection;
    cmd.LoadQuerySecure(
        "SELECT CustomerName, City " +
        "FROM Customers " +
        "WHERE d1 = '{0}' AND d3 = {2} AND d2 = '{1}'", evil_data1, evil_data2, evil_data3);
    connection.Open();
    var dataReader = await cmd.ExecuteReaderAsync();
}
````

### `Microsoft.Data.SqlClient.SqlCommand`
Moving forward, [Microsoft.Data.SqlClient.SqlCommand](https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlclient.sqlcommand) is Microsoft's recommended Transact-SQL statement and stored procedure executor against a SQL database. The AntiSQLi library extends this class with a method called `LoadQuerySecure` to automatically parameterize and load queries.
````csharp
LoadQuerySecure(this SqlCommand sqlCommandObj, String queryText, params Object[] queryTextArgs)
````
#### Example
````csharp
using IronBox.AntiSQLi.Core.Sql;
using Microsoft.Data.SqlClient;

using (var connection = new SqlConnection("connectionstring"))
{
    SqlCommand cmd = new SqlCommand();
    cmd.connection = connection;
    cmd.LoadQuerySecure(
        "SELECT OrderID, CustomerID " +
        "FROM dbo.Orders " +
        "WHERE state = {0} OR state = {1}", evil_data1, evil_data2);
    connection.Open();
    using (var reader = cmd.ExecuteReader())
    {
        while (reader.Read())
        {
            Console.WriteLine(String.Format("{0}, {1}",
                reader[0], reader[1]));
        }
    }
}
````

### `Microsoft.Azure.Documents.SqlQuerySpec`
Azure Cosmos DB is Microsoft's cloud-based nonrelational database service that supports querying items using SQL. While Microsoft has implemented controls in the Azure Cosmos DB service to prevent privilege escalation, it may still be possible for an attacker to gain unauthorized access to data using SQLi attacks.

One way to perform a SQL query in the Azure Cosmos DB service is with the use of the [Microsoft.Azure.Documents.SqlQuerySpec](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.sqlqueryspec) class. The AntiSQLi library extends this class with a method called `LoadQuerySecure` to automatically parameterize and load queries.
````csharp
LoadQuerySecure(this SqlQuerySpec sqs, String queryText, params Object[] queryTextArgs)
````
#### Example
````csharp
using IronBox.AntiSQLi.Core.Cosmos;
using Microsoft.Azure.Documents.Client;

var querySpec = new SqlQuerySpec();
querySpec.LoadQuerySecure("select * from doc where doc.name = {0}", evil_data1);
var documentClient = new DocumentClient(new Uri("document_db_uri"), "document_key");
...
var queryResult = documentClient.CreateDocumentQuery("collection_link", querySpec);
````
### `Microsoft.Azure.Cosmos.QueryDefinition`
Another way to perform a SQL query against a Azure Cosmos DB service is with the use of the [Microsoft.Azure.Cosmos.QueryDefinition](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.querydefinition) class. 

This class only supports loading queries at the time of instantiation so using extensions was not possible. Instead, the AntiSQLi provides a helper class to automatically parameterize and load queries into a new QueryDefinition instance.
#### Example
````csharp
using IronBox.AntiSQLi.Core.Cosmos;
using Microsoft.Azure.Cosmos;

QueryDefinition parameterizedQueryDefinition = SecureQueryDefinition.Create("SELECT codes FROM productkeys WHERE pname = {0}", evil_data1);
var container = client.GetContainer("databasename", "collectionname");
var feedIterator = container.GetItemQueryIterator<String>(query);
while (feedIterator.HasMoreResults)
{
...
}
````

## How the AntiSQLi Library Works
Whenever a dynamic SQL query is constructed using data from an untrusted source and then processed by a SQL interpreter, the potential for an attacker to execute unauthorized commands through the interpreter is created. This is because the untrusted data itself could contain executable SQL statements. 

Application code that contains this pattern is said to be vulnerable to [SQL injection (SQLi)](https://owasp.org/www-community/attacks/SQL_Injection) attacks. This vulnerability could allow the attacker to perform actions, such as, but not limited to, executing system commands (privilege escalation) and unauthorized access to sensitive information (data breach).

### Automatic Query Parameterization
#### Understanding Query Parameterization
A common pattern for creating a dynamic SQL query is to express the query as a string with placeholders for variables. For example, the class method `String.Format` is a convenient way to implement this pattern.

```csharp
// Vulnerable SQLi application code example, username is untrusted data
SqlCommand cmd = new SqlCommand();
cmd.Connection = new SqlConnection("connection_string");
cmd.CommandText = String.Format("SELECT * FROM UserTable WHERE uname = '{0}'", username); // Vulnerable to SQLi
var dataReader = await cmd.ExecuteReaderAsync();
...
```
An approach to remediate the above vulnerable code is to use [SQL parameters](https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlcommand.parameters). When using SQL parameters, a unique identifier for the parameter is inserted into the query. Then a SQL parameter object is created with that unique identifier and the value of the untrusted data is read into the parameter. Other settings such as the parameter type and length might be set, but that is not always the case depending on the database technology or platform used. 
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
The SQL interpreter processes the query and treats the parameter as data-only (i.e., non-executable) which mitigates any risk from SQLi attacks.

Using parameters is an effective way to mitigate risk from SQLi attacks; however, it can become tedious and error-prone, especially in scenarios where queries contain large number of variables or the query significantly changes. Certainly, the convenience to the developer of using a single line of code to create a SQL query is lost. 

Developers are ultimately required to use a pattern that they may not be familiar with, difficult to maintain and that requires more code. Put another way, developers are less likely to use this security best-practice pattern.

#### The AntiSQLi Library Helps Make .NET Applications More Secure
The **AntiSQLi library provides class extensions that automatically parameterizes and loads dynamic queries**. This means that developers can continue to use same single line pattern that they are used to, and let the AntiSQLi library handles the task of query parameterization. Let's take a brief look at how this works:

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
```
The second task performed by the library is it replaces the original format-items in the query to match the IDs of the generated parameters. The original query:
````csharp
cmd.CommandText = "SELECT * FROM UserTable WHERE uname = '{0}'";
````
would be replaced at runtime with:
````csharp
cmd.CommandText = "SELECT * FROM UserTable WHERE uname = '@AntiSQLiParam1'";
````
The parameterized query is passed to the SQL interpreter for processing with the call to `await cmd.ExecuteReaderAsync();` and any risk from SQLi attacks is properly mitigated.

With the AntiSQLi library, developers can continue using the SQL coding patterns that they are already familiar with and let the library take care of applying security best practices in an easy-to-use and repeatable way.

## About
- `2012` Kevin Lam ([IronBox](https://www.goironbox.com)) and Joe Basirico ([Security Innovation](https://www.securityinnovation.com)) were thinking of ways to help .NET developers more easily defend their applications against SQL injection (SQLi) attacks, the #1 web application attack then. 
- `2013` The [initial version of the AntiSQLi Library](https://github.com/IronBox/AntiSQLi) was developed and released.
- `2020` SQLi continued to be the #1 web application attack; however, the surface area for this attack had greatly expanded. In response, the AntiSQLi library was completely re-written with the goals of improving ease-of-use for developers, integration and protection coverage.
