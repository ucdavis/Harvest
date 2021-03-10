[ "$#" -eq 1 ] || { echo "1 argument required, $# provided. Useage: sh CreateMigrationAndExecute <MigrationName>"; exit 1; }

dotnet ef migrations add $1 --context AppDbContextSqlite --output-dir Migrations/Sqlite --startup-project ../Harvest.Web/Harvest.Web.csproj -- --provider Sqlite
dotnet ef migrations add $1 --context AppDbContextSqlServer --output-dir Migrations/SqlServer --startup-project ../Harvest.Web/Harvest.Web.csproj -- --provider SqlServer
dotnet ef database update --startup-project ../Harvest.Web/Harvest.Web.csproj --context AppDbContextSqlServer
# dotnet ef database update --startup-project ../Harvest.Web/Harvest.Web.csproj --context AppDbContextSqlite

echo 'All done';