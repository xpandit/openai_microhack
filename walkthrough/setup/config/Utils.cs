// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.ChatCompletion;

// ReSharper disable InconsistentNaming
public static class Utils
{
    // Function used to wrap long lines of text
    public static string WordWrap(string text, int maxLineLength)
    {
        var result = new StringBuilder();
        int i;
        var last = 0;
        var space = new[] { ' ', '\r', '\n', '\t' };
        do
        {
            i = last + maxLineLength > text.Length
                ? text.Length
                : (text.LastIndexOfAny(new[] { ' ', ',', '.', '?', '!', ':', ';', '-', '\n', '\r', '\t' }, Math.Min(text.Length - 1, last + maxLineLength)) + 1);
            if (i <= last) i = Math.Min(last + maxLineLength, text.Length);
            result.AppendLine(text.Substring(last, i - last).Trim(space));
            last = i;
        } while (i < text.Length);

        return result.ToString();
    }

    // Function to print the latest message in the chat history
    public static Task MessageOutputAsync(ChatHistory chatHistory)
    {
        var message = chatHistory.Last();
        Console.WriteLine($"{message.Role}: {message.Content}");
        Console.WriteLine("------------------------");

        return Task.CompletedTask;
    }
}