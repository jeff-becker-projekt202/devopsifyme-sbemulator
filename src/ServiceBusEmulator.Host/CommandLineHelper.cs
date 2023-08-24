namespace ServiceBusEmulator.Host;

public class CommandLineHelper
{
    public static string[] TransformArgs(string[] args)
    {
        return IndexifyArgs(args);
    }

    // This function turns repeated arguments such as "--queue abcd --queue defg" into indexed arguments "--queue:0 abcd --queue:1 defg"
    // so they can map into the configs correctly
    private static string[] IndexifyArgs(string[] args)
    {
        var rewriteTracker = new Dictionary<string, List<int>>();
        for (int i = 0; i < args.Length; i++)
        {
            if (!rewriteTracker.ContainsKey(args[i]))
            {
                rewriteTracker[args[i]] = new List<int>();
            }
            rewriteTracker[args[i]].Add(i);
        }
        var rewritesToExecute = rewriteTracker.Where(p => p.Value.Count > 1);
        foreach (var (key, indexes) in rewritesToExecute)
        {
            for (int i = 0; i < indexes.Count; i++)
            {
                args[indexes[i]] = $"{key}:{i}";
            }
        }
        return args;
    }
}
