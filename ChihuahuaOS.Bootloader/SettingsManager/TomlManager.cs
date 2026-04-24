using System.Collections.Generic;
using System.IO;

namespace ChihuahuaOS.Bootloader.SettingsManager;

//DISCLAIMER!!! This is not a complete TOML 0.1 parser, as it doesn't support arrays, it only works with ASCII,
// and it might not work properly in different edge cases

public static class TomlManager
{
    private enum ReaderState
    {
        None = 0,
        Hash = 1,
        Key = 2,
        Value = 3,
        Comment = 4,

        /// <summary>
        /// This is when the key is set, but there is some white space or the equals sign between the key and the value.
        /// This must be entered after the key (since there is at least the '=' in between).
        /// </summary>
        KeyValueIntermediate = 5
    }

    public static unsafe List<TomlSetting> ReadFromStream(Stream stream, int estimatedEntries = 0)
    {
        stream.Position = 0;
        List<TomlSetting> entries = new(estimatedEntries);

        byte[] rawData = new byte[stream.Length];
        int bufLength = stream.Read(rawData, 0, rawData.Length);

        fixed (byte* rawChars = rawData)
        {
            //finite state machine
            int i = 0;
            var state = ReaderState.None;
            int identifierStartIdx = -1;

            //these won't be disposed here, as TomlSetting will take over the ownership
            string hash = string.Empty;
            string key = string.Empty;
            string value = string.Empty;

            while (i < bufLength)
            {
                char c = (char)rawChars[i];
                switch (state)
                {
                    case ReaderState.None:
                    {
                        if (key != string.Empty && value != string.Empty)
                        {
                            TomlType dataType = GetValueType(value);
                            TomlSetting setting = new(key, dataType, value, hash);
                            entries.Add(setting);

                            hash = string.Empty;
                            key = string.Empty;
                            value = string.Empty;
                        }

                        switch (c)
                        {
                            case '[':
                                state = ReaderState.Hash;
                                break;
                            case '#':
                                state = ReaderState.Comment;
                                break;
                            case '\n':
                                break;
                            default:
                                state = ReaderState.Key;
                                break;
                        }

                        identifierStartIdx = i;
                        break;
                    }
                    case ReaderState.Hash:
                    {
                        if (c != ']')
                        {
                            break;
                        }

                        state = ReaderState.None;
                        hash = CreateString(rawChars, identifierStartIdx, i - 1);
                        break;
                    }
                    case ReaderState.Comment:
                    {
                        if (c == '\n')
                        {
                            state = ReaderState.None;
                        }

                        break;
                    }
                    case ReaderState.Key:
                    {
                        if (c != ' ' && c != '=')
                        {
                            break;
                        }

                        state = ReaderState.KeyValueIntermediate;
                        key = CreateString(rawChars, identifierStartIdx, i - 1);
                        break;
                    }
                    case ReaderState.KeyValueIntermediate:
                    {
                        if (c != ' ' && c != '=')
                        {
                            state = ReaderState.Value;
                            identifierStartIdx = i;
                        }

                        break;
                    }
                    case ReaderState.Value:
                    {
                        //if not a newline and not the last character
                        if (c != '\n' && i < bufLength - 1)
                        {
                            break;
                        }

                        state = ReaderState.None;
                        value = CreateString(rawChars, identifierStartIdx, i - 1);
                        break;
                    }
                }

                i++;
            }

            //if there is a single final value that wasn't added yet (the last entry)
            if (key != string.Empty && value != string.Empty)
            {
                TomlType dataType = GetValueType(value);
                TomlSetting setting = new(key, dataType, value, hash);
                entries.Add(setting);
            }
        }

        rawData.Dispose();
        return entries;
    }

    public static bool WriteToStream(Stream stream, List<TomlSetting> settings)
    {
        stream.Position = 0;
        int numEntriesHandled = 0;
        List<string> handledHashes = [];

        //TODO: replace logic with a dictionary of lists

        //first, handle entries that aren't part of a hash
        for (int i = 0; i < settings.Count; i++)
        {
            if (settings[i].Hash == string.Empty)
            {
                WriteEntry(stream, settings[i]);
                numEntriesHandled++;
            }
        }

        //then, handle the remaining entries
        const int LOOP_GUARD = 1000;
        int loops = 0;
        while (numEntriesHandled < settings.Count && loops < LOOP_GUARD)
        {
            string? hash = null;
            for (int i = 0; i < settings.Count; i++)
            {
                if (handledHashes.Contains(settings[i].Hash))
                {
                    continue;
                }

                hash = settings[i].Hash;
                break;
            }

            if (hash == null)
            {
                loops++;
                continue;
            }

            stream.WriteByte((byte)'\n');

            using string hashRecord = "[" + hash + "]\n";
            byte[] buffer = new byte[hashRecord.Length];
            for (int i = 0; i < hashRecord.Length; i++)
            {
                buffer[i] = (byte)hashRecord[i];
            }

            stream.Write(buffer, 0, buffer.Length);
            buffer.Dispose();

            for (int i = 0; i < settings.Count; i++)
            {
                if (settings[i].Hash == hash)
                {
                    WriteEntry(stream, settings[i]);
                    numEntriesHandled++;
                }
            }

            loops++;
            handledHashes.Add(hash);
        }

        handledHashes.Dispose();
        return loops < LOOP_GUARD;
    }

    private static void WriteEntry(Stream stream, TomlSetting entry)
    {
        byte[] buffer = new byte[entry.Key.Length];
        for (int i = 0; i < entry.Key.Length; i++)
        {
            buffer[i] = (byte)entry.Key[i];
        }

        stream.Write(buffer, 0, buffer.Length);
        buffer.Dispose();

        // ReSharper disable once UseUtf8StringLiteral
        buffer = [(byte)' ', (byte)'=', (byte)' '];
        stream.Write(buffer, 0, buffer.Length);
        buffer.Dispose();

        using string valueAsStr = entry.Value;
        buffer = new byte[valueAsStr.Length];
        for (int i = 0; i < valueAsStr.Length; i++)
        {
            buffer[i] = (byte)valueAsStr[i];
        }

        stream.Write(buffer, 0, buffer.Length);
        stream.WriteByte((byte)'\n');
        buffer.Dispose();
    }

    private static unsafe string CreateString(byte* rawData, int start, int end)
    {
        int numChars = end - start + 1;
        char[] chars = new char[numChars];

        int charsIdx = 0;
        for (int i = start; i <= end; i++)
        {
            //NOTE: this only supports ASCII
            chars[charsIdx] = (char)rawData[i];
            charsIdx++;
        }

        string str = new(chars);
        chars.Dispose();
        return str;
    }

    private static TomlType GetValueType(string value)
    {
        if (value.StartsWith('\"'))
        {
            return TomlType.String;
        }

        if (value.ToLowerInvariant() == "true" || value.ToLowerInvariant() == "false")
        {
            return TomlType.Boolean;
        }

        bool containsDecimalPoint = value.Contains('.');
        if (containsDecimalPoint)
        {
            //TODO: add TryParse to float
            return TomlType.None;
        }

        return long.TryParse(value, out long _) ? TomlType.Integer : TomlType.None;
    }
}