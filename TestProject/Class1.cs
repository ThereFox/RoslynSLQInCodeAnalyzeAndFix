namespace TestProject;

public class Class1
{
    public string SQLCommand = "SELECT * FROM Test";

    public void Execute()
    {
        var sql = "SELECT * FROM Test";
        var insert = "INSERT INTO Test VALUES (@param1, @param2)";
        var update = "UPDATE Test SET @param1 = @param2 WHERE @param1 = @param2";
        
        Console.WriteLine(sql);
    }
}