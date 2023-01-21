  
 <div align="center"> 
  
[![license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/theilgaz/fastql/blob/master/LICENSE)  [![NuGet](https://img.shields.io/nuget/v/Fastql)](https://www.nuget.org/packages/Fastql/)   [![NuGet](https://img.shields.io/nuget/dt/Fastql)](https://www.nuget.org/packages/Fastql/) 

   
<img src="https://github.com/theilgaz/fastql/blob/main/resource/fastql-logo-resized.png?raw=true" style="width:250px"/><br>
***Here comes the query!***
   
</div>



# Overview
### What is the ⚡Fastql?
A small and fast library for building SQL queries from entity classes in a better way than regular string concatenation.

## Table of Contents

|#     ||
|------|---------------------------|
|1     |&nbsp; **[Get Started](#get-started)**  |
|1.1   |&nbsp; &nbsp; **[Install Package](#install-package)**  |
|1.2   |&nbsp; &nbsp; **[Import the namespace](#import-the-namespace)**  |
|1.3   |&nbsp; &nbsp; **[Sample Code](#sample-code)**  |
|||
|2     |**[How to use](#how-to-use)**  |
|2.1   |&nbsp;&nbsp; **[Prepare your Entity](#prepare-your-entity)**  |
|2.1.1 |&nbsp; &nbsp; &nbsp; &nbsp; **[Table Attribute](#table-attribute)**  |
|2.1.2 |&nbsp; &nbsp; &nbsp; &nbsp; **[IsPrimaryKey Attribute](#isprimarykey-attribute)**  |
|2.1.3 |&nbsp; &nbsp; &nbsp; &nbsp; **[IsNotInsertable Attribute](#isnotinsertable-attribute)**  |
|2.1.4 |&nbsp; &nbsp; &nbsp; &nbsp; **[IsNotUpdatable Attribute](#isnotupdatable-attribute)**  |
|2.1.5 |&nbsp; &nbsp; &nbsp; &nbsp; **[Field Attribute](#field-attribute)**  |
|2.1.6 |&nbsp; &nbsp; &nbsp; &nbsp; **[CustomField Attribute](#customfield-attribute)**  |
|2.2   |&nbsp;&nbsp; **[APIs](#apis)**  |
|||
|3     |**[Dapper Example](#dapper-example)**  |
|3.1   |&nbsp; &nbsp; **[Crud Operations](#crud-operations)**  |
|3.1.1 |&nbsp; &nbsp; &nbsp; &nbsp; **[Create (Insert)](#create-insert)**  |
|3.1.1 |&nbsp; &nbsp; &nbsp; &nbsp; **[Read (Select)](#read-select)**  |
|3.1.1 |&nbsp; &nbsp; &nbsp; &nbsp; **[Update](#update)**  |
|3.1.1 |&nbsp; &nbsp; &nbsp; &nbsp; **[Delete](#delete)**  |


## Get Started

### Install Package

⚡ Add package to your existing project from NuGet Package Manager. <br>
⚡ Or download the source code from GitHub.

### Import the namespace

```
using Fastsql;
```

### Sample Code
[Check out](https://github.com/theilgaz/fastql-unit-of-work-in-repository-pattern) the Fastql Sample Project with Unit of Work in Repository Pattern using Dapper.

# How to use

Fastql can be used with two way: using **object** and using **helper** class.

## 1. Using FastqlBuilder Way

Define a FastqlBuilder variable in your database related class. *TEntity* should be your poco to generate CRUD queries.

⚡ This class fits perfect to Repository Pattern with Dapper Micro ORM.

```
   private FastqlBuilder<TEntity> fastql = new FastqlBuilder<TEntity>();
```

## 2. Using FastqlHelper Way

Call FastqlHelper with your *TEntity*. *TEntity* should be your poco to generate CRUD queries.

⚡ This class fits perfect to Repository Pattern with Dapper Micro ORM.

```
   FastqlHelper<TEntity>.InsertQuery(); // insert query for TEntity
```


## Prepare Your Entity

Fastql has 5 attributes to handle your queries.

### Table Attribute

Define your table and schema name. If you don't set your schema, it'll use *dbo* as default.

```
 [Table("Customer", "Sales")]
    public class Customer
    {
```
It will be rendered like **[Sales].[Customer]** for your query.

### IsPrimaryKey Attribute

If your database table has PK and auto increment, you should define this attribute to your PK field. If your field has this key, your field won't be used in Insert or Update query.

```
 [Table("Customer", "Sales")]
    public class Customer
    {
        [IsPrimaryKey]
        public int Id { get; set; }
    }
```

### IsNotInsertable Attribute

Fields have this attribute won't be placed in your Insert query.
*You don't need to define this key for your PK field.*

```
[IsNotInsertable]
public DateTime UpdatedOn { get; set; }
```


### IsNotUpdatable Attribute

Fields have this attribute won't be placed in your Update query.
*You don't need to define this key for your PK field.*

```
[IsNotUpdatable]
public DateTime CreatedOn { get; set; }
```

### Field Attribute

Fields have this attribute will be replaced in your Insert and Update query.
*You can define CreatedAt field on your code and your field attribute can contain the original version of database property like created_at.*

```
[Field("created_at")]
public DateTime CreatedAt { get; set; }
```

### CustomField Attribute

Custom Fields have this attribute will be not included in your Select, Insert and Update queries.
*You can define CustomField attribute like this:*

```
[CustomField]
public string FullName => $"{FirstName} {LastName}";
```

## APIs

⚡ TableName 
⚡ InsertQuery 
⚡ UpdateQuery 
⚡ SelectQuery 
⚡ DeleteQuery 



# Dapper Example

⚡Fastql is a great extension for Dapper Micro ORM. You can handle all of your CRUD operations easily with ⚡Fastql.

## CRUD Operations

### Create (Insert)

*InsertQuery()* function returns you the insert query based on your decisions.

```
Connection.Execute(
                fastql.InsertQuery(),
                param: entity,
                transaction: Transaction
               );
```

### Read (Select)

#### SelectQuery(where)
*SelectQuery(where)* function returns you the select query based on your where condition.

```
Connection.Query<TEntity>(
                  fastql.SelectQuery("Id=@Id"),
                  param: new { Id = id },
                  transaction: Transaction
              );
```

#### SelectQuery(columns,where,top)

*SelectQuery(columns,where,top)* function returns you the select query based on desired columns, where conditions and top records.
```
Connection.Query<TEntity>(
                  fastql.SelectQuery(new string[] {"Field1","Field2","Field3"},"Id=@Id"),
                  param: new { Id = id },
                  transaction: Transaction
              );
```
**Sample output**: Select TOP(1000) [Field1],[Field2],[Field3] from [TableName] where Id=@Id

```
Connection.Query<TEntity>(
                  fastql.SelectQuery(new string[] {"Field1","Field2","Field3"},"Id=@Id",500),
                  param: new { Id = id },
                  transaction: Transaction
              );
```
**Sample output**: Select TOP(500) [Field1],[Field2],[Field3] from [TableName] where Id=@Id

### Update

*UpdateQuery(TEntity,string)* returns you the update query with parameter tags added. You can bind your **entity** to query as parameter. Where string can include data or you can set your param tag to use it. *(For.ex.) "Id=@Id" or $"Id={id}"*

```
Connection.Execute(
                  fastql.UpdateQuery(entity, where),
                  param: entity,
                  transaction: Transaction
              );
```
 

### Delete
*DeleteQuery(where)* function returns you the delete query based on your where condition.

```
Connection.Execute(
                  fastql.DeleteQuery(where),
                  // param: entity,
                  transaction: Transaction
              );
```                
                
