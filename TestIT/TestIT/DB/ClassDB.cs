using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TestIT.Interfaces;
using TestIT.Models;

namespace TestIT.DB
{
    class ClassDB
    {
        private static string stringConnection;

        private static string fullPathToDB = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"FileManagerDB.db");

        public static void Init()
        {
            stringConnection = $"Data Source={fullPathToDB}; foreign keys=true; Version=3;";

            if (!File.Exists(fullPathToDB))
            {
                SQLiteConnection.CreateFile(fullPathToDB);
                if (File.Exists(fullPathToDB))
                {
                    CreateTables();
                }
                else MessageBox.Show("Возникла ошибка при создании базы данных");
            }
        }

        private static void CreateTables()
        {
            Assembly asmbly = Assembly.GetExecutingAssembly();
            List<Type> typeList = asmbly.GetTypes().Where(t => t.GetCustomAttributes(typeof(TableAttribute), false).Length > 0).ToList();
            foreach (var temp in typeList)
            {
                CreateTable(temp);
            }
        }

        private static void CreateTable(Type type)
        {
            PropertyInfo[] propertyInfo = type.GetProperties();
            string titleRequest = $"CREATE TABLE IF NOT EXISTS {type.Name.ToLower()}";
            List<string> columns = new List<string>(); //обычные столбцы таблицы
            List<string> primaryColumns = new List<string>(); //столбцы таблицы с первичным ключом
            string foreignText = ""; //для столбцов таблицы с внешним ключом

            for (int i = 0; i < propertyInfo.Length; i++)
            {
                if (!(propertyInfo[i].GetCustomAttributes(typeof(PrimaryKeyAttribute), false).SingleOrDefault() == null))
                    primaryColumns.Add(propertyInfo[i].Name.ToLower());
                var notNull = propertyInfo[i].GetCustomAttributes(typeof(NotNullAttribute), false).SingleOrDefault() as NotNullAttribute;
                var unique = propertyInfo[i].GetCustomAttributes(typeof(UniqueAttribute), false).SingleOrDefault() as UniqueAttribute;


                switch (propertyInfo[i].PropertyType.Name)
                {
                    case "String":
                        columns.Add($"{propertyInfo[i].Name.ToLower()} TEXT {unique?.Text} {notNull?.Text}".TrimEnd(' '));
                        break;
                    case "Boolean":
                    case "Int32":
                        columns.Add($"{propertyInfo[i].Name.ToLower()} INTEGER {unique?.Text} {notNull?.Text}".TrimEnd(' '));
                        break;
                }

                var foreign = propertyInfo[i].GetCustomAttributes(typeof(ForeignKeyAttribute), false).SingleOrDefault() as ForeignKeyAttribute;

                //принимаем, что ссылка у foreign key может быть только на столбец id 
                if (!(foreign == null))
                    foreignText += $", {foreign.Text}({propertyInfo[i].Name.ToLower()}) REFERENCES {foreign.TypeRef.Name.ToLower()}(id)";
            }
            string request = $"{titleRequest} ({string.Join(", ", columns.ToArray())}{foreignText}, PRIMARY KEY ({string.Join(", ", primaryColumns.ToArray())}));";

            using (SQLiteConnection conn = new SQLiteConnection(stringConnection))
            {
                SQLiteCommand command = new SQLiteCommand(request, conn);
                conn.Open();
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void InsertRow<T>(T obj)
        {
            Type type = typeof(T);
            List<string> fields = GetNameProperties(type);
            string comm = GetRowINSERT<T>(type, fields);
            comm += $"SELECT last_insert_rowid();";

            int id;

            using (SQLiteConnection conn = new SQLiteConnection(stringConnection))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand(conn);
                command.CommandText = comm;

                for (int i = 0; i < fields.Count; i++)
                {
                    PropertyInfo fi = type.GetProperty(fields[i]);
                    command.Parameters.AddWithValue(fields[i], fi.GetValue(obj));
                }
                try
                {
                    var t = command.ExecuteScalar();
                    id = int.Parse(t.ToString());
                    PropertyInfo fi_id = type.GetProperty("ID");
                    fi_id?.SetValue(obj, id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                conn.Close();
            }
        }

        public static void DeleteObject<T>(T obj)
        {
            Type type = typeof(T);

            PropertyInfo fi_id = type.GetProperty("ID");
            int id = Convert.ToInt32(fi_id?.GetValue(obj));

            using (SQLiteConnection conn = new SQLiteConnection(stringConnection))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand(conn);
                command.CommandText = $"DELETE FROM {type.Name.ToLower()} WHERE id={id}";

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                conn.Close();
            }

        }

        private static string GetRowUPDATE<T>(Type type, List<string> fields, int id)
        {
            string comm = $"UPDATE {type.Name.ToLower()} SET ";

            for (int i = 0; i < fields.Count; i++)
            {
                if (i != 0)
                    comm += ", ";
                comm += $"{fields[i]} = @{fields[i]}";
            }

            comm += $" WHERE id={id};";
            return comm;
        }

        public static void UpdateObject<T>(T obj)
        {
            Type type = typeof(T);
            List<string> fields = GetNameProperties(type);
            PropertyInfo fi_id = type.GetProperty("ID");
            int id =  Convert.ToInt32(fi_id?.GetValue(obj));

            string comm = GetRowUPDATE<T>(type, fields, id);

            using (SQLiteConnection conn = new SQLiteConnection(stringConnection))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand(conn);
                command.CommandText = comm;

                for (int i = 0; i < fields.Count; i++)
                {
                    PropertyInfo fi = type.GetProperty(fields[i]);
                    command.Parameters.AddWithValue(fields[i], fi.GetValue(obj));
                }
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                conn.Close();
            }

        }


        private static List<string> GetNameProperties(Type type)
        {
            List<string> fields = new List<string>();
            PropertyInfo[] propertyInfo = type.GetProperties();
            foreach (var prop in propertyInfo)
            {
                if (prop.Name == "ID") continue;
                else fields.Add(prop.Name);
            }
            return fields;
        }

        private static string GetRowINSERT<T>(Type type, List<string> fields)
        {
            string comm = $"INSERT INTO {type.Name.ToLower()} (";

            for (int i = 0; i < fields.Count; i++)
            {
                if (i != 0)
                    comm += ", ";
                comm += $"{fields[i]}";
            }

            comm += ") VALUES (";

            for (int i = 0; i < fields.Count; i++)
            {
                if (i != 0)
                    comm += ", ";
                comm += $"@{fields[i]}";
            }

            comm += ");";
            return comm;
        }

        internal static ObservableCollection<IStorage> GetIStorages(int numberParent)
        {
            ObservableCollection<IStorage> result = new ObservableCollection<IStorage>();

            using (SQLiteConnection conn = new SQLiteConnection(stringConnection))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    SQLiteCommand command = new SQLiteCommand(conn);
                    command.Transaction = transaction;

                    command.CommandText = $"SELECT * FROM folderm WHERE parentfolderid={numberParent}"; ;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FolderM folderM = new FolderM()
                            {
                                ID = Convert.ToInt32(reader[nameof(FolderM.ID).ToString()]),
                                Name = reader[nameof(FolderM.Name).ToString()].ToString(),
                                ParentFolderID = Convert.ToInt32(reader[nameof(FolderM.ParentFolderID).ToString()]),
                            };
                            result.Add(folderM);
                        }
                    }

