namespace TestProject;

public class Class1
{
    public string SQLCommand = "SELECT * FROM Test";

    public void Execute()
    {
        var sql = "SELECT * FROM Test";
        var insert = "INSERT INTO Test VALUES (@param1, @param2)";
        var update = "UPDATE Test SET @param1 = @param2 WHERE @param1 = @param2";

        object test123;
        
        test123 = 123;
        
        var te = 123;
        
        object test = 123;
        
        Console.WriteLine(Generated_Name.NameOfClass);
        Console.WriteLine(Generated_SuperClassName.NameOfClass);
        sql = "test";
        
        Console.WriteLine(sql);
    }
}