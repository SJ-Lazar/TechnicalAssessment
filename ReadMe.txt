
# Overview 
 - Development Task: User Hub for Management of Users and Permissions

## Entities
	- Users
	- Group
	- Permissions

### Functionality
 - User can belong to multiple groups
 - Groups can contain multiple users
 - Each group must be associated with mutiple permissions
 
#### Features
 - Add Users
 - Update users 
 - Remove users 
 - Total user count
 - Return number of users per group

 ##### Technologies Used
 - Programming Language: C#
 - Framework: .NET Core
 - Database: SQLLite 
 - ORM: Entity Framework Core
 - API: ASP.NET Core Web API
 - Testing: NUnit

 ##### Setup Instructions
 - Clone the repository
 - Navigate to the project directory
 - Run `dotnet restore` to install dependencies
 - Run `dotnet ef database update` to create the database


 ##### Running the Application
 - Set Up WebService and WebInterface as startup projects
 - Run the application using `dotnet run` or through IDE
 - Access the API at `https://localhost:{port}/api/users`
 - Access the Web Interface at `https://localhost:{port}/`
 
 ##### Testing
 - Navigate to the test project directory
 - Run `dotnet test` to execute the unit tests







