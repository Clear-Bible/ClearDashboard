To create a migration do the following:
1. From the `ClearDashboard.DAL.Data` project, right click and choose:  `Open in Terminal`
2. In the terminal type `dotnet-ef migrations add <Your Migration Name>`
This will create a new migration in the Migrations folder and will update the ProjectDbContextModelSnapshot file.