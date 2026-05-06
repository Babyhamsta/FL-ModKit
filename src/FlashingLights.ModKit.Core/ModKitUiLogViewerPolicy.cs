namespace FlashingLights.ModKit.Core;

internal static class ModKitUiLogViewerPolicy
{
    public static string TakeLastLines(IEnumerable<string> lines, int count)
    {
        if (count <= 0)
        {
            return string.Empty;
        }

        var queue = new Queue<string>(count);
        foreach (var line in lines)
        {
            if (queue.Count == count)
            {
                queue.Dequeue();
            }

            queue.Enqueue(line);
        }

        return string.Join('\n', queue);
    }
}