                    command.CommandText = $"SELECT * FROM filem WHERE folderid={numberParent}"; ;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FileM fileM = new FileM()
                            {
                                ID = Convert.ToInt32(reader[nameof(FileM.ID).ToString()]),
                                Name = reader[nameof(FileM.Name).ToString()].ToString(),
                                FolderID = Convert.ToInt32(reader[nameof(FileM.FolderID).ToString()]),
                                Content = reader[nameof(FileM.Content).ToString()].ToString(),
                                Description = reader[nameof(FileM.Description).ToString()].ToString(),
                                FileExtentionID = Convert.ToInt32(reader[nameof(FileM.FileExtentionID).ToString()]),
                            };
                            result.Add(fileM);
                        }
                    }

                    transaction.Commit();
                }
                conn.Close();
            }
            return result;
        }

        internal static ObservableCollection<FolderM> GetFolders(int numberParent)
        {
            ObservableCollection<FolderM> result = new ObservableCollection<FolderM>();

            using (SQLiteConnection conn = new SQLiteConnection(stringConnection))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    SQLiteCommand command = new SQLiteCommand(conn);
                    command.Transaction = transaction;

                    command.CommandText = $"SELECT * FROM folderm WHERE parentfolderid={numberParent}"; ;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FolderM folderM = new FolderM()
                            {
                                ID = Convert.ToInt32(reader[nameof(FolderM.ID).ToString()]),
                                Name = reader[nameof(FolderM.Name).ToString()].ToString(),
                                ParentFolderID = Convert.ToInt32(reader[nameof(FolderM.ParentFolderID).ToString()]),
                            };
                            result.Add(folderM);
                        }
                    }

                    transaction.Commit();
                }
                conn.Close();
            }
            return result;
        }

        internal static ObservableCollection<FileM> GetFiles(int numberParent)
        {
            ObservableCollection<FileM> result = new ObservableCollection<FileM>();

            using (SQLiteConnection conn = new SQLiteConnection(stringConnection))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    SQLiteCommand command = new SQLiteCommand(conn);
                    command.Transaction = transaction;

                    command.CommandText = $"SELECT * FROM filem WHERE folderid={numberParent}"; ;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FileM fileM = new FileM()
                            {
                                ID = Convert.ToInt32(reader[nameof(FileM.ID).ToString()]),
                                Name = reader[nameof(FileM.Name).ToString()].ToString(),
                                FolderID = Convert.ToInt32(reader[nameof(FileM.FolderID).ToString()]),
                                Content = reader[nameof(FileM.Content).ToString()].ToString(),
                                Description = reader[nameof(FileM.Description).ToString()].ToString(),
                                FileExtentionID = Convert.ToInt32(reader[nameof(FileM.FileExtentionID).ToString()]),
                            };
                            result.Add(fileM);
                        }
                    }
                    transaction.Commit();
                }
                conn.Close();
            }
            return result;
        }


        internal static ObservableCollection<FileExtensionM> GetExtentions()
        {
            ObservableCollection<FileExtensionM> result = new ObservableCollection<FileExtensionM>();

            using (SQLiteConnection conn = new SQLiteConnection(stringConnection))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    SQLiteCommand command = new SQLiteCommand(conn);
                    command.Transaction = transaction;

                    command.CommandText = $"SELECT * FROM fileextensionm; "; ;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FileExtensionM extentionM = new FileExtensionM()
                            {
                                ID = Convert.ToInt32(reader[nameof(FileExtensionM.ID).ToString()]),
                                TypeFile = reader[nameof(FileExtensionM.TypeFile).ToString()].ToString(),
                                Icon = reader[nameof(FileExtensionM.Icon).ToString()].ToString(),
                            };
                            result.Add(extentionM);
                        }
                    }
                    transaction.Commit();
                }
                conn.Close();
            }
            return result;
        }


        internal static void SaveToDB<T>(T obj)
        {
            InsertRow(obj);
        }

    }

    //Для указания атрибутов у класса для данных (создание DB), чтобы вручную не перебирать
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
        public string Text { get; } = "PRIMARY KEY";
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class NotNullAttribute : Attribute
    {
        public string Text { get; } = "NOT NULL";
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueAttribute : Attribute
    {
        public string Text { get; } = "UNIQUE";
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute
    {
        public Type TypeRef { get; private set; }
        public string Text { get; } = "FOREIGN KEY";

        public ForeignKeyAttribute(Type type)
        {
            this.TypeRef = type;
        }
    }

}
