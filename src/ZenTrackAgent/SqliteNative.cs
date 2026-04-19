using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace ZenTrackAgent
{
    internal sealed class SqliteNativeConnection : IDisposable
    {
        private readonly string _databasePath;
        private IntPtr _database;

        public SqliteNativeConnection(string databasePath)
        {
            _databasePath = databasePath;
        }

        public void Open()
        {
            if (_database != IntPtr.Zero)
            {
                return;
            }

            string fullPath = Path.GetFullPath(_databasePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? AppDomain.CurrentDomain.BaseDirectory);

            int result = NativeMethods.sqlite3_open_v2(
                fullPath,
                out _database,
                NativeMethods.SQLITE_OPEN_READWRITE | NativeMethods.SQLITE_OPEN_CREATE | NativeMethods.SQLITE_OPEN_NOMUTEX,
                IntPtr.Zero);

            ThrowIfError(result, _database, "Unable to open SQLite database.");
        }

        public void ExecuteNonQuery(string sql, params SqliteParameter[] parameters)
        {
            ExecuteScalarInternal(sql, false, parameters);
        }

        public long ExecuteScalarInt64(string sql, params SqliteParameter[] parameters)
        {
            return ExecuteScalarInternal(sql, true, parameters);
        }

        public List<T> ExecuteQuery<T>(string sql, Func<SqliteRowReader, T> map, params SqliteParameter[] parameters)
        {
            EnsureOpen();

            IntPtr statement;
            int result = NativeMethods.sqlite3_prepare_v2(_database, sql, -1, out statement, IntPtr.Zero);
            ThrowIfError(result, _database, "Unable to prepare SQL: " + sql);

            try
            {
                BindParameters(statement, parameters);
                List<T> rows = new List<T>();

                while (true)
                {
                    result = NativeMethods.sqlite3_step(statement);
                    if (result == NativeMethods.SQLITE_ROW)
                    {
                        rows.Add(map(new SqliteRowReader(statement)));
                    }
                    else if (result == NativeMethods.SQLITE_DONE)
                    {
                        return rows;
                    }
                    else
                    {
                        ThrowIfError(result, _database, "SQLite query failed: " + sql);
                    }
                }
            }
            finally
            {
                NativeMethods.sqlite3_finalize(statement);
            }
        }

        private long ExecuteScalarInternal(string sql, bool expectScalar, params SqliteParameter[] parameters)
        {
            EnsureOpen();

            IntPtr statement;
            int result = NativeMethods.sqlite3_prepare_v2(_database, sql, -1, out statement, IntPtr.Zero);
            ThrowIfError(result, _database, "Unable to prepare SQL: " + sql);

            try
            {
                BindParameters(statement, parameters);

                long scalar = 0;
                while (true)
                {
                    result = NativeMethods.sqlite3_step(statement);
                    if (result == NativeMethods.SQLITE_ROW)
                    {
                        if (expectScalar)
                        {
                            scalar = NativeMethods.sqlite3_column_int64(statement, 0);
                        }
                    }
                    else if (result == NativeMethods.SQLITE_DONE)
                    {
                        return scalar;
                    }
                    else
                    {
                        ThrowIfError(result, _database, "SQLite command failed: " + sql);
                    }
                }
            }
            finally
            {
                NativeMethods.sqlite3_finalize(statement);
            }
        }

        private static void BindParameters(IntPtr statement, IReadOnlyList<SqliteParameter> parameters)
        {
            if (parameters == null)
            {
                return;
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                SqliteParameter parameter = parameters[i];
                int index = NativeMethods.sqlite3_bind_parameter_index(statement, parameter.Name);
                if (index == 0)
                {
                    throw new InvalidOperationException("SQLite parameter not found in SQL: " + parameter.Name);
                }

                int result;
                if (parameter.Value == null)
                {
                    result = NativeMethods.sqlite3_bind_null(statement, index);
                }
                else if (parameter.Value is int)
                {
                    result = NativeMethods.sqlite3_bind_int(statement, index, (int)parameter.Value);
                }
                else if (parameter.Value is long)
                {
                    result = NativeMethods.sqlite3_bind_int64(statement, index, (long)parameter.Value);
                }
                else
                {
                    string text = Convert.ToString(parameter.Value);
                    result = NativeMethods.sqlite3_bind_text(statement, index, text, -1, new IntPtr(-1));
                }

                if (result != NativeMethods.SQLITE_OK)
                {
                    throw new Win32Exception(result, "Failed to bind SQLite parameter " + parameter.Name);
                }
            }
        }

        private void EnsureOpen()
        {
            if (_database == IntPtr.Zero)
            {
                throw new InvalidOperationException("SQLite database is not open.");
            }
        }

        private static void ThrowIfError(int result, IntPtr database, string message)
        {
            if (result == NativeMethods.SQLITE_OK || result == NativeMethods.SQLITE_ROW || result == NativeMethods.SQLITE_DONE)
            {
                return;
            }

            string sqliteMessage = database == IntPtr.Zero
                ? "SQLite error code " + result
                : Marshal.PtrToStringAnsi(NativeMethods.sqlite3_errmsg(database));

            throw new InvalidOperationException(message + " " + sqliteMessage);
        }

        public void Dispose()
        {
            if (_database != IntPtr.Zero)
            {
                NativeMethods.sqlite3_close(_database);
                _database = IntPtr.Zero;
            }
        }
    }

    internal sealed class SqliteParameter
    {
        public SqliteParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }

        public object Value { get; private set; }
    }

    internal sealed class SqliteRowReader
    {
        private readonly IntPtr _statement;

        public SqliteRowReader(IntPtr statement)
        {
            _statement = statement;
        }

        public bool IsNull(int ordinal)
        {
            return NativeMethods.sqlite3_column_type(_statement, ordinal) == NativeMethods.SQLITE_NULL;
        }

        public int GetInt32(int ordinal)
        {
            return (int)NativeMethods.sqlite3_column_int64(_statement, ordinal);
        }

        public long GetInt64(int ordinal)
        {
            return NativeMethods.sqlite3_column_int64(_statement, ordinal);
        }

        public string GetString(int ordinal)
        {
            IntPtr value = NativeMethods.sqlite3_column_text(_statement, ordinal);
            return value == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(value);
        }
    }

    internal static class NativeMethods
    {
        public const int SQLITE_OK = 0;
        public const int SQLITE_NULL = 5;
        public const int SQLITE_ROW = 100;
        public const int SQLITE_DONE = 101;

        public const int SQLITE_OPEN_READWRITE = 0x00000002;
        public const int SQLITE_OPEN_CREATE = 0x00000004;
        public const int SQLITE_OPEN_NOMUTEX = 0x00008000;

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int sqlite3_open_v2(string filename, out IntPtr database, int flags, IntPtr zvfs);

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int sqlite3_close(IntPtr database);

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int sqlite3_prepare_v2(IntPtr database, string sql, int numBytes, out IntPtr statement, IntPtr tail);

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_step(IntPtr statement);

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_finalize(IntPtr statement);

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int sqlite3_bind_parameter_index(IntPtr statement, string name);

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_bind_null(IntPtr statement, int index);

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_bind_int(IntPtr statement, int index, int value);

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_bind_int64(IntPtr statement, int index, long value);

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int sqlite3_bind_text(IntPtr statement, int index, string value, int length, IntPtr destructor);

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long sqlite3_column_int64(IntPtr statement, int columnIndex);

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_column_type(IntPtr statement, int columnIndex);

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sqlite3_column_text(IntPtr statement, int columnIndex);

        [DllImport("winsqlite3.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sqlite3_errmsg(IntPtr database);
    }
}
