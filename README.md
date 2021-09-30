  
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
|||
|2     |**[How to use](#how-to-use)**  |
|2.1   |&nbsp;&nbsp; **[Prepare your Entity](#prepare-your-entity)**  |
|2.1.1 |&nbsp; &nbsp; &nbsp; &nbsp; **[Table Attribute](#table-attribute)**  |
|2.1.2 |&nbsp; &nbsp; &nbsp; &nbsp; **[IsPrimaryKey Attribute](#isprimarykey-attribute)**  |
|2.1.3 |&nbsp; &nbsp; &nbsp; &nbsp; **[IsNotInsertable Attribute](#isnotinsertable-attribute)**  |
|2.1.4 |&nbsp; &nbsp; &nbsp; &nbsp; **[IsNotUpdatable Attribute](#isnotupdatable-attribute)**  |
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

# How to use

Define a FastqlBuilder variable in your database related class. *TEntity* should be your poco to generate Insert and Update queries.

⚡ This class fits perfect to Repository Pattern with Dapper Micro ORM.

```
   private FastqlBuilder<TEntity> fastql;
```

Constructor needs an object to handle attributes of the demanded class at runtime. If you don't have any at design time, you can initialize your fastql like this:

```
fastql = new FastqlBuilder<TEntity>((TEntity)Activator.CreateInstance(typeof(TEntity)));
```
(*Activator is in namespace System in assembly System.Runtime.dll*)

## Prepare Your Entity

Fastql has 4 attributes to handle your queries.

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


## APIs

⚡ TableName <br>
⚡ InsertQuery <br>
⚡ UpdateQuery <br>


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

*TableName()* function returns you the **schema** and **table** for ready-to-use in select query.

```
Connection.Query<TEntity>(
                  $"SELECT * FROM {fastql.TableName()} WHERE Id=@Id",
                  param: new { Id = id },
                  transaction: Transaction
              );
```

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
*TableName()* function can be used for delete operation too. You can handle your where condition with your way. 

```
Connection.Execute(
                  $"DELETE FROM {fastql.TableName()} WHERE {where}",
                  // param: entity,
                  transaction: Transaction
```                  
                

<img src="https://github.com/theilgaz/fastql/blob/main/resource/fastql-amblem.png?raw=true" style="width:100px"/>
