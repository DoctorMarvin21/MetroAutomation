using LiteDB;
using MetroAutomation.Automation;
using MetroAutomation.Calibration;
using MetroAutomation.FrontPanel;
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

            // Find more advanced way to do that, now I'm struggling issues with sub-queries

            return dataCollection.Query()
                .Where(x => x.IsStandard)
                .ToEnumerable()
                .Where(x => x.ModeInfo != null && x.ModeInfo.Count(y => y.Mode == pairedMode) > 0)
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

        public static bool CanRemoveCommandSet(int id)
        {
            using var db = new LiteDatabase(DataBasePath);
            var dataCollection = db.GetCollection<DeviceConfiguration>();
            return dataCollection.Count(x => x.CommandSetID == id) == 0;
        }

        public static bool CanRemoveDeviceConfiguration(int id)
        {
            using var db = new LiteDatabase(DataBasePath);

            // Find more advanced way to do that, now I'm struggling issues with sub-queries

            var notInFrontPanels = db.GetCollection<FrontPanels>()
                .Query().ToEnumerable()
                .Count(x => x.ConfigurationFrontPanels != null && x.ConfigurationFrontPanels.Count(y => y != null && y.ConfigurationID == id) > 0) == 0;

            var notUsedInValueSets = db.GetCollection<FrontPanelValueSet>()
                .Query().ToEnumerable()
                .Count(x => x.Values != null && x.Values.Count(y => y != null && y.ConfigurationID == id) > 0) == 0;

            var notUsedInProtocols = db.GetCollection<DeviceProtocol>()
                .Query().ToEnumerable()
                .Count(x => x.ConfigurationID == id || (x.Blocks != null && x.Blocks.Count(y => y != null && y.StandardConfigurationIDs.Contains(id)) > 0)) == 0;

            var notUsedInCliche = db.GetCollection<DeviceProtocolCliche>()
                .Query().ToEnumerable()
                .Count(x => x.ConfigurationID == id || (x.Blocks != null && x.Blocks.Count(y => y != null && y.StandardConfigurationIDs.Contains(id)) > 0)) == 0;

            return notInFrontPanels && notUsedInValueSets && notUsedInProtocols && notUsedInCliche;
        }

        public static DeviceProtocolDisplayed[] SearchProtocol(int maxCount, string searchQuery)
        {
            using var db = new LiteDatabase(DataBasePath);
            var dataCollection = db.GetCollection<DeviceProtocol>();

            dataCollection.EnsureIndex(x => x.AccountInfo);
            dataCollection.EnsureIndex(x => x.ProtocolNumber);
            dataCollection.EnsureIndex(x => x.Name);
            dataCollection.EnsureIndex(x => x.Type);
            dataCollection.EnsureIndex(x => x.Grsi);
            dataCollection.EnsureIndex(x => x.SerialNumber);
            dataCollection.EnsureIndex(x => x.DeviceOwner);

            var query = dataCollection.Query().OrderByDescending(x => x.ID);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query.Where(x =>
                    x.AccountInfo.StartsWith(searchQuery)
                    || x.ProtocolNumber.StartsWith(searchQuery)
                    || x.Grsi.StartsWith(searchQuery)
                    || x.SerialNumber.StartsWith(searchQuery)
                    || x.Name.Contains(searchQuery)
                    || x.Type.Contains(searchQuery)
                    || x.DeviceOwner.Contains(searchQuery));
            }

            if (maxCount >= 0)
            {
                query.Limit(maxCount);
            }

            return query
                .Select(x =>
                    new DeviceProtocolDisplayed
                    {
                        ID = x.ID,
                        DeviceOwner = x.DeviceOwner,
                        Grsi = x.Grsi,
                        Name = x.Name,
                        AccountInfo = x.AccountInfo,
                        ProtocolNumber = x.ProtocolNumber,
                        Type = x.Type,
                        SerialNumber = x.SerialNumber,
                        CalibrationDate = x.CalibrationDate,
                        WorkStatus = x.WorkStatus
                    })
                .ToEnumerable()
                .OrderBy(x => x.ID)
                .ToArray();
        }

        public static DeviceProtocolClicheDisplayed[] SearchProtocolCliche(int maxCount, string searchQuery)
        {
            using var db = new LiteDatabase(DataBasePath);
            var dataCollection = db.GetCollection<DeviceProtocolCliche>();

            dataCollection.EnsureIndex(x => x.Grsi);
            dataCollection.EnsureIndex(x => x.Name);
            dataCollection.EnsureIndex(x => x.Type);

            var query = dataCollection.Query().OrderByDescending(x => x.ID);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query.Where(x => x.Grsi.StartsWith(searchQuery)
                    || x.Name.Contains(searchQuery)
                    || x.Type.Contains(searchQuery));
            }

            if (maxCount >= 0)
            {
                query.Limit(maxCount);
            }

            return query
                .Select(x => new DeviceProtocolClicheDisplayed
                    {
                        ID = x.ID,
                        Name = x.Name,
                        Type = x.Type,
                        Grsi = x.Grsi,
                        Comment = x.Comment
                    })
                .ToEnumerable().OrderBy(x => x.ID).ToArray();
        }
    }
}
