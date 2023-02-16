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

                Console.WriteLine();

                ShowTableAccounts();
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

    var sql = "SELECT account_value FROM accounts WHERE account_id = @fromId AND account_units = @units";

    var cmd = new NpgsqlCommand(sql, con);

    cmd.Parameters.AddWithValue("fromId", int.Parse(fromId));
    cmd.Parameters.AddWithValue("units", units);
    cmd.Prepare();
    var firstResult = cmd.ExecuteScalar();
    cmd.Dispose();

    sql = "SELECT account_value FROM accounts WHERE account_id = @toId AND account_units = @units";

    cmd = new NpgsqlCommand(sql, con);

    cmd.Parameters.AddWithValue("toId", int.Parse(toId));
    cmd.Parameters.AddWithValue("units", units);
    cmd.Prepare();
    var secondResult = cmd.ExecuteScalar();
    cmd.Dispose();

    if(firstResult != null && secondResult != null)
    {
        sql = "INSERT INTO deposits(deposit_from, deposit_to, deposit_date, deposit_time, deposit_value, deposit_units) " +
        "VALUES(@fromId, @toId, @dateNow, @timeNow, @value, @units);\r\n" +
        $"UPDATE accounts SET account_value = account_value - @value WHERE account_id = @fromId AND account_units = @units;\r\n" +
        $"UPDATE accounts SET account_value = account_value + @value WHERE account_id = @toId AND account_units = @units;\r\n";

        cmd = new NpgsqlCommand(sql, con);

        cmd.Parameters.AddWithValue("fromId", int.Parse(fromId));
        cmd.Parameters.AddWithValue("toId", int.Parse(toId));
        cmd.Parameters.AddWithValue("dateNow", DateOnly.FromDateTime(DateTime.Now));
        cmd.Parameters.AddWithValue("timeNow", TimeOnly.FromDateTime(DateTime.Now));
        cmd.Parameters.AddWithValue("value", double.Parse(value));
        cmd.Parameters.AddWithValue("units", units);
        cmd.Prepare();
        cmd.ExecuteNonQuery();
        cmd.Dispose();
    }
    else
    {
        Console.WriteLine("Can't execute this adding.");
    }
    
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

#region Creations
static void CreateTableUsers()
{
    using var con = new NpgsqlConnection(connectionString);
    con.Open();

    string sql = "CREATE TABLE public.users" +
        "(" +
        "user_id serial NOT NULL, " +
        "user_first_name text, " +
        "user_mid_name text, " +
        "user_last_name text, " +
        "PRIMARY KEY (user_id)" +
        ");";

    using var cmd = new NpgsqlCommand(sql, con);
    cmd.Prepare();
    cmd.ExecuteNonQuery();
    con.Close();
}

static void CreateTableAccounts()
{
    using var con = new NpgsqlConnection(connectionString);
    con.Open();

    string sql = "CREATE TABLE public.accounts" +
        "(" +
        "account_id serial NOT NULL," +
        "account_user bigint REFERENCES users (user_id)," +
        "account_type text," +
        "account_value numeric," +
        "account_units text," +
        "PRIMARY KEY (account_id)" +
        ");";

    using var cmd = new NpgsqlCommand(sql, con);
    cmd.Prepare();
    cmd.ExecuteNonQuery();
    con.Close();
}

static void CreateTableDeposits()
{
    using var con = new NpgsqlConnection(connectionString);
    con.Open();

    string sql = "CREATE TABLE public.deposits" +
    "(" +
    "deposit_id serial NOT NULL," +
    "deposit_from bigint REFERENCES accounts (account_id) NOT NULL," +
    "deposit_to bigint REFERENCES accounts (account_id) NOT NULL," +
    "deposit_date date," +
    "deposit_time time," +
    "deposit_value numeric," +
    "deposit_units text," +
    "PRIMARY KEY (deposit_id)" +
    ");";

    using var cmd = new NpgsqlCommand(sql, con);
    cmd.Prepare();
    cmd.ExecuteNonQuery();
    con.Close();
}
#endregion

#region Fillings
static void FillTableUsers()
{
    using var con = new NpgsqlConnection(connectionString);
    con.Open();

    string sql = "INSERT INTO users (user_mid_name, user_first_name, user_last_name) VALUES" +
    "('Иванов', 'Иван', 'Иванович')," +
    "('Денисов', 'Денис', 'Денисович')," +
    "('Петров', 'Петр', 'Петрович')," +
    "('Александров', 'Александр', 'Александрович')," +
    "('Сергеев', 'Сергей', 'Сергеевич');";

    using var cmd = new NpgsqlCommand(sql, con);
    cmd.Prepare();
    cmd.ExecuteNonQuery();
    con.Close();
}

static void FillTableAccounts()
{
    using var con = new NpgsqlConnection(connectionString);
    con.Open();

    string sql = "INSERT INTO accounts (account_user, account_type, account_value, account_units) VALUES" +
    "(1, 'credit_card', 15000, 'RUB')," +
    "(2, 'credit_card', 200, 'USD')," +
    "(3, 'credit_card', 5500, 'RUB')," +
    "(4, 'credit_card', 4890, 'EUR')," +
    "(5, 'money_box', 245021, 'RUB');";

    using var cmd = new NpgsqlCommand(sql, con);
    cmd.Prepare();
    cmd.ExecuteNonQuery();
    con.Close();
}

static void FillTableDeposits()
{
    using var con = new NpgsqlConnection(connectionString);
    con.Open();

    string sql = "INSERT INTO deposits (deposit_from, deposit_to, deposit_date, deposit_time, deposit_value, deposit_units) VALUES" +
    "(1, 2, '2017-03-14', '16:11:42', 250, 'RUB')," +
    "(1, 3, '2017-03-16', '04:32:15', 100, 'RUB')," +
    "(3, 1, '2017-03-15', '08:21:05', 1500, 'RUB')," +
    "(4, 2, '2017-03-11', '19:07:26', 420, 'RUB')," +
    "(5, 4, '2017-03-15', '23:10:03', 13000, 'RUB');";

    using var cmd = new NpgsqlCommand(sql, con);
    cmd.Prepare();
    cmd.ExecuteNonQuery();
    con.Close();
}
#endregion