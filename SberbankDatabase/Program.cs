using Npgsql;
using System.Text;

const string connectionString = "Host=localhost;Username=postgres;Password=sudo1010;Database=sberbank_db";

#region Startup
ShowVersion();

Console.WriteLine();

Console.WriteLine("If you want to close app - press \"Ctrl + C\" or \"Ctrl + Break\".");

Console.WriteLine();

ShowTableAccounts();

Console.WriteLine();

ShowTableDeposits();

Console.WriteLine();

ShowTableUsers();

Console.WriteLine();

Console.WriteLine("Possible commands: accounts add, deposits add, users add.");

Console.WriteLine();
#endregion

while (true)
{
    switch (Console.ReadLine().Trim())
    {
        case "accounts add":
            {
                Console.WriteLine("Enter id of user:");
                var userId = Console.ReadLine();

                Console.WriteLine("Enter type of account:");
                var accountType = Console.ReadLine();

                Console.WriteLine("Enter account start value:");
                var value = Console.ReadLine();

                Console.WriteLine("Enter value units:");
                var units = Console.ReadLine();

                AddAccount(userId, accountType, value, units);

                Console.WriteLine();

                ShowTableAccounts();
            }
            break;
        case "deposits add":
            {
                Console.WriteLine("Enter id of sender:");
                var fromId = Console.ReadLine();

                Console.WriteLine("Enter id of responder:");
                var toId = Console.ReadLine();

                Console.WriteLine("Enter value:");
                var value = Console.ReadLine();

                Console.WriteLine("Enter units:");
                var units = Console.ReadLine();

                AddDeposit(fromId, toId, value, units);

                Console.WriteLine();

                ShowTableDeposits();
            }
            break;
        case "users add":
            {
                Console.WriteLine("Enter first name:");
                var firstName = Console.ReadLine();

                Console.WriteLine("Enter middle name:");
                var midName = Console.ReadLine();

                Console.WriteLine("Enter last name:");
                var lastName = Console.ReadLine();

                AddUser(firstName, midName, lastName);

                Console.WriteLine();

                ShowTableUsers();
            }
            break;
        default:
            Console.WriteLine("Non executable command! Allowed commands: accounts add, deposits add, users add.");
            break;
    }
}

static void ShowVersion()
{
    using var con = new NpgsqlConnection(connectionString);
    con.Open();

    var sql = "SELECT version()";

    using var cmd = new NpgsqlCommand(sql, con);

    var version = cmd.ExecuteScalar().ToString();

    Console.WriteLine($"PostgreSQL version: {version}");

    con.Close();
}

#region Addings
static void AddUser(string firstName, string midName, string lastName)
{
    using var con = new NpgsqlConnection(connectionString);
    con.Open();

    var sql = "INSERT INTO users(user_first_name, user_mid_name, user_last_name) " +
        "VALUES(@firstName, @midName, @lastName)";
    using var cmd = new NpgsqlCommand(sql, con);

    cmd.Parameters.AddWithValue("firstName", firstName);
    cmd.Parameters.AddWithValue("midName", midName);
    cmd.Parameters.AddWithValue("lastName", lastName);
    cmd.Prepare();

    cmd.ExecuteNonQuery();
    con.Close();
}

static void AddAccount(string userId, string accountType, string value, string units)
{
    using var con = new NpgsqlConnection(connectionString);
    con.Open();

    var sql = "INSERT INTO accounts(account_user, account_type, account_value, account_units) " +
        "VALUES(@userId, @accountType, @value, @units)";
    using var cmd = new NpgsqlCommand(sql, con);

    cmd.Parameters.AddWithValue("userId", int.Parse(userId));
    cmd.Parameters.AddWithValue("accountType", accountType);
    cmd.Parameters.AddWithValue("value", double.Parse(value));
    cmd.Parameters.AddWithValue("units", units);
    cmd.Prepare();

    cmd.ExecuteNonQuery();
    con.Close();
}

