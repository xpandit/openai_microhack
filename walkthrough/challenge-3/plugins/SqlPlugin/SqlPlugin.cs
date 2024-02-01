using System;
using System.ComponentModel;
using System.Data.SQLite;
using Microsoft.SemanticKernel;
using System.Threading.Tasks;
using Dapper;

public class SqlPlugin
{
    private const string DefaultDatabaseName = "northwind.db";

    [KernelFunction, Description("Obtain the table names in northwind sqlite database, which contains customer, products, orders and other data. Always run this before running other queries instead of assuming the user mentioned the correct name.")]
    public async Task<string> GetTables()
    {
        Console.WriteLine("Getting tables...");
        return QueryAsCSV($"SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;");
    }

    [KernelFunction, Description(@"
Get the schema for a table in the northwind sqlite database.
Adhere to these rules:
- The table to get the schema for.
- **Do not** include the schema name.
- Respect the table names from a previous step and do not change the casing of table names
    ")]
    public async Task<string> GetSchema(
    [Description(@"The table to get the schema for. Do not include the schema name.")] string tableName)
    {
        Console.WriteLine($"Getting schema for {tableName}...");
        return QueryAsCSV($"PRAGMA table_info({tableName});");
    }


    [KernelFunction, Description(@"
Run SQL against the northwind database
Adhere to these rules:
- **Deliberately go through the question and database schema word by word** to appropriately answer the question
- **Use Table Aliases** to prevent ambiguity. For example, `SELECT table1.col1, table2.col1 FROM table1 JOIN table2 ON table1.id = table2.id`.
- When creating a ratio, always cast the numerator as float
    ")]
    public async Task<string> RunQuery(
    [Description(@"The query to run on northwind sqlite database.")] string query)
    {
        Console.WriteLine($"Running query...");
        return QueryAsCSV(query);
    }

    // Function to Query a SQLite database as csv
    private static string QueryAsCSV(string query)
    {
        Console.WriteLine($"Querying database with query: {query}");

        var output = string.Empty;

        try
        {
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={DefaultDatabaseName}"))
            {
                connection.Open();
                var results = connection.Query(query);
                output = ConvertToCsv(results);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return output;
    }

    private static string ConvertToCsv(IEnumerable<dynamic> dapperRows)
    {
        // Check if there are any rows
        if (!dapperRows.Any())
            return "No data available";

        // Convert the first row to IDictionary to get column names
        var firstRow = (IDictionary<string, object>)dapperRows.First();
        var columnNames = firstRow.Keys;

        // Start building the CSV
        var csv = new System.Text.StringBuilder();

        // Add header
        csv.AppendLine(string.Join(",", columnNames));

        // Add rows
        foreach (IDictionary<string, object> row in dapperRows)
        {
            csv.AppendLine(string.Join(",", row.Values));
        }

        return csv.ToString();
    }
}