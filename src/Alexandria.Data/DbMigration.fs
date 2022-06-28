module Alexandria.Data.DbMigration

open System.Reflection


open DbUp

let migrateDb connectionString =
        EnsureDatabase.For
            .MySqlDatabase(connectionString)


        let upgrader =
            DeployChanges.To
                .MySqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .WithTransactionPerScript()
                .LogToConsole()
                .LogScriptOutput()
                .Build()

        upgrader.PerformUpgrade()