static void AddDeposit(string fromId, string toId, string value, string units)
{
    using var con = new NpgsqlConnection(connectionString);

    con.Open();

    var sql = "INSERT INTO deposits(deposit_from, deposit_to, deposit_date, deposit_time, deposit_value, deposit_units) " +
        "VALUES(@fromId, @toId, @dateNow, @timeNow, @value, @units)";
    using var cmd = new NpgsqlCommand(sql, con);

    cmd.Parameters.AddWithValue("fromId", int.Parse(fromId));
    cmd.Parameters.AddWithValue("toId", int.Parse(toId));
    cmd.Parameters.AddWithValue("dateNow", DateOnly.FromDateTime(DateTime.Now));
    cmd.Parameters.AddWithValue("timeNow", TimeOnly.FromDateTime(DateTime.Now));
    cmd.Parameters.AddWithValue("value", double.Parse(value));
    cmd.Parameters.AddWithValue("units", units);
    cmd.Prepare();

    cmd.ExecuteNonQuery();
    con.Close();
}
#endregion

#region Showings
static void ShowTableUsers()
{
    using var con = new NpgsqlConnection(connectionString);
    con.Open();

    string sql = "SELECT * FROM users";
    using var cmd = new NpgsqlCommand(sql, con);

    Console.WriteLine("|users table|");

    using NpgsqlDataReader rdr = cmd.ExecuteReader();
    StringBuilder stringBuilder = new StringBuilder();

    stringBuilder.Append("|");

    for (int i = 0; i < 4; i++)
    {
        stringBuilder.Append(rdr.GetName(i));
        stringBuilder.Append("|");
    }

    Console.WriteLine(stringBuilder.ToString());

    while (rdr.Read())
    {
        Console.WriteLine("|{0}|{1}|{2}|{3}|",
                rdr.GetInt32(0),
                rdr.GetString(1),
                rdr.GetString(2),
                rdr.GetString(3));
    }
    con.Close();
}

static void ShowTableAccounts()
{
    using var con = new NpgsqlConnection(connectionString);
    con.Open();

    string sql = "SELECT * FROM accounts";
    using var cmd = new NpgsqlCommand(sql, con);

    Console.WriteLine("|accounts table|");

    using NpgsqlDataReader rdr = cmd.ExecuteReader();
    StringBuilder stringBuilder = new StringBuilder();

    stringBuilder.Append("|");

    for (int i = 0; i < 5; i++)
    {
        stringBuilder.Append(rdr.GetName(i));
        stringBuilder.Append("|");
    }

    Console.WriteLine(stringBuilder.ToString());

    while (rdr.Read())
    {
        Console.WriteLine("|{0}|{1}|{2}|{3}|{4}|",
                rdr.GetInt32(0),
                rdr.GetInt32(1),
                rdr.GetString(2),
                rdr.GetDouble(3),
                rdr.GetString(4));
    }
    con.Close();
}

static void ShowTableDeposits()
{
    using var con = new NpgsqlConnection(connectionString);
    con.Open();

    string sql = "SELECT * FROM deposits";
    using var cmd = new NpgsqlCommand(sql, con);

    Console.WriteLine("|deposits table|");

    using NpgsqlDataReader rdr = cmd.ExecuteReader();
    StringBuilder stringBuilder = new StringBuilder();

    stringBuilder.Append("|");

    for (int i = 0; i < 7; i++)
    {
        stringBuilder.Append(rdr.GetName(i));
        stringBuilder.Append("|");
    }

    Console.WriteLine(stringBuilder.ToString());

    while (rdr.Read())
    {
        Console.WriteLine("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|",
                rdr.GetInt32(0),
                rdr.GetInt32(1),
                rdr.GetInt32(2),
                rdr.GetDateTime(3).ToShortDateString(),
                rdr.GetTimeSpan(4),
                rdr.GetDouble(5),
                rdr.GetString(6));
    }
    con.Close();
}
#endregion