namespace Checknote.Deployment.Database;

using System;

public static class Program
{
    public static int Main(string[] args)
    {
        return ChecknoteDatabaseTool.Run(args, Console.Out, Console.Error);
    }
}
