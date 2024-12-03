using System;

class Program
{
    static void Main()
    {
        List<string[]> a = new List<string[]>();
        string input = "SET,X0,3,128,24,1/GET,Y0,2/GET,D0,3";
        a = SplitCommands(input);

    }

    static List<string[]> SplitCommands(string input)
    {
        string[] commands = input.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        List<string[]> commandList = new List<string[]>();
        foreach (string command in commands)
        {
            string[] commands2nd = command.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            commandList.Add(commands2nd);
        }

        return commandList;
    }
}
