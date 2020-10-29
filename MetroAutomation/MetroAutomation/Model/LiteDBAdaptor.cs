using LiteDB;
using MetroAutomation.Calibration;
using System.IO;
using System.Linq;

namespace MetroAutomation.Model
{
    public static class LiteDBAdaptor
    {
        private const string DefaultPath = "Calibrations.db";
        private static readonly string DataBasePath;

        static LiteDBAdaptor()
        {
            DataBasePath = DefaultPath;
#if DEBUG
            string path = "../../../Calibrations.db";
#else
            string path = "Calibrations.db";
#endif

            if (string.IsNullOrWhiteSpace(path))
            {
                path = DefaultPath;
            }
            else
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(Path.GetFileName(path)))
                    {
                        path = Path.Combine(path, DefaultPath);
                    }
                }
                catch
                {
                    path = DefaultPath;
                }
            }

            DataBasePath = path;
        }

        public static void SaveData<T>(T data) where T : IDataObject
        {
            using var db = new LiteDatabase(DataBasePath);
            var dataCollection = db.GetCollection<T>();

            dataCollection.Upsert(data);
        }

        public static void RemoveData<T>(int id) where T : IDataObject
        {
            using var db = new LiteDatabase(DataBasePath);
            var dataCollection = db.GetCollection<T>();
            dataCollection.DeleteMany(x => x.ID == id);
        }

        public static void RemoveData<T>(T data) where T : IDataObject
        {
            RemoveData<T>(data.ID);
        }

        public static T LoadData<T>(int id) where T : IDataObject, new()
        {
            using var db = new LiteDatabase(DataBasePath);
            var dataCollection = db.GetCollection<T>();
            try
            {
                return dataCollection.FindOne(x => x.ID == id) ?? new T();
            }
            catch
            {
                return new T();
            }
        }

        public static NameID[] GetNames<T>() where T : IDataObject
        {
            using var db = new LiteDatabase(DataBasePath);
            var dataCollection = db.GetCollection<T>();
            return dataCollection.Query().Select(x => new NameID { Name = x.Name, ID = x.ID }).ToArray();
        }

        public static NameID[] GetStandardNames()
        {
            using var db = new LiteDatabase(DataBasePath);
            var dataCollection = db.GetCollection<DeviceConfiguration>();
            return dataCollection.Query().Where(x => x.IsStandard)
                .Select(x => new NameID { Name = x.Name, ID = x.ID }).ToArray();
        }


        public static NameID[] GetPairedStandardNames(Mode pairedMode)
        {
            using var db = new LiteDatabase(DataBasePath);
            var dataCollection = db.GetCollection<DeviceConfiguration>();

            return dataCollection.Query().Where(x => x.IsStandard && x.ModeInfo != null && x.ModeInfo.Count(y => y.Mode == pairedMode) > 0)
                .Select(x => new NameID { Name = x.Name, ID = x.ID }).ToArray();
        }

        public static T[] LoadAll<T>() where T : IDataObject
        {
            using var db = new LiteDatabase(DataBasePath);
            var dataCollection = db.GetCollection<T>();
            return dataCollection.Query().ToArray();
        }

        public static void ClearAll<T>() where T : IDataObject
        {
            using var db = new LiteDatabase(DataBasePath);
            var dataCollection = db.GetCollection<T>();
            dataCollection.DeleteAll();
        }

        //public static CalibrationData[] SearchCalibrationData(int maxCount, string searchQuery)
        //{
        //    using var db = new LiteDatabase(DataBasePath);
        //    var dataCollection = db.GetCollection<CalibrationData>();

        //    dataCollection.EnsureIndex(x => x.WorkInfo.DeviceOwner);
        //    dataCollection.EnsureIndex(x => x.WorkInfo.Metrologist);
        //    dataCollection.EnsureIndex(x => x.WorkInfo.WorkNumber);
        //    dataCollection.EnsureIndex(x => x.DeviceInfo.Grsi);
        //    dataCollection.EnsureIndex(x => x.DeviceInfo.Name);
        //    dataCollection.EnsureIndex(x => x.DeviceInfo.Type);
        //    dataCollection.EnsureIndex(x => x.DeviceInfo.Modification);
        //    dataCollection.EnsureIndex(x => x.DeviceInfo.SerialNumber);

        //    var query = dataCollection.Query().OrderByDescending(x => x.ID);

        //    if (!string.IsNullOrEmpty(searchQuery))
        //    {
        //        query.Where(x =>
        //            x.WorkInfo.DeviceOwner.Contains(searchQuery)
        //            || x.WorkInfo.Metrologist.Contains(searchQuery)
        //            || x.WorkInfo.WorkNumber.Contains(searchQuery)
        //            || x.DeviceInfo.Grsi.Contains(searchQuery)
        //            || x.DeviceInfo.Name.Contains(searchQuery)
        //            || x.DeviceInfo.Type.Contains(searchQuery)
        //            || x.DeviceInfo.Modification.Contains(searchQuery)
        //            || x.DeviceInfo.SerialNumber.Contains(searchQuery));
        //    }

        //    if (maxCount >= 0)
        //    {
        //        query.Limit(maxCount);
        //    }

        //    return query.ToEnumerable().OrderBy(x => x.ID).ToArray();
        //}

        //public static CalibrationDataCliche[] SearchCalibrationDataCliche(int maxCount, string searchQuery)
        //{
        //    using var db = new LiteDatabase(DataBasePath);
        //    var dataCollection = db.GetCollection<CalibrationDataCliche>();

        //    dataCollection.EnsureIndex(x => x.DeviceInfo.Grsi);
        //    dataCollection.EnsureIndex(x => x.DeviceInfo.Name);
        //    dataCollection.EnsureIndex(x => x.DeviceInfo.Type);
        //    dataCollection.EnsureIndex(x => x.DeviceInfo.Modification);

        //    var query = dataCollection.Query().OrderByDescending(x => x.ID);

        //    if (!string.IsNullOrEmpty(searchQuery))
        //    {
        //        query.Where(x => x.DeviceInfo.Grsi.Contains(searchQuery)
        //            || x.DeviceInfo.Name.Contains(searchQuery)
        //            || x.DeviceInfo.Type.Contains(searchQuery)
        //            || x.DeviceInfo.Modification.Contains(searchQuery));
        //    }

        //    if (maxCount >= 0)
        //    {
        //        query.Limit(maxCount);
        //    }

        //    return query.ToEnumerable().OrderBy(x => x.ID).ToArray();
        //}
    }
}
