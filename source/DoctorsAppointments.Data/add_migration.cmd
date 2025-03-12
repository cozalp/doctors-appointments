@echo on

set /p "migrationname=Enter new migration name: "

dotnet ef migrations add %migrationname% --context AppDbContext


:End
